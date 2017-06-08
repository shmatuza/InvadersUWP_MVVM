using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvadersUWP_MVVM.Model
{
    public class Highscore
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int Score { get; set; }
        [Required]
        public string PlayerName { get; set; }
    }
}