using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace InvadersUWP_MVVM.Model
{
    class Invader : Ship
    {
        public static Size InvaderSize { get; private set; } 
        public Enums.InvaderType InvaderType { get; private set; }
        public int Score { get; private set; }


        public Invader(Point location, Size size, Enums.InvaderType invaderType) : base(location, size)
        {
            Location = location;
            InvaderSize = size;
            InvaderType = invaderType;
        }

        public Invader(Point location, Size size, Enums.InvaderType invaderType, int score) : base(location, size)
        {
            InvaderType = invaderType;
            Score = score;
        }

        public override void Move(Enums.Direction direction) //MEthod to check
        {
            switch (direction)
            {
                case Enums.Direction.Left:
                    if (Location.X <= InvadersModel.PlayAreaSize.Width - InvadersModel.PlayAreaSize.Width + 10)
                        return;
                    Location = new Point(Location.X - 10, Location.Y);
                    break;

                case Enums.Direction.Right:
                    if (Location.X >= InvadersModel.PlayAreaSize.Width - 10)
                        return;
                    Location = new Point(Location.X + 10, Location.Y);
                    break;

                case Enums.Direction.Down:
                    if (Location.Y >= InvadersModel.PlayAreaSize.Height - 10)
                        return;
                    Location = new Point(Location.X, Location.Y + 10);
                    break;
            }
        }
    }
}
