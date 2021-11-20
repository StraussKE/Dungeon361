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

namespace Dungeon361
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string PLAYER = "🧙‍";
        private const int MAX_ROW = 5;
        private const int MAX_COL = 10;
        private const int TOTAL_ROOMS = MAX_COL + MAX_ROW;
        private const int MONSTER_COUNT = 3;
        private const int LOOT_COUNT = 5;

        private const string PYTHON_PATH = @"C:\Python39\python.exe";
        private const string PROJ_WORKING_DIR = @"D:\GitRepos\C#\Learning Gui\Dungeon361\Dungeon361\";
        private const string DICT_SVC_LOC = "DictScrape\\dictionaryService.py";
        private const string DICT_SVC_REQUEST_FILENAME = "request.txt";

        static Dictionary<string, string> monsterDict = new Dictionary<string, string>()
        {
            { "🦄", "unicorn"}, {"ᓚᘏᗢ", "sphinx"}, { "🕷", "spider"}, {"👻", "ghost"},
        };

        private static Dictionary<string, string> lootDict = new Dictionary<string, string>()
        {
            {"🍌", "banana"}, {"🎈", "balloon"}, { "📘", "tome" }, {"🏹", "longbow" },
            {"🗡", "sword" }, { "🛡", "shield"}, {"💣","bomb"}, {"🕯", "candle"}
        };
        private static readonly Dictionary<string, string> playerDict = new Dictionary<string, string>()
        {
            { "🧙‍", "wizard" },
        };

        static readonly List<Dictionary<string, string>> dictList = new List<Dictionary<string, string>>()
        {
            monsterDict, lootDict, playerDict
        };

        private Button playerLoc;

        static Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            SetUpDungeonUIElements();
            PopupateDungeon();
            OpenAdjacentRooms(playerLoc);
            ConnectDictService();
        }

        private void SetUpDungeonUIElements()
        {
            for (int row = 0; row < MAX_ROW; row++)
            {
                for (int col = 0; col < MAX_COL; col++)
                {
                    int roomNumber = Flatten(col, row);
                    Button newRoom = BuildRoom(roomNumber);
                    Border roomWalls = ErectWalls(roomNumber, col, row);
                    SetMapCoordinates(newRoom, col, row);
                    SetMapCoordinates(roomWalls, col, row);
                    _ = map.Children.Add(newRoom);
                    _ = map.Children.Add(roomWalls);
                }
            }
        }

        private int Flatten(int col, int row)
        {
            return col + (row * MAX_COL);
        }

        private Button BuildRoom(int roomNumber)
        {
            Button aRoom = new EmptyRoom(roomNumber);
            AddRoomClicks(aRoom);
            return aRoom;
        }

        private void AddRoomClicks(Button roomContents)
        {
            roomContents.Click += new RoutedEventHandler(EnterRoom_Click);
            roomContents.IsEnabled = false;
        }

        private Border ErectWalls(int roomNumber, int col, int row)
        {
            RoomWalls someWalls = new RoomWalls(roomNumber);
            PositionWalls(someWalls, col, row);
            return someWalls;
        }

        private void PositionWalls(RoomWalls someWalls, int col, int row)
        {
            //TODO: replace with file call, will be able to remove col/row params
            int north = col == 0 ? 0 : random.Next(2);
            int south = col == MAX_COL - 1 ? 0 : random.Next(2);
            int east = row == MAX_ROW ? 0 : random.Next(2);
            int west = row == 0 ? 0 : random.Next(2);

            someWalls.DrawWalls(north: north, south: south, east: east, west: west);
        }

        private void SetMapCoordinates(DependencyObject thing, int col, int row)
        {
            thing.SetValue(Grid.RowProperty, row);
            thing.SetValue(Grid.ColumnProperty, col);
        }

        private void AddToMap(UIElement element)
        {
            _ = map.Children.Add(element);
        }

        private void PopupateDungeon()
        {
            AddPlayer();
            AddExit();
            AddMonsters(MONSTER_COUNT);
            AddLoot(LOOT_COUNT);
            SetUpInfoBox();
        }

        private void AddPlayer()
        {
            string startingLocation = MakeRoomName(random.Next(MAX_COL), random.Next(MAX_ROW));
            playerLoc = FindRoom(startingLocation);
            playerLoc.Content = PLAYER;
            playerLoc.Tag = "show";
            playerLoc.Foreground = new SolidColorBrush(Colors.Blue);
            playerLoc.IsEnabled = true;
        }

        private Button FindRoom(string roomName)
        {
            return map.Children.OfType<Button>().FirstOrDefault(room => room.Name == roomName);
        }

        private void AddExit()
        {
            Button exitLocation = FindEmpty();
            exitLocation.Tag = "EXIT";
        }

        private Button FindEmpty()
        {
            Button emptyCell = null;
            while (emptyCell == null || emptyCell.Tag != null)
            {
                emptyCell = FindRoom("Room" + random.Next(TOTAL_ROOMS));
            }
            return emptyCell;
        }

        private void AddMonsters(int quantity)
        {
            List<string> monsters = monsterDict.Keys.AsQueryable().ToList<string>();
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
            InfoBox.SetValue(Grid.RowProperty, MAX_ROW);
            InfoBox.SetValue(Grid.ColumnSpanProperty, MAX_COL - 2);
        }

        private void EnterRoom_Click(object sender, RoutedEventArgs e)
        {
            Button room = sender as Button;

            RevealContents(room);
            OpenAdjacentRooms(room);

            string whatHaveWeHere = room.Content == null ? "" : GetSymbolString(room.Content.ToString());
            InfoBoxUpdate(whatHaveWeHere);
            if (whatHaveWeHere != "")
            {
                LookItUpButtonUpdate(whatHaveWeHere);
            }
        }

        private void OpenAdjacentRooms(Button aRoom)
        {
            Dictionary<string, int> coords = GetGridCoords(aRoom);
            List<string> roomsToOpen = GetAdjacentNames(coords);
            foreach (string roomName in roomsToOpen)
            {
                Button adjRoom = FindRoom(roomName);
                if (!adjRoom.IsEnabled)
                {
                    adjRoom.IsEnabled = true;
                }
            }
        }

        private Dictionary<string, int> GetGridCoords(DependencyObject thing)
        {
            return new Dictionary<string, int>
            {
                { "row", (int)thing.GetValue(Grid.RowProperty)},
                { "col", (int)thing.GetValue(Grid.ColumnProperty)},
            };

        }

        private List<string> GetAdjacentNames(Dictionary<string, int> coords)
        {
            List<string> roomList = new List<string>();

            if (coords["col"] != 0)
            {
                roomList.Add(MakeRoomName(coords["col"] - 1, coords["row"]));
            }

            if (coords["col"] != MAX_COL - 1)
            {
                roomList.Add(MakeRoomName(coords["col"] + 1, coords["row"]));
            }

            if (coords["row"] != 0)
            {
                roomList.Add(MakeRoomName(coords["col"], coords["row"] - 1));
            }

            if (coords["row"] != MAX_ROW - 2)
            {
                roomList.Add(MakeRoomName(coords["col"], coords["row"] + 1));
            }
            return roomList;
        }

        private string MakeRoomName(int col, int row)
        {
            return "Room" + Flatten(col, row);
        }

        private void RevealContents(Button room)
        {
            if (room.Tag == null || room.Tag.ToString() != "show")
            {
                room.Content = room.Tag;
                room.Tag = "show";
            }
        }

        private string GetSymbolString(string lookingfor)
        {
            string foundit = "";
            foreach (Dictionary<string, string> dict in dictList)
            {
                if (dict.TryGetValue(lookingfor, out foundit))
                {
                    break;
                }
            }
            return foundit;
        }

        private void InfoBoxUpdate(string roomContents)
        {
            InfoBox.Text = roomContents == ""
                ? "This room is empty"
                : roomContents == "wizard" ? "You're a wizard, Harry!" : "You've found a " + roomContents;
            InfoBox.FontSize = 20;
        }

        private void LookItUpButtonUpdate(string searchTerm)
        {
            if (!LookItUp.IsEnabled)
            {
                LookItUp.IsEnabled = true;
            }
            LookUpText.Text = "Click to look up the meaning of " + searchTerm;
            LookItUp.Tag = searchTerm;
        }

        private void ConnectDictService()
        {
            LookItUp.Click += new RoutedEventHandler(LookUp_Click);
            LookItUp.IsEnabled = false;
        }

        private void LookUp_Click(object sender, RoutedEventArgs e)
        {
            Button request = sender as Button;
            string definition = CheckDict(request.Tag.ToString());
            InfoBox.Text = definition;
        }

        private ProcessStartInfo CreateDictProcessStartInfo()
        {
            return new ProcessStartInfo()
            {
                WorkingDirectory = PROJ_WORKING_DIR,
                FileName = PYTHON_PATH,
                Arguments = DICT_SVC_LOC,
                CreateNoWindow = true
            };
        }

        private string RunDictService(ProcessStartInfo svcStartInfo, string searchTerm)
        {
            Process lookup = Process.Start(svcStartInfo);
            if (lookup != null)
            {
                lookup.WaitForExit();
                string infileName = searchTerm + ".json";
                return File.ReadAllText(PROJ_WORKING_DIR + infileName);
            }
            return "An Error Occurred";
        }

        private string CheckDict(string searchTerm)
        {
            WriteRequestTxt(PROJ_WORKING_DIR + DICT_SVC_REQUEST_FILENAME, searchTerm);
            ProcessStartInfo lookupStartInfo = CreateDictProcessStartInfo();
            return RunDictService(lookupStartInfo, searchTerm);
        }

        private void WriteRequestTxt(string filepath, string word)
        {
            File.WriteAllText(filepath, word);
        }
    }
}