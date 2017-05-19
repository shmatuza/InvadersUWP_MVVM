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
            if (_playerShots.Count < MaximumPlayerShots)
            {
                Shot newShot = new Shot(_player.Location, Enums.Direction.Up);
                _playerShots.Add(newShot);
                OnShotMoved(newShot, false);
            }
        }

        public void MovePlayer(Enums.Direction direction)
        {
            if (Lives == 0)
                return;
            else
            {
                _player.Move(direction);
                OnShipChanged(_player, false);
            }
        }

        public void Twinkle()
        {
            if (_stars.Count < InitialStarCount * 1.5 && _stars.Count > InitialStarCount * 0.15)
            {
                if (_random.Next(2) == 0)
                {
                    Point newStar = new Point(_random.Next(400), _random.Next(300));
                    _stars.Add(newStar);
                    OnStarChanged(newStar, false);
                }
                else
                {
                    int starToRemoveIndex = _random.Next(_stars.Count);
                    Point removedStar = _stars[starToRemoveIndex];
                    _stars.Remove(removedStar);
                    OnStarChanged(removedStar, true);
                }
            }    
        }

        public void Update(bool paused)
        {
            if (!paused)
            {
                if (GameOver == true)
                    return;
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
            foreach (Shot shot in _playerShots)
            {
                shot.Move();
                OnShotMoved(shot, false);
            }

            var outOfBounds =
                from shot in _playerShots
                where (shot.Location.Y < 10 || shot.Location.Y > PlayAreaSize.Height - 10)
                select shot;

            foreach (Shot shot in outOfBounds.ToList())
            {
                _playerShots.Remove(shot);
                OnShotMoved(shot, true);
            }
        }

        private void NextWave()
        {
            Wave++;
            _invaders.Clear();
            for(int row = 0; row < 5; row++)
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
            var invaderShots = _invaderShots;
            foreach(var invaderShot in invaderShots)
            {
                if (RectsOverlap(_player.Area, invaderShot.Area))
                {
                    Lives--;
                    _invaderShots.Remove(invaderShot);
                    OnShotMoved(invaderShot, true);
                }
            }
        }

        private void CheckForInvaderCollisions()
        {
            var playerShots = _playerShots;
            foreach(var playerShot in playerShots)
            {
                var hittedInvader = from invader in _invaders
                                    where RectsOverlap(invader.Area, playerShot.Area)
                                    select invader;

                foreach (var invader in hittedInvader)
                {
                    _invaders.Remove(invader);
                    OnShipChanged(invader, true);
                    _playerShots.Remove(playerShot); //Might be an error
                    OnShotMoved(playerShot, true);
                }                               
            }

            foreach (var invader in _invaders)
            {
                if (invader.Location.Y <= _player.Location.Y)
                {
                    EndGame();
                    return;
                }
            }
        }

        private void MoveInvaders()
        {
            if (DateTime.Now.Second - _lastUpdated.Second < 2)
                return;

            if (_invaderDirection == Enums.Direction.Right)
            {
                var closestInvader = from invader in _invaders
                                     where invader.Area.Right >= (PlayAreaSize.Width - 2 * Invader.InvaderSize.Width)
                                     select invader;
                if (closestInvader != null && _justMovedDown == false)
                {
                    foreach (var invader in _invaders)
                    {
                        invader.Move(Enums.Direction.Down);
                        OnShipChanged(invader, false);
                    }
                    _invaderDirection = Enums.Direction.Left;
                    _justMovedDown = true;
                }
                else
                {
                    foreach (var invader in _invaders)
                    {
                        invader.Move(Enums.Direction.Right);
                        OnShipChanged(invader, false);
                    }
                        _justMovedDown = false;
                }
            }

            if(_invaderDirection == Enums.Direction.Left)
            {
                var closestInvader = from invader in _invaders
                                     where invader.Area.Left <= (PlayAreaSize.Width - 2 * Invader.InvaderSize.Width)
                                     select invader;
                if(closestInvader != null && _justMovedDown == false)
                {
                    foreach (var invader in _invaders)
                    {
                        invader.Move(Enums.Direction.Down);
                        OnShipChanged(invader, false);
                    }
                    _invaderDirection = Enums.Direction.Right;
                    _justMovedDown = true;
                }
                else
                {
                    foreach (var invader in _invaders)
                    {
                        invader.Move(Enums.Direction.Left);
                        OnShipChanged(invader, false);
                    }
                    _justMovedDown = false;
                }
            }
        }

        private void ReturnFire()
        {
            if (_invaders.Count() == 0) return;

            var invaderShots =
                from Shot shot in _playerShots
                where shot.Direction == Enums.Direction.Down
                select shot;

            if (invaderShots.Count() > Wave + 1 || _random.Next(10) < 10 - Wave)
                return;

            var result =
                from invader in _invaders
                group invader by invader.Area.X into invaderGroup
                orderby invaderGroup.Key descending
                select invaderGroup;

            var randomGroup = result.ElementAt(_random.Next(result.ToList().Count()));
            var bottomInvader = randomGroup.Last();

            Point shotLocation = new Point(bottomInvader.Area.X + bottomInvader.Area.Width / 2, bottomInvader.Area.Bottom + 2);
            Shot invaderShot = new Shot(shotLocation, Enums.Direction.Down);
            _playerShots.Add(invaderShot);
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
            EventHandler<ShipChangedEventArgs> shipChanged = ShipChanged;
            if (shipChanged != null)
                shipChanged(this, new ShipChangedEventArgs(ship, killed));
        }

        public event EventHandler<ShotMovedEventArgs> ShotMoved;
        private void OnShotMoved(Shot shot, bool disappeared)
        {
            EventHandler<ShotMovedEventArgs> shotMoved = ShotMoved;
            if (shotMoved != null)
                shotMoved(this, new ShotMovedEventArgs(shot, disappeared));
        }

        public event EventHandler<StarChangedEventArgs> StarChanged;
        private void OnStarChanged(Point point, bool disappeared)
        {
            EventHandler<StarChangedEventArgs> starChanged = StarChanged;
            if (starChanged != null)
                starChanged(this, new StarChangedEventArgs(point, disappeared));
        }
    }
}
