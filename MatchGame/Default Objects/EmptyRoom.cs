using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MatchGame
{
    public class EmptyRoom : Button
    {
        public EmptyRoom(int roomNumber)
        {
            Name = "Room" + roomNumber;
            FontSize = 30;
            Height = 76;
            Width = 76;
            BorderThickness = new Thickness(0);
            Background = new SolidColorBrush(Colors.Tan);
            Content = '?';
        }
    }
}
