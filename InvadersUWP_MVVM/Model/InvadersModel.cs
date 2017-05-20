using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace InvadersUWP_MVVM.Model
{
    class InvadersModel
    {
        public readonly static Size PlayAreaSize = new Size(400, 300);
        public const int MaximumPlayerShots = 3;
        public const int InitialStarCount = 50;

        private readonly Random _random = new Random();

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int Lives { get; private set; }

        public bool GameOver { get; private set; }

        private DateTime? _playerDied = null;
        public bool PlayerDying { get { return _playerDied.HasValue; } }

        private Player _player;

        private readonly List<Invader> _invaders = new List<Invader>();
        private readonly List<Shot> _playerShots = new List<Shot>();
        private readonly List<Shot> _invaderShots = new List<Shot>();
        private readonly List<Point> _stars = new List<Point>();

        private Enums.Direction _invaderDirection = Enums.Direction.Left;
        private bool _justMovedDown = false;

        private DateTime _lastUpdated = DateTime.MinValue;

        public InvadersModel()
        {
            EndGame();
        }

        public void EndGame()
        {
            GameOver = true;
        }

        public void StartGame()
        {
            GameOver = false;
            Score = 0;

            foreach (Invader invader in _invaders)
                OnShipChanged(invader, true);
            _invaders.Clear();

            foreach (Shot shot in _playerShots)
                OnShotMoved(shot, true);
            _playerShots.Clear();
            _invaderShots.Clear();

            foreach (Point star in _stars)
                OnStarChanged(star, true);
            _stars.Clear();
            for (int i = 0; i < InitialStarCount; i++)
                AddAStar();

            _player = new Player();
            OnShipChanged(_player, false);

            Lives = 2;
            Wave = 0;
            NextWave();
        }

        private void AddAStar()
        {
            Point point = new Point(_random.Next((int)PlayAreaSize.Width), _random.Next(20, (int)PlayAreaSize.Height) - 20);
            if (!_stars.Contains(point))
            {
                _stars.Add(point);
                OnStarChanged(point, false);
            }
        }

        public void FireShot()
        {
            if (GameOver || PlayerDying || _lastUpdated == DateTime.MinValue)
                return;

            if (_playerShots.Count < MaximumPlayerShots)
            {
                Shot shotFired = new Shot(new Point(_player.Location.X + (_player.Size.Width / 2) - 1, _player.Location.Y),
                    Enums.Direction.Up);
                _playerShots.Add(shotFired);
                OnShotMoved(shotFired, false);
            }
        }

        public void MovePlayer(Enums.Direction direction)
        {
            if (_playerDied.HasValue)
                return;
            _player.Move(direction);
            OnShipChanged(_player, false);
        }

        public void Twinkle()
        {
            if ((_random.Next(2) == 0) && _stars.Count > ((int)InitialStarCount * .75))
                RemoveAStar();
            else if (_stars.Count < ((int)InitialStarCount * 1.5))
                AddAStar();
        }

        private void RemoveAStar()
        {
            if (_stars.Count <= 0) return;
            int starIndex = _random.Next(_stars.Count);
            OnStarChanged(_stars[starIndex], true);
            _stars.RemoveAt(starIndex);
        }

        public void Update(bool paused)
        {
            if (!paused)
            {
                if (_invaders.Count == 0)
                    NextWave();

                if (!_playerDied.HasValue)
                {
                    MoveInvaders();
                    MoveShots();
                    ReturnFire();
                    CheckForInvaderCollisions();
                    CheckForPlayerCollisions();
                }
                else if (_playerDied.HasValue && (DateTime.Now - _playerDied > TimeSpan.FromSeconds(2.5)))
                {
                    _playerDied = null;
                    OnShipChanged(_player, false);
                }
            }
            Twinkle();
        }

        private void MoveShots()
        {
            List<Shot> playerShots = _playerShots.ToList();
            foreach (Shot shot in playerShots)
            {
                shot.Move();
                OnShotMoved(shot, false);
                if (shot.Location.Y < 0)
                {
                    _playerShots.Remove(shot);
                    OnShotMoved(shot, true);
                }
            }

            List<Shot> invaderShots = _invaderShots.ToList();
            foreach (Shot shot in invaderShots)
            {
                shot.Move();
                OnShotMoved(shot, false);
                if (shot.Location.Y > PlayAreaSize.Height)
                {
                    _invaderShots.Remove(shot);
                    OnShotMoved(shot, true);
                }
            }
        }

        private void NextWave()
        {
            Wave++;
            _invaders.Clear();
            RemoveAllShots();
            for (int row = 0; row < 5; row++)
                for(int column = 0; column < 11; column++)
                {
                    Point location = new Point(column * Invader.InvaderSize.Width * 1.4, row * Invader.InvaderSize.Height * 1.4);
                    Invader newInvader;
                    switch(row)
                    {
                        case 0:
                            newInvader = new Invader(Enums.InvaderType.Spaceship, location, 50);
                            break;
                        case 1:
                            newInvader = new Invader(Enums.InvaderType.Bug, location, 40);
                            break;
                        case 2:
                            newInvader = new Invader(Enums.InvaderType.Saucer, location, 30);
                            break;
                        case 3:
                            newInvader = new Invader(Enums.InvaderType.Satellite, location, 20);
                            break;
                        default:
                            newInvader = new Invader(Enums.InvaderType.Star, location, 10);
                            break;
                    }
                    _invaders.Add(newInvader);
                    OnShipChanged(newInvader, false);
            }
        }

        private static bool RectsOverlap(Rect r1, Rect r2)
        {
            r1.Intersect(r2);
            if (r1.Width > 0 || r1.Height > 0)
                return true;
            return false;
        }

        private void CheckForPlayerCollisions()
        {
            List<Shot> invaderShots = _invaderShots.ToList();

            foreach (Shot shot in invaderShots)
            {
                Rect shotRect = new Rect(shot.Location.X, shot.Location.Y, Shot.ShotSize.Width,
                    Shot.ShotSize.Height);
                if (RectsOverlap(_player.Area, shotRect))
                {
                    if (Lives == 0)
                        EndGame();
                    else
                    {
                        _invaderShots.Remove(shot);
                        OnShotMoved(shot, true);
                        _playerDied = DateTime.Now;
                        OnShipChanged(_player, true);
                        RemoveAllShots();
                        Lives--;
                    }
                }
            }

            var invadersReachedBottom =
            from invader in _invaders
            where invader.Area.Bottom > _player.Area.Top
            select invader;

            if (invadersReachedBottom.Count() > 0)
                EndGame();
        }

        private void RemoveAllShots()
        {
            List<Shot> invaderShots = _invaderShots.ToList();
            List<Shot> playerShots = _playerShots.ToList();

            foreach (Shot shot in invaderShots)
                OnShotMoved(shot, true);

            foreach (Shot shot in playerShots)
                OnShotMoved(shot, true);

            _invaderShots.Clear();
            _playerShots.Clear();
        }

        private void CheckForInvaderCollisions()
        {
            List<Shot> shotsHit = new List<Shot>();
            List<Invader> invadersKilled = new List<Invader>();
            foreach (Shot shot in _playerShots)
            {
                var result = from invader in _invaders
                             where invader.Area.Contains(shot.Location) == true && shot.Direction == Enums.Direction.Up
                             select new { InvaderKilled = invader, ShotHit = shot };
                if (result.Count() > 0)
                {
                    foreach (var o in result)
                    {
                        shotsHit.Add(o.ShotHit);
                        invadersKilled.Add(o.InvaderKilled);
                    }
                }
            }
            foreach (Invader invader in invadersKilled)
            {
                Score += invader.Score;
                _invaders.Remove(invader);
                OnShipChanged(invader, true);
            }
            foreach (Shot shot in shotsHit)
            {
                _playerShots.Remove(shot);
                OnShotMoved(shot, true);
            }
        }

        private void MoveInvaders()
        {
            double millisecondsBetweenMovements = Math.Min(10 - Wave, 1) * (2 * _invaders.Count());
            if (DateTime.Now - _lastUpdated > TimeSpan.FromMilliseconds(millisecondsBetweenMovements))
            {
                _lastUpdated = DateTime.Now;

                var invadersTouchingLeftBoundary = from invader in _invaders where invader.Area.Left < 5 select invader;
                var invadersTouchingRightBoundary = from invader in _invaders where invader.Area.Right > PlayAreaSize.Width - (5 * 2) select invader;

                if (!_justMovedDown)
                {
                    if (invadersTouchingLeftBoundary.Count() > 0)
                    {
                        foreach (Invader invader in _invaders)
                        {
                            invader.Move(Enums.Direction.Down);
                            OnShipChanged(invader, false);
                        }
                        _invaderDirection = Enums.Direction.Right;
                    }
                    else if (invadersTouchingRightBoundary.Count() > 0)
                    {
                        foreach (Invader invader in _invaders)
                        {
                            invader.Move(Enums.Direction.Down);
                            OnShipChanged(invader, false);
                        }
                        _invaderDirection = Enums.Direction.Left;
                    }
                    _justMovedDown = true;
                }
                else
                {
                    _justMovedDown = false;
                    foreach (Invader invader in _invaders)
                    {
                        invader.Move(_invaderDirection);
                        OnShipChanged(invader, false);
                    }
                }
            }
        }

        private void ReturnFire()
        {
            if (_invaderShots.Count() > Wave + 1 || _random.Next(10) < 10 - Wave)
                return;

            var invaderColumns =
                from invader in _invaders
                group invader by invader.Location.X
                    into invaderGroup
                orderby invaderGroup.Key descending
                select invaderGroup;

            var randomGroup = invaderColumns.ElementAt(_random.Next(invaderColumns.Count()));
            var shooter = randomGroup.Last();

            Point shotLocation = new Point(shooter.Area.X + (shooter.Size.Width / 2) - 1, shooter.Area.Bottom);
            Shot invaderShot = new Shot(shotLocation, Enums.Direction.Down);
            _invaderShots.Add(invaderShot);

            OnShotMoved(invaderShot, false);
        }
        internal void UpdateAllShipsAndStars()
        {
            foreach (Shot shot in _playerShots)
                OnShotMoved(shot, false);
            foreach (Invader ship in _invaders)
                OnShipChanged(ship, false);
            OnShipChanged(_player, false);
            foreach (Point star in _stars)
                OnStarChanged(star, false);
        }

        public event EventHandler<ShipChangedEventArgs> ShipChanged;
        private void OnShipChanged(Ship ship, bool killed)
        {
            ShipChanged?.Invoke(this, new ShipChangedEventArgs(ship, killed));
        }

        public event EventHandler<ShotMovedEventArgs> ShotMoved;
        private void OnShotMoved(Shot shot, bool disappeared)
        {
            ShotMoved?.Invoke(this, new ShotMovedEventArgs(shot, disappeared));
        }

        public event EventHandler<StarChangedEventArgs> StarChanged;
        private void OnStarChanged(Point point, bool disappeared)
        {
            StarChanged?.Invoke(this, new StarChangedEventArgs(point, disappeared));
        }
    }
}
