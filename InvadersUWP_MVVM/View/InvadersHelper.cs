using InvadersUWP_MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace InvadersUWP_MVVM.View
{
    class InvadersHelper
    {
        private static readonly Random _random = new Random();

        internal static IEnumerable<string> CreateImageList(Enums.InvaderType invaderType)
        {
            string filename;
            switch(invaderType)
            {
                case Enums.InvaderType.Bug:
                    filename = "bug";
                    break;
                case Enums.InvaderType.Satellite:
                    filename = "satellite";
                    break;
                case Enums.InvaderType.Spaceship:
                    filename = "spaceship";
                    break;
                case Enums.InvaderType.Star:
                default:
                    filename = "star";
                    break;
            }
            List<string> imageList = new List<string>();
            for (int i = 1; i < 5; i++)
                imageList.Add(filename + i + ".png");

            return imageList;
        }

        internal static FrameworkElement InvaderControlFactory(Invader invader, double scale)
        {
            IEnumerable<string> imageList = CreateImageList(invader.InvaderType);
            AnimatedImage invaderControl = new AnimatedImage(imageList, TimeSpan.FromSeconds(.75));
            invaderControl.Width = invader.Size.Width * scale;
            invaderControl.Height = invader.Size.Height * scale;
            SetCanvasLocation(invaderControl, invader.Location.X * scale, invader.Location.Y * scale);

            return invaderControl;
        }

        internal static FrameworkElement ShotControlFactory(Shot shot, double scale)
        {
            Rectangle shotControl = new Rectangle();
            shotControl.Fill = new SolidColorBrush(Colors.Yellow);
            shotControl.Height = Shot.ShotSize.Height * scale;
            shotControl.Width = Shot.ShotSize.Width * scale;
            SetCanvasLocation(shotControl, shot.Location.X * scale, shot.Location.Y * scale);

            return shotControl;
        }

        internal static FrameworkElement PlayerControlFactory(Player player, double scale)
        {
            AnimatedImage playerControl = new AnimatedImage(new List<string> { "player.png" }, TimeSpan.FromSeconds(.75));
            playerControl.Width = player.Size.Width * scale;
            playerControl.Height = player.Size.Height * scale;
            SetCanvasLocation(playerControl, player.Location.X * scale, player.Location.Y * scale);

            return playerControl;
        }

        internal static FrameworkElement StarFactory(Point point, double scale)
        {
            FrameworkElement star;
            switch(_random.Next(3))
            {
                case 0:
                    star = new Rectangle();
                    ((Rectangle)star).Fill = new SolidColorBrush(RandomStarColor());
                    star.Width = 2;
                    star.Height = 2;
                    break;
                case 1:
                    star = new Ellipse();
                    ((Ellipse)star).Fill = new SolidColorBrush(RandomStarColor());
                    star.Width = 2;
                    star.Height = 2;
                    break;
                default:
                    star = new Star();
                    ((Star)star).SetFill(new SolidColorBrush(RandomStarColor()));
                    break;
            }
            SetCanvasLocation(star, point.X * scale, point.Y * scale);
            Canvas.SetZIndex(star, -1000);
            return star;
        }
        private static Color RandomStarColor()
        {
            switch (_random.Next(6))
            {
                case 0:
                    return Colors.White;
                case 1:
                    return Colors.LightBlue;
                case 2:
                    return Colors.MediumPurple;
                case 3:
                    return Colors.PaleVioletRed;
                case 4:
                    return Colors.Yellow;
                default:
                    return Colors.LightSlateGray;
            }
        }

        internal static FrameworkElement ScanLineFactory(int y, int width, double scale)
        {
            FrameworkElement scanLine = new Rectangle();
            scanLine.Width = width * scale;
            scanLine.Height = 2;
            scanLine.Opacity = .5;
            ((Rectangle)scanLine).Fill = new SolidColorBrush(Colors.White);
            SetCanvasLocation(scanLine, 0, y * scale);
            return scanLine;
        }

        public static DoubleAnimation CreateDoubleAnimation(FrameworkElement frameworkElement, double fromX, double toY, 
            string propertyToAnimate)
        {
            DoubleAnimation animation = new DoubleAnimation();
            Storyboard.SetTarget(animation, frameworkElement);
            Storyboard.SetTargetProperty(animation, propertyToAnimate);
            animation.From = fromX;
            animation.To = toY;
            animation.Duration = TimeSpan.FromMilliseconds(25);
            return animation;
        }

        public static void MoveElementOnCanvas(FrameworkElement frameworkElement, double toX, double toY)
        {
            double fromX = Canvas.GetLeft(frameworkElement);
            double fromY = Canvas.GetTop(frameworkElement);

            Storyboard storyboard = new Storyboard();
            DoubleAnimation animationX = CreateDoubleAnimation(frameworkElement, fromX, toX, "(Canvas.Left)");
            DoubleAnimation animationY = CreateDoubleAnimation(frameworkElement, fromY, toY, "(Canvas.Top)");
            storyboard.Children.Add(animationX);
            storyboard.Children.Add(animationY);
            storyboard.Begin();
        }
        public static BitmapImage CreateImageFromAssets(string imageFilename)
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/" + imageFilename));
        }

        public static void SetCanvasLocation(UIElement control, double x, double y)
        {
            Canvas.SetLeft(control, x);
            Canvas.SetTop(control, y);
        }
    }
}
