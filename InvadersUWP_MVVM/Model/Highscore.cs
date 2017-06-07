using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvadersUWP_MVVM.Model
{
    public class Highscore : IComparable<Highscore>
    {
        public int Score { get; private set; }
        public string PlayerName { get; private set; }

        public Highscore(int score, string playerName)
        {
            Score = score;
            PlayerName = playerName;
        }

        public int CompareTo(Highscore other)
        {
            if (this.Score == other.Score) return 0;
            return this.Score.CompareTo(other.Score);
        }
    }
}
