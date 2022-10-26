using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Układanka.Classes
{
    public class Game
    {
        public GameBoard Board { get; set; }
        public Game(GameBoard board)
        {
            Board = board;
        }
    }
}
