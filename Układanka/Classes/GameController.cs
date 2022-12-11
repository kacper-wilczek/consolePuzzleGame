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
            gameModel = model;
            gameView = view;
        }
        private readonly GameModel gameModel;
        private readonly GameView gameView;
        public void MakeAMove()
        {
            gameView.DisplayBoard(gameModel.Board);

            // Select first tile
            gameView.DisplayPrompt_SelectFirstTile();
            gameView.TryReceiveAddressFromUserInput(out (int column, int row) firstAddress);
            gameView.DisplayBoard(gameModel.Board, new HashSet<(int, int)>() { firstAddress });

            (int column, int row) secondAddress;
            bool swapSuccessful;
            do // ensure swap is successful - selected tiles are adjacent on Hard Mode
            {
                do // ensure different tiles are selected
                {
                    // Select second tile or cancel first tile selection
                    gameView.DisplayPrompt_SelectSecondTileOrCancelFirstTileSelection();
                    if (!gameView.TryReceiveAddressFromUserInput(out secondAddress, cancelIsAllowed: true))
                    {
                        return;
                    }

                    if (secondAddress == firstAddress)
                    {
                        gameView.DisplayPrompt_SameTilesSelected();
                        continue;
                    }
                } while (secondAddress == firstAddress);

                gameView.DisplayBoard(gameModel.Board, new HashSet<(int, int)>() { firstAddress, secondAddress });

                // Swap selected tiles
                swapSuccessful = gameModel.TrySwapTiles(firstAddress, secondAddress);
                if (!swapSuccessful) // On Hard Mode -> swap selection if the second tile was not adjacent to the first tile and continue selecting the second tile
                {
                    firstAddress = secondAddress;
                    gameView.DisplayBoard(gameModel.Board, new HashSet<(int, int)>() { firstAddress });
                    gameView.DisplayBoard(gameModel.Board, new HashSet<(int, int)>() { firstAddress });
                    gameView.DisplayPrompt_TilesNotAdjacent();
                }
                else
                {
                    gameView.DisplaySwappingAnimation(gameModel.Board, firstAddress, secondAddress);
                }
            } while (!swapSuccessful);
        }
    }
}