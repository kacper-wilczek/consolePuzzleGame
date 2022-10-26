using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Układanka.Classes
{
    public static class AnimationTest
    {
        public static void ShowSimplePercentage()
        {
            for (int i = 0; i <= 100; i++)
            {
                Console.Write($"\rProgress: {i}%   ");
                Thread.Sleep(25);
            }

            Console.Write("\rDone!          \n");
        }
    }
}
