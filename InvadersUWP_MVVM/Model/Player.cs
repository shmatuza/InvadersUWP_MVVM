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
        public static readonly Size PlayerSize = new Size(25, 15);
        private const double _speed = 10;

        public Player() : base(new Point(PlayerSize.Width, InvadersModel.PlayAreaSize.Height - InvadersModel.PlayAreaSize.Height * 3), PlayerSize)
        {
            Location = new Point(Location.X, InvadersModel.PlayAreaSize.Height - PlayerSize.Height * 3);
        }

        public override void Move(Enums.Direction direction)
        {
            switch (direction)
            {
                case Enums.Direction.Left:
                    if (Location.X > PlayerSize.Width / 2)
                        Location = new Point(Location.X - 10, Location.Y);
                    break;
                case Enums.Direction.Right:
                    if (Location.X < InvadersModel.PlayAreaSize.Width - PlayerSize.Width * 1.5)
                        Location = new Point(Location.X + 10, Location.Y);
                    break;
                default: break;
            }
        }
    }
}
