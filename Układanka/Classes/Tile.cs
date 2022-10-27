using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Układanka.Classes
{
    public class Tile
    {
        public Tile(int content, int position, char column, int row)
        {
            Content = content;
            PositionInt = position;
            Column = column;
            Row = row;
        }
        public int Content { get; set; }
        public int PositionInt { get; }
        public char Column { get; }
        public int Row { get; }
        private bool _isSelected;
        public void SelectTile()
        {
            _isSelected = true;
        }
        public void UnselectTile()
        {
            _isSelected = false;
        }
        public void DisplayTile(int maxLength) // inny bezużyteczny komentarz
        {
            char paddingCharacter = _isSelected ? '*' : ' ';
            string contentToDisplay = Content.ToString();
            int offset = (maxLength - contentToDisplay.Length) / 2;

            contentToDisplay = contentToDisplay.PadRight(contentToDisplay.Length + offset, paddingCharacter);
            contentToDisplay = contentToDisplay.PadLeft(maxLength, paddingCharacter);
            contentToDisplay = contentToDisplay.PadRight(contentToDisplay.Length + 1, paddingCharacter);
            contentToDisplay = contentToDisplay.PadLeft(contentToDisplay.Length + 1, paddingCharacter);

            Console.Write($"{contentToDisplay}|");

            //string contentToDisplay = Content.ToString().PadLeft(maxLength + 1, paddingCharacter);
            //contentToDisplay = contentToDisplay.PadRight(contentToDisplay.Length + 1, paddingCharacter);
            //Console.Write($"{contentToDisplay}|");

            //string contentFormat = "{0}";

            //if (maxLength % 2 == 0)
            //{
            //    contentFormat = contentFormat.PadLeft(maxLength + contentFormat.Length);
            //    contentFormat = contentFormat.PadRight(contentFormat.Length + 1);
            //}
            //else
            //{
            //    int padding = (maxLength + 1) / 2;
            //    contentFormat = contentFormat.PadLeft(contentFormat.Length + padding);
            //    contentFormat = contentFormat.PadRight(contentFormat.Length + padding);
            //}

            //foreach (char column in _columnsArr)
            //{
            //    Console.Write(contentFormat + "|", column);
            //}
        }
    }
}