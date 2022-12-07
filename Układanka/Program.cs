// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Collections;
using System.Drawing;
using static System.Net.WebRequestMethods;
using System.Text.RegularExpressions;
using Układanka.Classes;
using System.Globalization;

internal class Program
{
    public static int ConvertAddresToIndex((int column, int row) address, int boardSize)
    {
        return (address.row - 1) * boardSize + (address.column - 1);
    }

    private static void Main(string[] args)
    {
        while (true)
        {
            GameView gameView = new();

            gameView.Reset();
            gameView.DisplayPrompt_ExitOrRestartInformation();

            gameView.SetUpGameOptions(out bool hardModeOn, out int boardSize);

            GameModel gameModel = new(hardModeOn, boardSize);
            
            GameController gameController = new(gameModel, gameView);

            while (!gameModel.BoardIsInOrder)
            {
                gameController.MakeAMove();
            }

            gameView.DisplayVictoryAnimationAndMessage(gameModel.Board, gameModel.BoardSize);
            Console.ReadKey(true);
        }
    }
}