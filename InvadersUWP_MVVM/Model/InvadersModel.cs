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

            var invaders = _invaders.ToList();
            foreach (var invader in invaders)
            {
                OnShipChanged(invader, true);
                _invaders.Remove(invader);
            }

            var playerShots = _playerShots.ToList();
            foreach (var shot in playerShots)
            {
                OnShotMoved(shot, true);
                _playerShots.Remove(shot);
            }

            var invaderShots = _invaderShots.ToList();
            foreach (var shot in invaderShots)
            {
                OnShotMoved(shot, true);
                _invaderShots.Remove(shot);
            }
            var stars = _stars.ToList();
            foreach (var star in stars)
            {
                OnStarChanged(star, true);
                _stars.Remove(star);
            }

            for (int i = 0; i < InitialStarCount; i++)
                _stars.Add(new Point(_random.Next(400), _random.Next(300)));
            foreach (var star in _stars)
                OnStarChanged(star, false);

            _player = new Player(new Point(PlayAreaSize.Height - 250, PlayAreaSize.Width / 2), new Size(25, 15));
            OnShipChanged(_player, false);

            Lives = 2;
            Wave = 0;
            NextWave();
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

        public void Update()
        {
            if (GameOver == true)
                return;
            if (_invaders.Count == 0)
                NextWave();

            MoveInvaders();

            var playerShots = _playerShots.ToList();
            foreach(var shot in playerShots)
            {
                shot.Move();
                if (shot.Location.Y > PlayAreaSize.Height)
                {
                    _playerShots.Remove(shot);
                    OnShotMoved(shot, true);
                }
                else
                    OnShotMoved(shot, false);
            }

            var invaderShots = _invaderShots.ToList();
            foreach(var shot in invaderShots)
            {
                shot.Move();
                if (shot.Location.Y < 0)
                {
                    _invaderShots.Remove(shot);
                    OnShotMoved(shot, true);
                }
                else
                    OnShotMoved(shot, false);
            }
            ReturnFire();
            CheckForPlayerCollisions();
            CheckForInvaderCollisions();
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
