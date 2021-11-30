using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dungeon361
{
    public class WallSet
    {
        private string Name { get; set; }
        private bool North { get; set; }
        private bool South { get; set; }
        private bool East { get; set; }
        private bool West { get; set; }


        public Border DrawWalls()
        {
            Border newRoom = new Border();
            newRoom.BorderThickness = new Thickness()
            {
                Left = West == true ? 1 : 0,
                Right = East ? 1 : 0,
                Top = North ? 1 : 0,
                Bottom = South ? 1 : 0
            };
            newRoom.Name = this.Name;
            newRoom.Background = new SolidColorBrush(Colors.BlanchedAlmond);
            newRoom.BorderBrush = new SolidColorBrush(Colors.SaddleBrown);
            return newRoom;
        }
    }
}
