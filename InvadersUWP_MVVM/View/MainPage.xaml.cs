using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace InvadersUWP_MVVM
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.StartGame();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlayAreaSize(new Size(e.NewSize.Width, e.NewSize.Height - 160));
        }

        private void Page_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Delta.Translation.X < -1)
                viewModel.LeftGestureStarted();
            else if (e.Delta.Translation.X > 1)
                viewModel.RightGestureStarted();
        }

        private void Page_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            viewModel.LeftGestureCompleted();
            viewModel.RightGestureCompleted();
        }

        private void Page_Tapped(object sender, TappedRoutedEventArgs e)
        {
            viewModel.Tapped();
        }

        private void playArea_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlayAreaSize(playArea.RenderSize);
        }

        private void UpdatePlayAreaSize(Size newPlayAreaSize)
        {
            double targetWidth;
            double targetHeight;
            if(newPlayAreaSize.Width > newPlayAreaSize.Height)
            {
                targetWidth = newPlayAreaSize.Height * 4 / 3;
                targetHeight = newPlayAreaSize.Height;
                double leftRightMargin = (newPlayAreaSize.Width - targetWidth) / 2;
                playArea.Margin = new Thickness(leftRightMargin, 0, leftRightMargin, 0);
            }
            else
            {
                targetHeight = newPlayAreaSize.Width * 3 / 4;
                targetWidth = newPlayAreaSize.Width;
                double topBottomMargin = (newPlayAreaSize.Height - targetHeight) / 2;
                playArea.Margin = new Thickness(0, topBottomMargin, 0, topBottomMargin);
            }
            playArea.Width = targetWidth;
            playArea.Height = targetHeight;
            viewModel.PlayAreaSize = playArea.RenderSize;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += KeyDownHandler;
            Window.Current.CoreWindow.KeyUp += KeyUpHandler;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= KeyDownHandler;
            Window.Current.CoreWindow.KeyUp -= KeyUpHandler;
            base.OnNavigatedFrom(e);
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            viewModel.KeyDown(e.VirtualKey);
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            viewModel.KeyUp(e.VirtualKey);
        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (viewModel.Paused == true)
                viewModel.Paused = false;
            else
                viewModel.Paused = true;
        }

        private void AboutButtonClick(object sender, RoutedEventArgs e)
        {
            aboutPopup.IsOpen = true;
            viewModel.Paused = true;
        }

        private void ClosePopupButtonClick(object sender, RoutedEventArgs e)
        {
            aboutPopup.IsOpen = false;
            viewModel.Paused = false;
        }
    }
}
