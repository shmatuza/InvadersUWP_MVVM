using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace InvadersUWP_MVVM.View
{
    class InvadersHelper
    {
        public static void StarFactory(int random)
        {
            
        }

        private Color RandomStarColor()
        {
            Random rnd = new Random();
            Byte[] b = new Byte[4];
            rnd.NextBytes(b);
            Color color = Color.FromArgb(b[0], b[1], b[2], b[3]);
            return color;
        }

        internal static BitmapImage CreateImageFromAssets(string imageFilename)
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/" + imageFilename));
        }
    }
}
