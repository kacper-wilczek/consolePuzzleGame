using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Układanka.Classes
{
    public class GameView
    {
        public void SetUpGameOptions(out bool hardModeOn, out int boardSize)
        {
            hardModeOn = SetGameMode();
            boardSize = SetBoardSize();
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
        private void RestartAppIfInputIsRestart(string input)
        {
            input = input.ToUpper();
            if (input == "RESTART")
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
                Environment.Exit(0);
            }
        }
        private bool SetGameMode()
        {
            DisplayPrompt_SelectGameMode();
            
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
                        DisplayPrompt_EasyModeSelected();
                        break;
                    case "2":
                        hardModeOn = true;
                        gameModeChosen = true;
                        DisplayPrompt_HardModeSelected();
                        break;
                    default:
                        DisplayPrompt_ModeSelectionError();
                        break;
                }
            }
            return hardModeOn;
        }
        private int SetBoardSize()
        {
            int readBoardSize = 0; // default value
            bool boardSizeReadSuccessfully = false;
            while (!boardSizeReadSuccessfully)
            {
                DisplayPrompt_SelectBoardSize();

                boardSizeReadSuccessfully = int.TryParse(GetInputFromUser(), out int result);
                readBoardSize = result;

                if (!boardSizeReadSuccessfully)
                {
                    DisplayPrompt_BoardSizeSelectionError();
                    continue;
                }

                if (readBoardSize < 2 || readBoardSize > 26)
                {
                    DisplayPrompt_BoardSizeSelectionLimitError();
                    boardSizeReadSuccessfully = false;
                }
            }
            return readBoardSize;
        }
        public bool TryReceiveAddressFromUserInput(int boardSize, out (int column, int row) address, bool cancelIsAllowed = false)
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
                    DisplayPrompt_WrongTileAddress();
                    continue;
                }

                Match matchColumnRow = Regex.Match(input, @"^([A-Z])(\d+)$");
                Match matchRowColumn = Regex.Match(input, @"^(\d+)([A-Z])$");

                string column, row;

                if (matchColumnRow.Success)
                {
                    column = matchColumnRow.Groups[1].Value;
                    row = matchColumnRow.Groups[2].Value;
                }
                else if (matchRowColumn.Success)
                {
                    row = matchRowColumn.Groups[1].Value;
                    column = matchRowColumn.Groups[2].Value;
                }
                else
                {
                    DisplayPrompt_WrongTileAddress();
                    continue;
                }

                int columnInt = Convert.ToChar(column) - 'A' + 1; // convert column letter to column number
                int.TryParse(row, out int rowInt);

                if (!IsInRange(columnInt, boardSize) || !IsInRange(rowInt, boardSize))
                {
                    DisplayPrompt_WrongTileAddress();
                    continue;
                }

                address = (columnInt, rowInt);
                break;
            }

            return true;
        }
        private bool PlayerWantsToCancelSelection(string input)
        {
            input = input.ToUpper();
            return input == "X";
        }
        private bool IsInRange(int number, int maxNumber)
        {
            return number > 0 && number <= maxNumber;
        }
        public void DisplayBoard(int[] board, int boardSize, HashSet<(int, int)>? selectedAddressess = null)
        {
            selectedAddressess ??= new HashSet<(int column, int row)>();

            int maxRowNumberLength = boardSize.ToString().Length;
            int maxTileContentLength = (boardSize * boardSize).ToString().Length;

            Reset();
            DisplayPrompt_ExitOrRestartInformation();
            
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
                    (int column, int row) currentAddress = (j + 1, i + 1);
                    int currentIndex = (currentAddress.row - 1) * boardSize + (currentAddress.column - 1);
                    string currentTileContent = board[currentIndex].ToString();

                    bool currentAddressIsSelected = selectedAddressess.Contains(currentAddress);

                    DisplayTile(currentTileContent, currentAddressIsSelected, maxTileContentLength);
                }

                WriteSeparator(maxRowNumberLength, maxTileContentLength, boardSize);
            }
        }
        private void WriteColumns(int maxRowLength, int maxTileLength, int boardSize)
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
        private void WriteSeparator(int maxRowLength, int maxTileLength, int boardSize)
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
        private void WriteRowNumbersColumn(string content, int maxLength, char paddingChar = ' ')
        {
            content = content.PadLeft(maxLength + 1, paddingChar);
            content = content.PadRight(content.Length + 1, paddingChar);
            Console.Write($"|{content}||");
        }
        private void DisplayTile(string content, bool isSelected, int maxLength)
        {
            char paddingChar = isSelected ? '*' : ' ';

            int offset = (maxLength - content.Length) / 2;

            content = content.PadRight(content.Length + offset, paddingChar);
            content = content.PadLeft(maxLength, paddingChar);
            content = content.PadRight(content.Length + 1, paddingChar);
            content = content.PadLeft(content.Length + 1, paddingChar);

            Console.Write($"{content}|");
        }
        public void DisplaySwappingAnimation(int[] gameBoard, int boardSize, (int, int) firstSelectedAddress, (int, int) secondSelectedAddress)
        {
            Thread.Sleep(250);
            DisplayBoard(gameBoard, boardSize,  new HashSet<(int, int)>() { firstSelectedAddress, secondSelectedAddress});
            Thread.Sleep(250);
            DisplayBoard(gameBoard, boardSize);
        }
        public void DisplayVictoryAnimationAndMessage(int[] board, int boardSize)
        {
            HashSet<(int, int)> allAddresses = new();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    allAddresses.Add((j + 1, i + 1));
                }
            }

            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(250);
                DisplayBoard(board, boardSize, allAddresses);
                Thread.Sleep(250);
                DisplayBoard(board, boardSize);
            }

            Console.WriteLine("Gratulacje! Wygrałeś.\nWciśnij dowolny klawisz, aby kontynować.");
        }

        public void Reset()
        {
            Console.Clear();
        }
        public void DisplayPrompt_ExitOrRestartInformation()
        {
            Console.WriteLine("Napisz w dowolonym momencie \"exit\" aby zakończyć program lub \"restart\" aby rozpocząć ponownie.\n");
        }
        private void DisplayPrompt_SelectGameMode()
        {
            Console.WriteLine("Wybierz tryb gry:");
            Console.WriteLine("1. Łatwy - kolejność kafelków można zamieniać dowolnie");
            Console.WriteLine("2. Trudny - można zmieniać kolejność tylko kafelków znajdujących się obok siebie");
            Console.WriteLine();
        }   
        private void DisplayPrompt_EasyModeSelected()
        {
            Console.WriteLine("\nWybrano tryb łatwy.\n");
        }
        private void DisplayPrompt_HardModeSelected()
        {
            Console.WriteLine("\nWybrano tryb trudny.\n");
        }
        private void DisplayPrompt_ModeSelectionError()
        {
            Console.WriteLine("\nWybierz tryb gry!");
        }
        private void DisplayPrompt_SelectBoardSize()
        {
            Console.WriteLine($"Podaj rozmiar planszy. Liczba całkowita od 2 do 26.\n");
        }
        private void DisplayPrompt_BoardSizeSelectionError()
        {
            Console.WriteLine("\nPodaj prawidłowy rozmiar planszy!");
        }
        private void DisplayPrompt_BoardSizeSelectionLimitError()
        {
            Console.WriteLine("\nPodaj liczbę całkowitą z prawidłowego przedziału!");
        }
        public void DisplayPrompt_SelectFirstTile()
        {
            Console.WriteLine("Wybierz pierwszy kafelek");
        }
        private void DisplayPrompt_WrongTileAddress()
        {
            Console.WriteLine("Podaj prawidłowy adres!");
        }
        public void DisplayPrompt_SelectSecondTileOrCancelFirstTileSelection()
        {
            Console.WriteLine("Wpisz adres pozycji, na którą chcesz przesunąć kafelek.\nWpisz \"X\" aby ponownie wybrać pierwszy kafelek.\n");
        }
        public void DisplayPrompt_SameTilesSelected()
        {
            Console.WriteLine("Wybrano ten sam kafelek!");
        }
        public void DisplayPrompt_TilesNotAdjacent()
        {
            Console.WriteLine("Kafelki nie leżą obok siebie!");
        }
    }
}
