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
        public static readonly Size InvaderSize = new Size(15, 15);
        public Enums.InvaderType InvaderType { get; private set; }
        public int Score { get; private set; }


        public Invader(Enums.InvaderType invaderType, Point location, int score)
            : base(location, Invader.InvaderSize)
        {
            InvaderType = invaderType;
            Score = score;
        }

        public override void Move(Enums.Direction direction)
        {
            switch (direction)
            {
                case Enums.Direction.Left:
                    Location = new Point(Location.X - 5, Location.Y);
                    break;

                case Enums.Direction.Right:
                    Location = new Point(Location.X + 5, Location.Y);
                    break;

                case Enums.Direction.Down:
                    Location = new Point(Location.X, Location.Y + 15);
                    break;
            }
        }
    }
}
