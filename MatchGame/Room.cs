using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;

namespace MatchGame
{
    public class Room
    {
        public Border Walls;
        public Button Contents;

        public int mapCol;
        public int mapRow;

        static Random random = new Random(); // replace this later

        public Room(int col, int row)
        {
            mapCol = col;
            mapRow = row;

            Walls = new Border()
            {
                Background = new SolidColorBrush(Colors.Tan),
                BorderBrush = new SolidColorBrush(Colors.SaddleBrown),
            };
            Walls.SetValue(Grid.ColumnProperty, mapCol);
            Walls.SetValue(Grid.RowProperty, mapRow);
            
            AddWalls(); // replace later

            Contents = new Button()
            {
                FontSize = 30,
                Height = 76,
                Width = 76,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Colors.Tan),
                Content = '?',
            };
            Contents.SetValue(Grid.ColumnProperty, col);
            Contents.SetValue(Grid.RowProperty, row);
            //int flat = Flatten(col, row);

            //roomContents.Name = ROOM_TEXT + flat.ToString();
        }

        public void AddWalls()
        {
            // replace this
            int north = mapCol == 0 ? 0 : random.Next(2);
            int south = mapCol == Constants.MAX_COL - 1 ? 0 : random.Next(2);
            int east = mapRow == Constants.MAX_ROW ? 0 : random.Next(2);
            int west = mapRow == 0 ? 0 : random.Next(2);
            Walls.BorderThickness = new Thickness()
            {
                Left = west,
                Right = east,
                Top = north,
                Bottom = south
            };
        }

    }
}
