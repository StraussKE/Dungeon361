﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dungeon361
{
    public class RoomWalls : Border
    {
        public RoomWalls(int roomNumber)
        {
            Name = "Walls" + roomNumber;
            Background = new SolidColorBrush(Colors.BlanchedAlmond);
            BorderBrush = new SolidColorBrush(Colors.SaddleBrown);
        }

        public void DrawWalls(int north, int south, int east, int west)
        {
            BorderThickness = new Thickness()
            {
                Left = west,
                Right = east,
                Top = north,
                Bottom = south
            };
        }
    }
}
