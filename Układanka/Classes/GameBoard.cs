using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Układanka.Interfaces;

namespace Układanka.Classes
{
    public class GameBoard
    {
        private readonly IUserInput _userInput;
        public GameBoard(IUserInput userInput, (int, int) sizeLimits, bool hardMode)
        {
            _userInput = userInput;
            _sizeLimits = sizeLimits;
            _boardSize = SetBoardSize();
            _hardModeOn = hardMode;
            _intArray = GenerateIntArray(_boardSize);
            _columnsArr = GenerateColumnsArr(_boardSize);
            _rowsArr = GenerateRowsArr(_boardSize);
            _tablePositionDict = GenerateTablePositionDict(_boardSize);
            _tilesList = GenerateTiles(_boardSize);
            publicTilesList = _tilesList;
        }

        private readonly (int MinSize, int MaxSize) _sizeLimits;
        private readonly int _boardSize;
        private bool _hardModeOn;
        private readonly List<Tile> _tilesList;
        public readonly List<Tile> publicTilesList; // dla testu
        private bool _boardSizeReadSuccessfully;
        private int[] _intArray;
        private readonly char[] _columnsArr;
        private readonly int[] _rowsArr;
        private readonly Dictionary<(char column, int row), int> _tablePositionDict;
        private int SetBoardSize()
        {
            int readBoardSize = 0;
            while (!_boardSizeReadSuccessfully)
            {
                Console.WriteLine($"Podaj rozmiar planszy. Liczba całkowita od {_sizeLimits.MinSize} do {_sizeLimits.MaxSize}.\n");

                _boardSizeReadSuccessfully = int.TryParse(_userInput.GetUserInput(), out int result);
                readBoardSize = result;
                if (!_boardSizeReadSuccessfully)
                {
                    Console.WriteLine("\nPodaj prawidłowy rozmiar planszy!");
                    Console.WriteLine("\n***\n");
                    continue;
                }

                if (readBoardSize < _sizeLimits.MinSize ^ readBoardSize > _sizeLimits.MaxSize)
                {
                    Console.WriteLine("\nPodaj liczbę całkowitą z prawidłowego przedziału!");
                    _boardSizeReadSuccessfully = false;
                    Console.WriteLine("\n***\n");
                    continue;
                }

            }
            return readBoardSize;
        }
        private List<Tile> GenerateTiles(int size)
        {
            List<Tile> tiles = new List<Tile>();
            if (_boardSize != 0)
            {
                for (int i = 1; i <= size * size; i++)
                {
                    char column = _tablePositionDict.First(kvp => kvp.Value == i).Key.column;
                    int row = _tablePositionDict.First(kvp => kvp.Value == i).Key.row;
                    tiles.Add(new Tile(i, i, column, row));
                }
            }
            else
            {
                Console.WriteLine("Nie można wygenerować kafelków - nie ustalono rozmiaru planszy!");
            }
            return tiles;
        }
        private int[] GenerateIntArray(int size)
        {
            int[] generatedArray = new int[size * size];
            for (int i = 0; i < size * size; i++)
            {
                generatedArray[i] = i + 1;
            }
            return generatedArray;
        }
        private char[] GenerateColumnsArr(int boardSize)
        {
            char[] charArr = Enumerable.Range('A', boardSize).Select(i => (char)i).ToArray();
            return charArr;
        }
        private int[] GenerateRowsArr(int boardSize)
        {
            int[] intArr = new int[boardSize];
            
            for (int i = 0; i < boardSize; i++)
            {
                intArr[i] = i + 1;
            }
            
            return intArr;
        }
        private Dictionary<(char, int), int> GenerateTablePositionDict(int boardSize)
        {
            Dictionary<(char, int), int> tablePositionDict = new Dictionary<(char, int), int>();

            int count = 0;

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    count++;
                    tablePositionDict.Add((_columnsArr[j], _rowsArr[i]), count);
                }
            }

            //Console.WriteLine("Słownik tabla - pozycja: ");

            //foreach (KeyValuePair<(char, int), int> kvp in tablePositionDict)
            //{
            //    Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
            //}

            return tablePositionDict;
        }
        public void AssignRandomPositions()
        {
            Random randomNumberGenerator = new Random();
            _intArray = _intArray.OrderBy(e => randomNumberGenerator.Next()).ToArray();

            //Console.WriteLine("Początkowe pozycje kolejnych kafelków:");

            for (int i = 0; i < _tilesList.Count; i++)
            {
                _tilesList[i].Content = _intArray[i];
                //Console.WriteLine($"Kafelek nr {_tilesList[i].PositionInt} - pozycja: {_tilesList[i].Content}");
            }
        }
        public void DisplayTiles()
        {
            foreach (var item in _tilesList)
            {
                Console.WriteLine(item.Content);
            }
        }
        public bool BoardIsInOrder()
        {
            return !_tilesList.Any(t => t.PositionInt != t.Content);
        }
        public void DisplayBoard()
        {
            Console.Clear();

            int maxRowNumberLength = _rowsArr.Last().ToString().Length;
            int maxTileContentLength = (_boardSize * _boardSize).ToString().Length;

            Console.WriteLine();

            // wypisanie wiersza z nazwami kolumn
            WriteColumns(maxRowNumberLength, maxTileContentLength);
            WriteSeparator(maxRowNumberLength, maxTileContentLength);

            // wypisanie numeru wiersza i dalej zawartości każdego kafelka przez wywołanie metody
            for (int i = 0; i < _rowsArr.Length; i++)
            {
                WriteRowNumber(_rowsArr[i], maxRowNumberLength);

                List<Tile> tilesInCurrentRow = _tilesList.Where(t => t.Row == i + 1).ToList();

                for (int j = 0; j < tilesInCurrentRow.Count; j++)
                {
                    tilesInCurrentRow[j].DisplayTile(maxTileContentLength);
                }

                WriteSeparator(maxRowNumberLength, maxTileContentLength);
            }

            Console.WriteLine("\n");

            void WriteSeparator(int maxRowLength, int maxTileLength)
            {
                Console.WriteLine();

                string rowsColumn = "-";
                rowsColumn = rowsColumn.PadLeft(maxRowLength + 1, '-');
                rowsColumn = rowsColumn.PadRight(rowsColumn.Length + 1, '-');
                Console.Write($"|{rowsColumn}||");

                for (int i = 0; i < _columnsArr.Length; i++)
                {
                    string columnsSeparator = "-";
                    columnsSeparator = columnsSeparator.PadLeft(maxTileLength + 1, '-');
                    columnsSeparator = columnsSeparator.PadRight(columnsSeparator.Length + 1, '-');
                    Console.Write($"{columnsSeparator}|");
                }

                Console.WriteLine("");
            }

            void WriteColumns(int maxRowLength, int maxTileLength)
            {
                string rowsColumn = " ";
                rowsColumn = rowsColumn.PadLeft(maxRowLength + 1);
                rowsColumn = rowsColumn.PadRight(rowsColumn.Length + 1);
                Console.Write($"|{rowsColumn}||");

                foreach (char column in _columnsArr)
                {
                    string columnString = column.ToString();
                    int offset = (maxTileLength - columnString.Length) / 2;

                    columnString = columnString.PadRight(columnString.Length + offset);
                    columnString = columnString.PadLeft(maxTileLength);
                    columnString = columnString.PadRight(columnString.Length + 1);
                    columnString = columnString.PadLeft(columnString.Length + 1);

                    Console.Write($"{columnString}|");
                }
            }

            void WriteRowNumber(int rowNumber, int maxRowLength)
            {
                string rowNumberString = rowNumber.ToString();
                rowNumberString = rowNumberString.PadLeft(maxRowLength + 1);
                rowNumberString = rowNumberString.PadRight(rowNumberString.Length + 1);
                Console.Write($"|{rowNumberString}||");
            }
        }
        public void AskPlayerForMove()
        {
            DisplayBoard();
            
            Console.WriteLine("Wpisz adres wybranego kafelka.\n");
            Tile firstTile = AskPlayerToSelectTile(_userInput, isCancellable: false, out _);
            firstTile.SelectTile();
            DisplayBoard();

            Console.WriteLine("Wpisz adres pozycji, na którą chcesz przesunąć kafelek.\nWpisz \"X\" aby ponownie wybrać pierwszy kafelek.\n");
            string textEnteredByUser;
            Tile secondTile = AskPlayerToSelectTile(_userInput, isCancellable: true, out textEnteredByUser);
            if (textEnteredByUser == "X")
            {
                firstTile.UnselectTile();
                return;
            }

            while (secondTile == firstTile)
            {
                DisplayBoard();
                Console.WriteLine("Wybrano ten sam kafelek!");
                Console.WriteLine("Wpisz adres pozycji, na którą chcesz przesunąć kafelek.\nWpisz \"X\" aby ponownie wybrać pierwszy kafelek.\n");
                secondTile = AskPlayerToSelectTile(_userInput, isCancellable: true,out textEnteredByUser);
                if (textEnteredByUser == "X")
                {
                    firstTile.UnselectTile();
                    return;
                }
            }
            if (_hardModeOn)
            {
                while (!TilesAreAdjacent(firstTile, secondTile))
                {
                    firstTile.UnselectTile();
                    firstTile = secondTile;
                    firstTile.SelectTile();
                    DisplayBoard();
                    Console.WriteLine("Kafelki nie leżą obok siebie!");
                    Console.WriteLine("Wpisz adres pozycji, na którą chcesz przesunąć kafelek.\nWpisz \"X\" aby ponownie wybrać pierwszy kafelek.\n");
                    secondTile = AskPlayerToSelectTile(_userInput, isCancellable: true, out textEnteredByUser);
                    if (textEnteredByUser == "X")
                    {
                        firstTile.UnselectTile();
                        return;
                    }
                }
            }

            secondTile.SelectTile();
            DisplayBoard();

            bool moveIsAllowed = true;
            if (_hardModeOn)
            {
                moveIsAllowed = TilesAreAdjacent(firstTile, secondTile);
            }

            if (moveIsAllowed)
            {
                SwapTilesContents(firstTile, secondTile);
            }

            Thread.Sleep(250);
            DisplayBoard();
            firstTile.UnselectTile();
            secondTile.UnselectTile();
            Thread.Sleep(250);
            DisplayBoard();
        }
        private Tile AskPlayerToSelectTile(IUserInput userInput, bool isCancellable, out string inputText)
        {
            (char column, int row) selectedTilePosition = ('A', 1); // default value
            Tile selectedTile = _tilesList[0]; // default value
            string input = "";
            inputText = input;
            bool tileFormatIsCorrect = false;
            while (!tileFormatIsCorrect)
            {
                input = userInput.GetUserInput().ToUpper();
                inputText = input;
                if (isCancellable)
                {
                    if (inputText == "X")
                    {
                        break;
                    }
                }
                
                if (input.Length != 2)
                {
                    Console.WriteLine("Podaj prawidłowy adres kafelka!\n");
                    continue;
                }
                
                char inputCharNo1 = input[0];
                char inputCharNo2 = input[1];
                
                if (_columnsArr.Contains(inputCharNo1) && _rowsArr.Contains((int)Char.GetNumericValue(inputCharNo2)))
                {
                    selectedTilePosition.column = inputCharNo1;
                    selectedTilePosition.row = (int)Char.GetNumericValue(inputCharNo2);
                }
                else if (_columnsArr.Contains(inputCharNo2) && _rowsArr.Contains((int)Char.GetNumericValue(inputCharNo1)))
                {
                    selectedTilePosition.column = inputCharNo2;
                    selectedTilePosition.row = (int)Char.GetNumericValue(inputCharNo1);
                }
                else
                {
                    Console.WriteLine("Podaj prawidłowy adres kafelka!\n");
                    continue;
                }

                List<Tile> selectedTilesList = _tilesList.Where(t => t.Column == selectedTilePosition.column & t.Row == selectedTilePosition.row).ToList();

                if (selectedTilesList.Count != 1)
                {
                    Console.WriteLine("Podaj prawidłowy adres kafelka!\n");
                    continue;
                }

                tileFormatIsCorrect = true;
                selectedTile = selectedTilesList.First();
            }
            
            return selectedTile;
        }
        bool TilesAreAdjacent(Tile tile1, Tile tile2)
        {
            int indexOfPrecedingColumn = Array.IndexOf(_columnsArr, tile1.Column) - 1;
            indexOfPrecedingColumn = indexOfPrecedingColumn < 0 ? 0 : indexOfPrecedingColumn;
            char precedingColumn = _columnsArr[indexOfPrecedingColumn];

            int indexOfFollowingColumn = Array.IndexOf(_columnsArr, tile1.Column) + 1;
            indexOfFollowingColumn = indexOfFollowingColumn > _columnsArr.Length - 1 ? _columnsArr.Length - 1 : indexOfFollowingColumn;
            char followingColumn = _columnsArr[indexOfFollowingColumn];

            int indexOfPrecedingRow = Array.IndexOf(_rowsArr, tile1.Row) - 1;
            indexOfPrecedingRow = indexOfPrecedingRow < 0 ? 0 : indexOfPrecedingRow;
            int precedingRow = _rowsArr[indexOfPrecedingRow];

            int indexOfFollowingRow = Array.IndexOf(_rowsArr, tile1.Row) + 1;
            indexOfFollowingRow = indexOfFollowingRow > _rowsArr.Length - 1 ? _rowsArr.Length - 1 : indexOfFollowingRow;
            int followingRow = _rowsArr[indexOfFollowingRow];

            bool IsSameRowAdjacentColumn(Tile tile1, Tile tile2)
            {
                return tile2.Row == tile1.Row && (tile2.Column == precedingColumn || tile2.Column == followingColumn);
            }

            bool IsSameColumnAdjacentRow(Tile tile1, Tile tile2)
            {
                return tile2.Column == tile1.Column && (tile2.Row == precedingRow || tile2.Row == followingRow);
            }

            return IsSameRowAdjacentColumn(tile1, tile2) || IsSameColumnAdjacentRow(tile1, tile2);
        }
        void SwapTilesContents(Tile tile1, Tile tile2)
        {
            int temp = tile1.Content;
            tile1.Content = tile2.Content;
            tile2.Content = temp;
        }
        public void DisplayVictoryAnimation()
        {
            Thread.Sleep(250);

            for (int i = 0; i < 2; i++)
            {
                foreach (Tile tile in _tilesList)
                {
                    tile.SelectTile();
                }
                DisplayBoard();
                Thread.Sleep(250);

                foreach (Tile tile in _tilesList)
                {
                    tile.UnselectTile();
                }
                DisplayBoard();
                Thread.Sleep(250);
            }
        }
    }
}