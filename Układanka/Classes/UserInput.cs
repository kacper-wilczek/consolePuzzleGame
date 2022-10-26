using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Układanka.Interfaces;

namespace Układanka.Classes
{
    public class UserInput : IUserInput
    {
        public string GetUserInput()
        {
            string? userInput = Console.ReadLine();

            if (userInput == null)
            {
                return "";
            }

            if (userInput.ToLower() == "exit")
            {
                Environment.Exit(0);
            }

            return userInput;
        }
    }
}
