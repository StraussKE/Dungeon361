using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dungeon361
{
    public class EmptyRoom : Button
    {
        public EmptyRoom(string roomNumber)
        {
            Name = "Room" + roomNumber;
            FontSize = 30;
            Height = 76;
            Width = 76;
            BorderThickness = new Thickness(0);
            Background = new SolidColorBrush(Colors.BlanchedAlmond);
            Content = '?';
            IsHitTestVisible = false;
        }
    }
}
