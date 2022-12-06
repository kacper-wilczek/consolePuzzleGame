using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Układanka.Classes
{
    public class GameModel
    {
        public GameModel(bool hardModeOn, int boardSize)
        {
            HardModeOn = hardModeOn;
            BoardSize = boardSize;
            Board = SetUpGameBoard(boardSize);
        }
        public bool HardModeOn { get; init; }
        public int BoardSize { get; init; }
        public int[] Board { get; init; }
        public bool BoardIsInOrder { get => IntArrayIsInAscendingOrder(Board); }
        private int[] SetUpGameBoard(int boardSize)
        {
            int[] intArray = new int[boardSize * boardSize];

            for (int i = 0; i < boardSize * boardSize; i++)
            {
                intArray[i] = i + 1;
            }

            do
            {
                RandomizeArray(ref intArray);
            } while (IntArrayIsInAscendingOrder(intArray));

            return intArray;
        }
        private bool IntArrayIsInAscendingOrder(int[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i - 1] > arr[i])
                {
                    return false;
                }
            }
            return true;
        }
        private void RandomizeArray(ref int[] arr)
        {
            Random randomNumberGenerator = new Random();
            arr = arr.OrderBy(e => randomNumberGenerator.Next()).ToArray();
        }
        public int FetchContentForGivenAddress((int column, int row) address)
        {
            return Board[ConvertAddresToIndex(address)];
        }
        public bool TrySwapTiles((int column, int row) address1, (int column, int row) address2)
        {
            int index1 = ConvertAddresToIndex(address1);
            int index2 = ConvertAddresToIndex(address2);

            if (HardModeOn && !AreAdjacent(index1, index2))
            {
                return false;
            }

            (Board[index2], Board[index1]) = (Board[index1], Board[index2]);
            
            return true;
        }
        private bool AreAdjacent(int index1, int index2)
        {
            return index2 == index1 - 1
                || index2 == index1 + 1
                || index2 == index1 - BoardSize
                || index2 == index1 + BoardSize;
        }
        private int ConvertAddresToIndex((int column, int row) address)
        {
            return (address.row - 1) * BoardSize + (address.column - 1);
        }
    }
}
