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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace InvadersUWP_MVVM.View
{
    class InvadersHelper
    {
        private readonly Random _random = new Random();
        public static FrameworkElement StarFactory(Point point, double scale) //TODO
        {
            FrameworkElement star;
            switch(_random.Next(3))
            {

            }
        }

        private Color RandomStarColor()
        {
            Random rnd = new Random();
            Byte[] b = new Byte[4];
            rnd.NextBytes(b);
            Color color = Color.FromArgb(b[0], b[1], b[2], b[3]);
            return color;
        }

        public static FrameworkElement ScanLineFactory(int y, int width, double scale)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Fill = new SolidColorBrush(Colors.White);
            rectangle.Height = 2;
            rectangle.Opacity = .1;
            rectangle.Width = width * scale;
            SetCanvasLocation(rectangle, 0, y * scale);
            return rectangle;
        }

        internal static BitmapImage CreateImageFromAssets(string imageFilename)
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
