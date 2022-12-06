using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Układanka.Classes
{
    public class GameController
    {
        public GameController(GameModel model, GameView view)
        {
            GameModel = model;
            GameView = view;
        }
        GameModel GameModel { get; init; }
        GameView GameView { get; init; }
        public void MakeAMove(int[] gameBoard, int boardSize, bool hardModeOn)
        {
            GameView.DisplayBoard(GameModel.Board, GameModel.BoardSize);

            // Select first tile
            GameView.DisplayPrompt_SelectFirstTile();
            GameView.TryReceiveAddressFromUserInput(GameModel.BoardSize, out (int column, int row) firstAddress);
            GameView.DisplayBoard(GameModel.Board, GameModel.BoardSize, new HashSet<(int, int)>() { firstAddress });

            (int column, int row) secondAddress;
            bool swapSuccessfull = false; // default value
            do // ensure swap is successfull - selected tiles are adjacent on Hard Mode
            {
                do // ensure different tiles are selected
                {
                    // Select second tile or cancel first tile selection
                    GameView.DisplayPrompt_SelectSecondTileOrCancelFirstTileSelection();
                    if (!GameView.TryReceiveAddressFromUserInput(GameModel.BoardSize, out secondAddress, cancelIsAllowed: true))
                    {
                        return;
                    }

                    if (secondAddress == firstAddress)
                    {
                        GameView.DisplayPrompt_SameTilesSelected();
                        continue;
                    }
                } while (secondAddress == firstAddress);

                GameView.DisplayBoard(GameModel.Board, GameModel.BoardSize, new HashSet<(int, int)>() { firstAddress, secondAddress });

                // Swap selected tiles
                swapSuccessfull = GameModel.TrySwapTiles(firstAddress, secondAddress);
                if (!swapSuccessfull) // On Hard Mode -> swap selection if the second tile was not adjacent to the first tile and continue selecting the second tile
                {
                    firstAddress = secondAddress;
                    GameView.DisplayBoard(GameModel.Board, GameModel.BoardSize, new HashSet<(int, int)>() { firstAddress });
                    GameView.DisplayBoard(GameModel.Board, GameModel.BoardSize, new HashSet<(int, int)>() { firstAddress });
                    GameView.DisplayPrompt_TilesNotAdjacent();
                }
                else
                {
                    GameView.DisplaySwappingAnimation(GameModel.Board, GameModel.BoardSize, firstAddress, secondAddress);
                }
            } while (!swapSuccessfull);
        }
    }
}
