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

while (true)
{
    Console.Clear();
    Console.WriteLine("Napisz w dowolonym momencie \"exit\" aby zakończyć program lub \"restart\" aby rozpocząć ponownie.\n");

    bool hardModeOn = SetGameMode();
    
    int boardSize = SetBoardSize();

    int[] gameBoard = SetUpGameBoard(boardSize);

    while (!IntArrayIsInAscendingOrder(gameBoard))
    {
        MakeAMove(gameBoard, boardSize, hardModeOn);
    }

    DisplayVictoryAnimationAndMessage(gameBoard);
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
                Console.WriteLine("Wybierz tryb gry!");
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
            continue;
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
        if (arr[i - 1] > arr[i])
        {
            return false;
        }
    }
    return true;
}

void MakeAMove(int[] gameBoard, int boardSize, bool hardModeOn)
{
    DisplayBoard(gameBoard);

    // Select first tile
    Console.WriteLine("Wybierz pierwszy kafelek");
    TryReceiveAddressFromUserInput(boardSize, out (int column, int row) address);
    int firstIndex = ConvertAddresToIndex(address, boardSize);
    DisplayBoard(gameBoard, new HashSet<int>() { firstIndex });

    int secondIndex;
    bool swapSuccessfull = false; // default value
    do // ensure swap is successfull - selected tiles are adjacent on Hard Mode
    {
        // Select second tile or cancel first tile selection
        do // ensure different tiles are selected
        {
            Console.WriteLine("Wpisz adres pozycji, na którą chcesz przesunąć kafelek.\nWpisz \"X\" aby ponownie wybrać pierwszy kafelek.\n");
            if (!TryReceiveAddressFromUserInput(boardSize, out address, cancelIsAllowed: true))
            {
                return;
            }
            secondIndex = ConvertAddresToIndex(address, boardSize);

            if (secondIndex == firstIndex)
            {
                Console.WriteLine("Wybrano ten sam kafelek!");
                continue;
            }
            break;
        } while (secondIndex == firstIndex);

        DisplayBoard(gameBoard, new HashSet<int>() { firstIndex, secondIndex });

        // Swap selected tiles
        swapSuccessfull = TrySwapIndexes(gameBoard, firstIndex, secondIndex, hardModeOn);
        if (!swapSuccessfull) // On Hard Mode -> swap selection if the second tile was not adjacent to the first tile and continue selecting the second tile
        {
            firstIndex = secondIndex;
            DisplayBoard(gameBoard, new HashSet<int>() { firstIndex });
            Console.WriteLine("Kafelki nie leżą obok siebie!");
        }
        else
        {
            DisplaySwappingAnimation(gameBoard, firstIndex, secondIndex);
        }
    } while (!swapSuccessfull);
}

bool TryReceiveAddressFromUserInput(int boardSize, out (int column, int row) address, bool cancelIsAllowed = false)
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

bool TrySwapIndexes(int[] arr, int firstIndex, int secondIndex, bool hardModeOn)
{
    if (hardModeOn && !AreAdjacent((int)Math.Sqrt(arr.Length), firstIndex, secondIndex))
    {
        return false;
    }
    (arr[secondIndex], arr[firstIndex]) = (arr[firstIndex], arr[secondIndex]);
    return true;
}

bool AreAdjacent(int boardSize, int index1, int index2) // TODO przemyśleć dodatkowe warunki
{
    return index2 == index1 - 1
        || index2 == index1 + 1
        || index2 == index1 - boardSize
        || index2 == index1 + boardSize;
}

int ConvertAddresToIndex((int column, int row) address, int boardSize)
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

void DisplayBoard(int[] board, HashSet<int>? selectedIndexes = null)
{
    selectedIndexes ??= new HashSet<int>();

    Console.Clear();
    Console.WriteLine("Napisz w dowolonym momencie \"exit\" aby zakończyć program lub \"restart\" aby rozpocząć ponownie.\n");
    //Console.WriteLine();

    int boardSize = (int)Math.Sqrt(board.Length);
    int maxRowNumberLength = boardSize.ToString().Length;
    int maxTileContentLength = (boardSize * boardSize).ToString().Length;

    // Display row with columns
    WriteColumns(maxRowNumberLength, maxTileContentLength, boardSize);
    WriteSeparator(maxRowNumberLength, maxTileContentLength, boardSize);

    // Row number loop
    for (int i = 0; i < boardSize; i++)
    {
        WriteRowNumber(i + 1, maxRowNumberLength);

        // Column loop
        for (int j = 0; j < boardSize; j++)
        {
            int currentIndex = ConvertAddresToIndex((j + 1, i + 1), boardSize);
            string currentTileContent = board[currentIndex].ToString();

            bool currentIndexIsSelected = selectedIndexes.Contains(currentIndex);

            DisplayTile(currentTileContent, currentIndexIsSelected, maxTileContentLength);
        }

        WriteSeparator(maxRowNumberLength, maxTileContentLength, boardSize);
    }
}

void WriteColumns(int maxRowLength, int maxTileLength, int boardSize)
{
    string rowsColumn = " ";
    rowsColumn = rowsColumn.PadLeft(maxRowLength + 1);
    rowsColumn = rowsColumn.PadRight(rowsColumn.Length + 1);
    Console.Write($"|{rowsColumn}||");

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

    string rowsColumn = "-";
    rowsColumn = rowsColumn.PadLeft(maxRowLength + 1, '-');
    rowsColumn = rowsColumn.PadRight(rowsColumn.Length + 1, '-');
    Console.Write($"|{rowsColumn}||");

    string columnsSeparator = "-";
    columnsSeparator = columnsSeparator.PadLeft(maxTileLength + 1, '-');
    columnsSeparator = columnsSeparator.PadRight(columnsSeparator.Length + 1, '-');

    for (int i = 0; i < boardSize; i++)
    {
        Console.Write($"{columnsSeparator}|");
    }

    Console.WriteLine("");
}

void WriteRowNumber(int rowNumber, int maxRowLength)
{
    string rowNumberString = rowNumber.ToString();
    rowNumberString = rowNumberString.PadLeft(maxRowLength + 1);
    rowNumberString = rowNumberString.PadRight(rowNumberString.Length + 1);
    Console.Write($"|{rowNumberString}||");
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

void DisplaySwappingAnimation(int[] gameBoard, int firstSelectedIndex, int secondSelectedIndex)
{
    Thread.Sleep(250);
    DisplayBoard(gameBoard, new HashSet<int>() { firstSelectedIndex, secondSelectedIndex });
    Thread.Sleep(250);
    DisplayBoard(gameBoard);
}

void DisplayVictoryAnimationAndMessage(int[] board)
{
    int boardSize = (int)Math.Sqrt(board.Length);

    for (int i = 0; i < 2; i++)
    {
        Thread.Sleep(250);
        DisplayBoard(board, new HashSet<int>(Enumerable.Range(0, boardSize * boardSize)));
        Thread.Sleep(250);
        DisplayBoard(board);
    }

    Console.WriteLine("Gratulacje! Wygrałeś.\nWciśnij dowolny klawisz, aby kontynować.");
}
