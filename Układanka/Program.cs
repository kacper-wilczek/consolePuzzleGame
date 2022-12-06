﻿// See https://aka.ms/new-console-template for more information
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

while (true)
{
    GameView gameView = new();

    gameView.SetUpGameOptions(out bool hardModeOn, out int boardSize);

    gameView.Reset();
    gameView.DisplayPrompt_ExitOrRestartInformation();
    
    GameModel gameModel = new(hardModeOn, boardSize);
    GameController gameController = new(gameModel, gameView);

    while (!gameModel.BoardIsInOrder)
    {
        gameController.MakeAMove(gameModel.Board, gameModel.BoardSize, gameModel.HardModeOn);
    }

    gameView.DisplayVictoryAnimationAndMessage(gameModel.Board, gameModel.BoardSize);
    Console.ReadKey(true);
}
