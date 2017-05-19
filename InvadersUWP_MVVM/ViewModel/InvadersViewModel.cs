using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvadersUWP_MVVM.View;
using InvadersUWP_MVVM.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.Foundation;
using DispatcherTimer = Windows.UI.Xaml.DispatcherTimer;
using FrameworkElement = Windows.UI.Xaml.FrameworkElement;
using System.ComponentModel;

namespace InvadersUWP_MVVM.ViewModel
{
    public class InvadersViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<FrameworkElement> _sprites = new ObservableCollection<FrameworkElement>();
        private readonly ObservableCollection<object> _lives = new ObservableCollection<object>();
        private bool _lastPaused = true;
        private readonly InvadersModel _model = new InvadersModel();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private FrameworkElement _playerControl = null;
        private bool _playerFlashing = false;
        private readonly Dictionary<Invader, FrameworkElement> _invaders = new Dictionary<Invader, FrameworkElement>();
        private readonly Dictionary<FrameworkElement, DateTime> _shotInvaders = new Dictionary<FrameworkElement, DateTime>();
        private readonly Dictionary<Shot, FrameworkElement> _shots = new Dictionary<Shot, FrameworkElement>();
        private readonly Dictionary<Point, FrameworkElement> _stars = new Dictionary<Point, FrameworkElement>();
        private readonly List<FrameworkElement> _scanLines = new List<FrameworkElement>();

        public INotifyCollectionChanged Sprites { get { return _sprites; } }
        public bool GameOver { get { return _model.GameOver; } }
        public INotifyCollectionChanged Lives { get { return _lives; } }
        public bool Paused { get; set; }
        public static double Scale { get; private set; }
        public int Score { get; private set; }
        public Size PlayAreaSize
        {
            set
            {
                Scale = value.Width / 405;
                _model.Update(Paused);
                RecreateScanLines();
            }
        }

        public InvadersViewModel()
        {
            Scale = 1;

            _model.ShipChanged += ModelShipChangedEventHandler;
            _model.ShotMoved += ModelShotMovedEventHandler;
            _model.StarChanged += ModelStarChangedEventHandler;
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += TimerTickEventHandler;

            _model.EndGame();
        }

        void TimerTickEventHandler(object sender, object e)
        {
            if (_lastPaused != Paused)
            {
                _lastPaused = Paused;
                OnPropertyChanged("Paused");
            }
            if (!Paused)
            {
                if (_leftAction.HasValue && _rightAction.HasValue)
                {
                    if (_leftAction.Value > _rightAction.Value)
                        _model.MovePlayer(Enums.Direction.Left);
                    else
                        _model.MovePlayer(Enums.Direction.Right);
                }
                else if (_leftAction.HasValue)
                    _model.MovePlayer(Enums.Direction.Left);
                else if (_rightAction.HasValue)
                    _model.MovePlayer(Enums.Direction.Right);
            }
            _model.Update(Paused);
            if (Score != _model.Score)
            {
                Score = _model.Score;
                OnPropertyChanged("Score");
            }
            if (_lives.Count != _model.Lives)
            {
                _lives.Clear();
                for (int i = 0; i < _model.Lives; i++)
                    _lives.Add(new object());
            }

            foreach (FrameworkElement control in _shotInvaders.Keys.ToList())
            {
                DateTime elapsed = _shotInvaders[control];
                if (DateTime.Now - elapsed > TimeSpan.FromMilliseconds(50))
                {
                    _sprites.Remove(control);
                    _shotInvaders.Remove(control);
                }
            }
            if (_model.GameOver)
            {
                OnPropertyChanged("GameOver");
                _timer.Stop();
            }
        }

        void ModelShipChangedEventHandler(object sender, ShipChangedEventArgs e)
        {
            if (!e.Killed)
            {
                if (e.ShipUpdated is Invader)
                {
                    Invader invader = e.ShipUpdated as Invader;
                    if (!_invaders.ContainsKey(invader))
                    {
                        FrameworkElement invaderControl = InvadersHelper.InvaderControlFactory(invader, Scale);
                        _invaders.Add(invader, invaderControl);
                        _sprites.Add(invaderControl);
                    }
                    else
                    {
                        FrameworkElement invaderControl = _invaders[invader];
                        InvadersHelper.SetCanvasLocation(invaderControl, invader.Location.X, invader.Location.Y);
                        InvadersHelper.ResizeElement(invaderControl, invader.Size.Width * Scale, invader.Size.Height * Scale);
                    }
                }
                else if (e.ShipUpdated is Player)
                {
                    Player player = e.ShipUpdated as Player;
                    if (_playerFlashing)
                    {
                        _playerFlashing = false;
                        AnimatedImage control = _playerControl as AnimatedImage;
                        control.StopFlashing();
                    }
                    if (_playerControl == null)
                    {
                        _playerControl = InvadersHelper.PlayerControlFactory(player, Scale);
                        _sprites.Add(_playerControl);
                    }
                    else
                    {
                        InvadersHelper.MoveElementOnCanvas(_playerControl, player.Location.X * Scale, player.Location.Y * Scale);
                        InvadersHelper.ResizeElement(_playerControl, player.Size.Width * Scale, player.Size.Height * Scale);
                    }
                }
            }
            else
            {
                if (e.ShipUpdated is Invader)
                {
                    Invader invader = e.ShipUpdated as Invader;
                    if (!_invaders.ContainsKey(invader)) return;
                    AnimatedImage invaderControl = _invaders[invader] as AnimatedImage;
                    if (invaderControl != null)
                    {
                        invaderControl.InvaderShot();
                        _shotInvaders.Add(invaderControl, DateTime.Now);
                        _invaders.Remove(invader);
                    }
                }
                else if (e.ShipUpdated is Player)
                {
                    AnimatedImage control = _playerControl as AnimatedImage;
                    if (control != null)
                        control.StartFlashing();
                    _playerFlashing = true;
                }
            }
        }

        void ModelShotMovedEventHandler(object sender, ShotMovedEventArgs e)
        {
            if (!e.Disappeared)
            {
                if (!_shots.ContainsKey(e.Shot))
                {
                    FrameworkElement shotControl = InvadersHelper.ShotControlFactory(e.Shot, Scale);
                    _shots.Add(e.Shot, shotControl);
                    _sprites.Add(shotControl);
                }
                else
                {
                    FrameworkElement shotControl = _shots[e.Shot];
                    InvadersHelper.MoveElementOnCanvas(shotControl, e.Shot.Location.X * Scale, e.Shot.Location.Y * Scale);
                }
            }
            else
            {
                if (_shots.ContainsKey(e.Shot))
                {
                    FrameworkElement shotControl = _shots[e.Shot];
                    _shots.Remove(e.Shot);
                    _sprites.Remove(shotControl);
                }
            }
        }

        void ModelStarChangedEventHandler(object sender, StarChangedEventArgs e)
        {
            if (e.Disappeared && _stars.ContainsKey(e.Point))
            {
                FrameworkElement starControl = _stars[e.Point];
                _sprites.Remove(starControl);
            }
            else
            {
                if (!_stars.ContainsKey(e.Point))
                {
                    FrameworkElement starControl = InvadersHelper.StarFactory(e.Point, Scale);
                    _stars.Add(e.Point, starControl);
                    _sprites.Add(starControl);
                }
                else
                {
                    FrameworkElement starControl = _stars[e.Point];
                    InvadersHelper.MoveElementOnCanvas(starControl, e.Point.X * Scale, e.Point.Y * Scale);
                }
            }
        }

        public void StartGame()
        {
            Paused = false;
            foreach (var invader in _invaders.Values) _sprites.Remove(invader);
            foreach (var shot in _shots.Values) _sprites.Remove(shot);
            _model.StartGame();
            OnPropertyChanged("GameOver");
            _timer.Start();
        }

        private void RecreateScanLines()
        {
            foreach (FrameworkElement scanLine in _scanLines)
                if (_sprites.Contains(scanLine))
                    _sprites.Remove(scanLine);
            _scanLines.Clear();
            for (int y = 0; y < 300; y += 2)
            {
                FrameworkElement scanLine = InvadersHelper.ScanLineFactory(y, 400, Scale);
                _scanLines.Add(scanLine);
                _sprites.Add(scanLine);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Interaction service with user
        private DateTime? _leftAction = null;
        private DateTime? _rightAction = null;

        internal void KeyDown(Windows.System.VirtualKey virtualKey)
        {
            if (virtualKey == Windows.System.VirtualKey.Space)
                _model.FireShot();

            if (virtualKey == Windows.System.VirtualKey.Left)
                _leftAction = DateTime.Now;

            if (virtualKey == Windows.System.VirtualKey.Right)
                _leftAction = DateTime.Now;
        }
        internal void KeyUp(Windows.System.VirtualKey virtualKey)
        {
            if (virtualKey == Windows.System.VirtualKey.Left)
                _leftAction = null;

            if (virtualKey == Windows.System.VirtualKey.Right)
                _rightAction = null;
        }
        internal void LeftGestureStarted()
        {
            _leftAction = DateTime.Now;
        }
        internal void LeftGestureCompleted()
        {
            _leftAction = null;
        }

        internal void RightGestureStarted()
        {
            _rightAction = DateTime.Now;
        }
        internal void RightGestureCompleted()
        {
            _rightAction = null;
        }

        internal void Tapped()
        {
            _model.FireShot();
        }
        #endregion
    }
}
