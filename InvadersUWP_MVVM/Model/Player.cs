using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace InvadersUWP_MVVM.Model
{
    class Player : Ship
    {
        private static Size PlayerSize { get { return new Size(25, 15); } }
        private const double _speed = 10;

        public Player(Point location, Size size) : base(location, size)
        { }

        public override void Move(Enums.Direction direction) //TODO: Check for extending game area size
        {
            switch (direction)
            {
                case Enums.Direction.Left:
                    if (Location.X <= InvadersModel.PlayAreaSize.Width - InvadersModel.PlayAreaSize.Width + 10)
                        return;
                    Location = new Point(Location.X - _speed, Location.Y);
                    break;
                case Enums.Direction.Right:
                    if (Location.X >= InvadersModel.PlayAreaSize.Width - 10)
                        return;
                    Location = new Point(Location.X + _speed, Location.Y);
                    break;
            }
        }
    }
}
