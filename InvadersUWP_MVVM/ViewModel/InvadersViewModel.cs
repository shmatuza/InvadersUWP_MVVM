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
        private FrameworkElement playerControl = null;
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
                _model.Update();
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

            EndGame();
        }

        void TimerTickEventHandler(object sender, object e)
        {
            if(_lastPaused != Paused)
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
            for (int y = 0; y<300; y+=2)
            {
                FrameworkElement scanLine = InvadersHelper.ScanLineFactory(y, 400, Scale);
                _scanLines.Add(scanLine);
                _sprites.Add(scanLine);
            }
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
