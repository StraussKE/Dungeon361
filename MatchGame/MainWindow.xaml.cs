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

    public static class Constants
    {
        public const int MAX_ROW = 5;
        public const int MAX_COL = 10;
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string PLAYER = "🧙‍";
        const int MONSTER_COUNT = 3;
        const int LOOT_COUNT = 5;

        int [] playerLoc = new int[2]; // col, row
        Room[,] dungeon;

        static Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            MakeBoard();
            SetUpGame();
            SetUpInfoBox();
            OpenRooms(playerLoc[0], playerLoc[1]);
            ConnectDictService();
        }

        private void MakeBoard()
        {
            dungeon = new Room[Constants.MAX_COL, Constants.MAX_ROW];
            for (int col = 0; col < Constants.MAX_COL; col++)
            {
                for (int row = 0; row < Constants.MAX_ROW; row++)
                {
                    dungeon[col, row] = new Room(col, row);
                    AddClicks(dungeon[col, row].Contents);
                    _ = map.Children.Add(dungeon[col, row].Contents);
                    _ = map.Children.Add(dungeon[col, row].Walls);
                }
            }
        }

        private void AddClicks(Button roomContents)
        {
            roomContents.Click += new RoutedEventHandler(EnterRoom_Click);
            roomContents.IsEnabled = false;
        }

        private void SetUpGame()
        {
            AddPlayer();
            AddExit();
            AddMonsters(MONSTER_COUNT);
            AddLoot(LOOT_COUNT);
        }

        private Button FindEmpty()
        {
            Button emptyCell = null;
            while (emptyCell == null || emptyCell.Tag.ToString() == "show")
            {
                emptyCell = dungeon[random.Next(Constants.MAX_COL), random.Next(Constants.MAX_ROW)].Contents;
            }
            return emptyCell;
        }

        private void AddPlayer()
        {
            playerLoc[0] = random.Next(Constants.MAX_COL);
            playerLoc[1] = random.Next(Constants.MAX_ROW);

            Button playerRoom = dungeon[playerLoc[0], playerLoc[1]].Contents;
            playerRoom.Tag = "show";
            playerRoom.Content = PLAYER;
            playerRoom.Foreground = new SolidColorBrush(Colors.Blue);
            playerRoom.IsEnabled = true;
        }

        private void AddExit()
        {
            Button exitLocation = FindEmpty();
            exitLocation.Tag = "EXIT";
        }

        private void AddMonsters(int quantity)
        {
            List<string> monsters = new List<string>()
            {
                "ᓚᘏᗢ","👻","🕷", "🦄"
            };

            Button monsterCell;

            for (int i = 0; i < quantity; i++)
            {
                monsterCell = FindEmpty();
                monsterCell.Tag = monsters[random.Next(monsters.Count)];
                monsterCell.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void AddLoot(int quantity)
        {
            List<string> resources = new List<string>()
            {
                "🍌", "🎈", "📘", "🏹", "🗡", "🛡", "💣","🕯"
            };

            Button lootCell;

            for (int i = 0; i < quantity; i++)
            {
                lootCell = FindEmpty();
                lootCell.Tag = resources[random.Next(resources.Count)];
                lootCell.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void SetUpInfoBox()
        {
            InfoBox.Text = "Welcome to the Dungeon!!!";
            InfoBox.Foreground = new SolidColorBrush(Colors.Brown);
            InfoBox.FontSize = 50;
            InfoBox.VerticalAlignment = VerticalAlignment.Center;
            InfoBox.HorizontalAlignment = HorizontalAlignment.Center;
            InfoBox.SetValue(Grid.RowProperty, Constants.MAX_ROW);
            InfoBox.SetValue(Grid.ColumnSpanProperty, Constants.MAX_COL - 2);
        }

        private void ConnectDictService()
        {
            LookItUp.Click += new RoutedEventHandler(LookUp_Click);
            LookItUp.SetValue(Grid.RowProperty, Constants.MAX_ROW);
            LookItUp.SetValue(Grid.ColumnProperty, Constants.MAX_COL);
        }

        private void EnterRoom_Click(object sender, RoutedEventArgs e)
        {
            Button room = sender as Button;
            int row = (int)room.GetValue(Grid.RowProperty);
            int col = (int)room.GetValue(Grid.ColumnProperty);
            OpenRooms(col, row);

            Dictionary<string, string> symbolNamesDict = new Dictionary<string, string>()
            {
                { "🦄", "unicorn"}, {"ᓚᘏᗢ", "sphinx"}, { "🕷", "spider"},
                {"👻", "ghost"}, {"🍌", "banana"}, {"🎈", "balloon"},
                { "📘", "mystical tome" }, {"🏹", "longbow" }, {"🗡", "sword" },
                { "🛡", "shield"}, {"💣","bomb"}, {"🕯", "candle"}
            };

            string inTheRoom;

            if (room.Tag == null || room.Tag.ToString() != "show")
            {
                room.Content = room.Tag;
                room.Tag = "show";
            }

            if (room.Content == null)
            {
                inTheRoom = "This room is empty";
            }
            else if (room.Content.ToString() == PLAYER)
            {
                inTheRoom = "You gaze admiringly upon yourself";
            }
            else
            {
                string dictLookup;
                symbolNamesDict.TryGetValue(room.Content.ToString(), out dictLookup);
                inTheRoom = "You've found a " + dictLookup;
                LookUpText.Text = "Click to look up the meaning of " + dictLookup;
                LookItUp.Tag = dictLookup;
                LookItUp.BorderThickness = new Thickness(2);
            }
            InfoBox.Text = inTheRoom;
            InfoBox.FontSize = 20;
        }

        private void LookUp_Click(object sender, RoutedEventArgs e)
        {
            Button request = sender as Button;
            string definition = CheckDict(request.Tag.ToString());
            InfoBox.Text = definition;
        }

        private string CheckDict(string searchTerm) {
            string pyPath = @"C:\Python39\python.exe";
            string workingDirectory = @"D:\GitRepos\C#\Learning Gui\Matchgame\MatchGame\";
            string serviceLocation = "DictScrape\\dictionaryService.py";
            string outfileName = "request.txt";
            string infileName = searchTerm + ".json";

            WriteTxt(workingDirectory + outfileName, searchTerm);
            ProcessStartInfo lookupStartInfo = new ProcessStartInfo() {
                WorkingDirectory = workingDirectory,
                FileName = pyPath,
                Arguments = serviceLocation,
                CreateNoWindow = true
            };

            string defined;

            Process lookup = Process.Start(lookupStartInfo);
            if (lookup != null) {
                lookup.WaitForExit();
                defined = File.ReadAllText(workingDirectory + infileName);
            } else {
                defined = "An Error Occurred";
            }
            return defined;
        }

        private void WriteTxt(string filepath, string word)
        {
            File.WriteAllText(filepath, word);
        }

        private void OpenRooms(int col, int row)
        {
            List<Button> openlist = new List<Button>();

            if(col != 0)
            {
                openlist.Add(dungeon[col-1, row].Contents);
            }

            if (col != Constants.MAX_COL - 1)
            {
                openlist.Add(dungeon[col + 1, row].Contents);
            }

            if (row != 0)
            {
                openlist.Add(dungeon[col, row - 1].Contents);
            }

            if (row != Constants.MAX_ROW - 1)
            {
                openlist.Add(dungeon[col, row + 1].Contents);
            }

            foreach (Button room in openlist)
            {
                if(!room.IsEnabled)
                {
                    room.IsEnabled = true;
                }
            }
        }
    }
}