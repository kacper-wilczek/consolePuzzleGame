// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Układanka.Classes;
using Układanka.Interfaces;

while (true)
{
    Console.Clear();
    Console.WriteLine("Napisz \"exit\" w dowolonym momencie aby zakończyć program.\n");
    
    IUserInput userInput = new UserInput();

    Console.WriteLine("Wybierz tryb gry:");
    Console.WriteLine("1. Łatwy - kolejność kafelków można zamieniać dowolnie");
    Console.WriteLine("2. Trudny - można zmieniać kolejność tylko kafelków znajdujących się obok siebie");
    Console.WriteLine();

    bool hardModeOn = false;
    bool gameModeChosen = false;
    while (!gameModeChosen)
    {
        string input = userInput.GetUserInput();
        switch (input)
        {
            case "1":
                hardModeOn = false;
                gameModeChosen = true;
                Console.WriteLine("\nWybrano tryb łatwy.\n");
                break;
            case "2":
                hardModeOn = true;
                gameModeChosen = true;
                Console.WriteLine("\nWybrano tryb trudny.\n");
                break;
            default:
                Console.WriteLine("Wybierz tryb gry!");
                break;
        }
    }

    GameBoard gameBoard = new GameBoard(userInput, sizeLimits: (2, 26), hardModeOn);
    Game game = new Game(gameBoard);

    do
    {
        gameBoard.AssignRandomPositions();
    } while (gameBoard.BoardIsInOrder());

    gameBoard.DisplayBoard();

    while (!gameBoard.BoardIsInOrder())
    {
        gameBoard.AskPlayerForMove();
    }
    gameBoard.DisplayVictoryAnimation();
    Console.WriteLine("Gratulacje! Wygrałeś.\n");
    Console.ReadLine();
}