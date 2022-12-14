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

internal class Program
{
    public static bool hardModeOn;
    public static int boardSize;
    public static int[] gameBoard;
    private static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Napisz w dowolonym momencie \"exit\" aby zakończyć program lub \"restart\" aby rozpocząć ponownie.\n");

            hardModeOn = SetGameMode();

            boardSize = SetBoardSize();

            gameBoard = SetUpGameBoard(boardSize);

            while (!IntArrayIsInAscendingOrder(gameBoard))
            {
                MakeAMove();
            }

            DisplayVictoryAnimationAndMessage();
            Console.ReadKey(true);
        }

        bool SetGameMode()
        {
            Console.WriteLine("Wybierz tryb gry:");
            Console.WriteLine("1. Łatwy - kolejność kafelków można zamieniać dowolnie");
            Console.WriteLine("2. Trudny - można zmieniać kolejność tylko kafelków znajdujących się obok siebie");
            Console.WriteLine();

            bool hardModeOn = false;
            bool gameModeChosen = false;
            while (!gameModeChosen)
            {
                string input = GetInputFromUser();
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
                        Console.WriteLine("\nWybierz tryb gry!");
                        break;
                }
            }
            return hardModeOn;
        }

        int SetBoardSize()
        {
            int readBoardSize = 0; // default value
            bool boardSizeReadSuccessfully = false;
            while (!boardSizeReadSuccessfully)
            {
                Console.WriteLine($"Podaj rozmiar planszy. Liczba całkowita od 2 do 26.\n");

                boardSizeReadSuccessfully = int.TryParse(GetInputFromUser(), out int result);
                readBoardSize = result;

                if (!boardSizeReadSuccessfully)
                {
                    Console.WriteLine("\nPodaj prawidłowy rozmiar planszy!");
                    continue;
                }

                if (readBoardSize < 2 || readBoardSize > 26)
                {
                    Console.WriteLine("\nPodaj liczbę całkowitą z prawidłowego przedziału!");
                    boardSizeReadSuccessfully = false;
                }
            }
            return readBoardSize;
        }

        int[] SetUpGameBoard(int boardSize)
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

        bool IntArrayIsInAscendingOrder(int[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i - 1] >= arr[i])
                {
                    return false;
                }
            }
            return true;
        }

        void MakeAMove()
        {
            DisplayBoard();

            // Select first tile
            Console.WriteLine("Wybierz pierwszy kafelek");
            TryReceiveAddressFromUserInput(out (int column, int row) address);
            int firstIndex = ConvertAddressToIndex(address);
            DisplayBoard(new HashSet<int>() { firstIndex });

            int secondIndex;
            bool swapSuccessful = false; // default value
            do // ensure swap is successful - selected tiles are adjacent on Hard Mode
            {
                do // ensure different tiles are selected
                {
                    // Select second tile or cancel first tile selection
                    Console.WriteLine("Wpisz adres pozycji, na którą chcesz przesunąć kafelek.\nWpisz \"X\" aby ponownie wybrać pierwszy kafelek.\n");
                    if (!TryReceiveAddressFromUserInput(out address, cancelIsAllowed: true))
                    {
                        return;
                    }
                    secondIndex = ConvertAddressToIndex(address);

                    if (secondIndex == firstIndex)
                    {
                        Console.WriteLine("Wybrano ten sam kafelek!");
                        continue;
                    }
                } while (secondIndex == firstIndex);

                DisplayBoard(new HashSet<int>() { firstIndex, secondIndex });

                // Swap selected tiles
                swapSuccessful = TrySwapIndexes(firstIndex, secondIndex);
                if (!swapSuccessful) // On Hard Mode -> swap selection if the second tile was not adjacent to the first tile and continue selecting the second tile
                {
                    firstIndex = secondIndex;
                    DisplayBoard(new HashSet<int>() { firstIndex });
                    Console.WriteLine("Kafelki nie leżą obok siebie!");
                }
                else
                {
                    DisplaySwappingAnimation(firstIndex, secondIndex);
                }
            } while (!swapSuccessful);
        }

        bool TryReceiveAddressFromUserInput(out (int column, int row) address, bool cancelIsAllowed = false)
        {
            string input;
            address = (1, 1); // default value
            int maxRowNumberLength = boardSize.ToString().Length;

            while (true) // ensure correct address is provided
            {
                input = GetInputFromUser().ToUpper();

                if (cancelIsAllowed && PlayerWantsToCancelSelection(input))
                {
                    return false;
                }

                if (input.Length == 0 || input.Length > maxRowNumberLength + 1)
                {
                    Console.WriteLine("Podaj prawidłowy adres!");
                    continue;
                }

                Match matchColumn = Regex.Match(input, @"^[A-Z](?=\d+$)"); // match column given first in input
                matchColumn = matchColumn.Success ? matchColumn : Regex.Match(input, @"(?<=^\d+)[A-Z]$"); // match column given last in input

                Match matchRow = Regex.Match(input, @"^\d+(?=[A-Z]$)"); // match row given first in input
                matchRow = matchRow.Success ? matchRow : Regex.Match(input, @"(?<=^[A-Z])\d+$"); // match row given last in input

                if (!matchColumn.Success || !matchRow.Success)
                {
                    Console.WriteLine("Podaj prawidłowy adres!");
                    continue;
                }

                int columnInt = Convert.ToChar(matchColumn.Value) - 'A' + 1; // convert column letter to column number
                int.TryParse(matchRow.Value, out int rowInt);

                if (!IsInRange(columnInt, boardSize) || !IsInRange(rowInt, boardSize))
                {
                    Console.WriteLine("Podaj prawidłowy adres!");
                    continue;
                }

                address = (columnInt, rowInt);
                break;
            }

            return true;
        }

        bool PlayerWantsToCancelSelection(string input)
        {
            input = input.ToUpper();
            return input == "X";
        }

        bool IsInRange(int number, int maxNumber)
        {
            return number > 0 && number <= maxNumber;
        }

        bool TrySwapIndexes(int firstIndex, int secondIndex)
        {
            if (hardModeOn && !AreAdjacent(firstIndex, secondIndex))
            {
                return false;
            }

            (gameBoard[secondIndex], gameBoard[firstIndex]) = (gameBoard[firstIndex], gameBoard[secondIndex]);

            return true;
        }

        bool AreAdjacent(int index1, int index2)
        {
            return index2 == index1 - 1
                || index2 == index1 + 1
                || index2 == index1 - boardSize
                || index2 == index1 + boardSize;
        }

        int ConvertAddressToIndex((int column, int row) address)
        {
            return (address.row - 1) * boardSize + (address.column - 1);
        }

        void RandomizeArray(ref int[] arr)
        {
            Random randomNumberGenerator = new Random();
            arr = arr.OrderBy(e => randomNumberGenerator.Next()).ToArray();
        }

        string GetInputFromUser()
        {
            string? inputOrNull = Console.ReadLine();
            string input = inputOrNull ?? "";
            ExitAppIfInputIsExit(input);
            RestartAppIfInputIsRestart(input);
            return input;
        }

        void ExitAppIfInputIsExit(string input)
        {
            input = input.ToUpper();
            if (input == "EXIT")
            {
                Environment.Exit(0);
            }
        }

        void RestartAppIfInputIsRestart(string input)
        {
            input = input.ToUpper();
            if (input == "RESTART")
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
                Environment.Exit(0);
            }
        }

        void DisplayBoard(HashSet<int>? selectedIndexes = null)
        {
            selectedIndexes ??= new HashSet<int>();

            int maxRowNumberLength = boardSize.ToString().Length;
            int maxTileContentLength = (boardSize * boardSize).ToString().Length;

            Console.Clear();
            Console.WriteLine("Napisz w dowolnym momencie \"exit\" aby zakończyć program lub \"restart\" aby rozpocząć ponownie.\n");

            // Display row with columns
            WriteColumns(maxRowNumberLength, maxTileContentLength, boardSize);
            WriteSeparator(maxRowNumberLength, maxTileContentLength, boardSize);

            // Row number loop
            for (int i = 0; i < boardSize; i++)
            {
                WriteRowNumbersColumn((i + 1).ToString(), maxRowNumberLength);

                // Column loop
                for (int j = 0; j < boardSize; j++)
                {
                    int currentIndex = ConvertAddressToIndex((j + 1, i + 1));
                    string currentTileContent = gameBoard[currentIndex].ToString();

                    bool currentIndexIsSelected = selectedIndexes.Contains(currentIndex);

                    DisplayTile(currentTileContent, currentIndexIsSelected, maxTileContentLength);
                }

                WriteSeparator(maxRowNumberLength, maxTileContentLength, boardSize);
            }
        }

        void WriteColumns(int maxRowLength, int maxTileLength, int boardSize)
        {
            WriteRowNumbersColumn(" ", maxRowLength);

            for (int i = 0; i < boardSize; i++)
            {
                string columnString = ((char)('A' + i)).ToString();
                int offset = (maxTileLength - columnString.Length) / 2;

                columnString = columnString.PadRight(columnString.Length + offset);
                columnString = columnString.PadLeft(maxTileLength);
                columnString = columnString.PadRight(columnString.Length + 1);
                columnString = columnString.PadLeft(columnString.Length + 1);

                Console.Write($"{columnString}|");
            }
        }

        void WriteSeparator(int maxRowLength, int maxTileLength, int boardSize)
        {
            Console.WriteLine();

            WriteRowNumbersColumn("-", maxRowLength, '-');

            string columnsSeparator = "-";
            columnsSeparator = columnsSeparator.PadLeft(maxTileLength + 1, '-');
            columnsSeparator = columnsSeparator.PadRight(columnsSeparator.Length + 1, '-');

            for (int i = 0; i < boardSize; i++)
            {
                Console.Write($"{columnsSeparator}|");
            }

            Console.WriteLine("");
        }

        void WriteRowNumbersColumn(string content, int maxLength, char paddingChar = ' ')
        {
            content = content.PadLeft(maxLength + 1, paddingChar);
            content = content.PadRight(content.Length + 1, paddingChar);
            Console.Write($"|{content}||");
        }

        void DisplayTile(string content, bool isSelected, int maxLength)
        {
            char paddingChar = isSelected ? '*' : ' ';

            int offset = (maxLength - content.Length) / 2;

            content = content.PadRight(content.Length + offset, paddingChar);
            content = content.PadLeft(maxLength, paddingChar);
            content = content.PadRight(content.Length + 1, paddingChar);
            content = content.PadLeft(content.Length + 1, paddingChar);

            Console.Write($"{content}|");
        }

        void DisplaySwappingAnimation(int firstSelectedIndex, int secondSelectedIndex)
        {
            Thread.Sleep(250);
            DisplayBoard(new HashSet<int>() { firstSelectedIndex, secondSelectedIndex });
            Thread.Sleep(250);
            DisplayBoard();
        }

        void DisplayVictoryAnimationAndMessage()
        {
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(250);
                DisplayBoard(new HashSet<int>(Enumerable.Range(0, boardSize * boardSize)));
                Thread.Sleep(250);
                DisplayBoard();
            }

            Console.WriteLine("Gratulacje! Wygrałeś.\nWciśnij dowolny klawisz, aby kontynować.");
        }
    }
}