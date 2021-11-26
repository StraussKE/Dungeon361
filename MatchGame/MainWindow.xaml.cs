using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private const int TOTAL_ROOMS = MAX_COL * MAX_ROW;
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

        private static readonly List<Dictionary<string, string>> dictList = new List<Dictionary<string, string>>()
        {
            monsterDict, lootDict, playerDict
        };

        private Button playerLoc;

        private static readonly Random random = new Random();
        private static SolidColorBrush visitedRoomColor = new SolidColorBrush(Colors.Tan);
        private static SolidColorBrush monsterColor = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush lootColor = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush playerColor = new SolidColorBrush(Colors.Blue);

        public MainWindow()
        {
            InitializeComponent();
            SetUpDungeonUIElements();
            PopupateDungeon();
            ToggleAdjacentRooms(playerLoc, true);
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
                    PositionOnMap(roomWalls, col, row);
                    PositionOnMap(newRoom, col, row);
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
            aRoom.Click += new RoutedEventHandler(EnterRoom_Click);
            return aRoom;
        }

        private Border ErectWalls(int roomNumber, int col, int row)
        {
            RoomWalls someWalls = new RoomWalls(roomNumber);
            PositionWalls(someWalls, col, row);
            return someWalls;
        }

        private void PositionWalls(RoomWalls someWalls, int col, int row)
        {
            // Dictionary<string, Dictionary<string, bool> = JsonSerializer.Serialize(wallmap.json) <have to file i/o this
            //TODO: replace with file call, will be able to remove col/row params
            int north = col == 0 ? 0 : random.Next(2);
            int south = col == MAX_COL - 1 ? 0 : random.Next(2);
            int east = row == MAX_ROW ? 0 : random.Next(2);
            int west = row == 0 ? 0 : random.Next(2);

            someWalls.DrawWalls(north: north, south: south, east: east, west: west);
        }

        private void PositionOnMap(UIElement thing, int col, int row)
        {
            thing.SetValue(Grid.RowProperty, row);
            thing.SetValue(Grid.ColumnProperty, col);
            _ = map.Children.Add(thing);
        }

        private void PopupateDungeon()
        {
            AddPlayer();
            AddExit();
            AddToDungeon(MONSTER_COUNT, monsterDict.Keys.AsQueryable().ToList());
            AddToDungeon(LOOT_COUNT, lootDict.Keys.AsQueryable().ToList());
            SetUpInfoBox();
        }

        private void AddPlayer()
        {
            string startingLocation = MakeRoomName(random.Next(MAX_COL), random.Next(MAX_ROW));
            playerLoc = FindRoom(startingLocation);
            playerLoc.Content = PLAYER;
            playerLoc.Tag = "show";
            playerLoc.Foreground = playerColor;
            playerLoc.IsHitTestVisible = true;
            playerLoc.Background = visitedRoomColor;
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

        private void AddToDungeon(int quantity, List<string> possibleAdditions)
        {
            Button emptyCell;

            for (int i = 0; i < quantity; i++)
            {
                emptyCell = FindEmpty();
                emptyCell.Tag = possibleAdditions[random.Next(possibleAdditions.Count)];
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
            room.Background = visitedRoomColor;
            RevealContents(room);

            string whatHaveWeHere = room.Content == null ? "" : GetSymbolString(room.Content.ToString());
            InfoBoxUpdate(whatHaveWeHere);
            if (whatHaveWeHere != "")
            {
                LookItUpButtonUpdate(whatHaveWeHere);
            }
            ToggleAdjacentRooms(room, true);
            MovePlayer(room);
            room.IsHitTestVisible = true;
        }

        private void MovePlayer(Button room)
        {
            if (room == playerLoc)
            {
                return;
            }
            ToggleAdjacentRooms(playerLoc, false);
            room.Content = PLAYER + RoomContents(room);
            playerLoc.Content = null;
            playerLoc.Foreground = null;
            playerLoc = room;
        }

        private string RoomContents(Button room)
        {
            if(room.Content == null)
            {
                room.Foreground = playerColor;
            }
            else if(lootDict.ContainsKey(room.Content.ToString()))
            {
                // TODO - add item to player inventory
                room.Foreground = lootColor;
            }
            else if(monsterDict.ContainsKey(room.Content.ToString()))
            {
                // TODO - add battle mechanic and click commands
                room.Foreground = monsterColor;
                return " | " + room.Content.ToString();
            }
            return "";
        }

        private void ToggleAdjacentRooms(Button aRoom, bool toggleChange)
        {
            Dictionary<string, int> coords = GetGridCoords(aRoom);
            List<string> roomsToToggle = GetAdjacentNames(coords);
            foreach (string roomName in roomsToToggle)
            {
                Button adjRoom = FindRoom(roomName);
                adjRoom.IsHitTestVisible = toggleChange;
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

            if (coords["row"] != MAX_ROW - 1)
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
            string searchTerm = request.Tag.ToString();
            string definition = CheckDict(searchTerm);
            List<string> listOfDefinitions = JsonSerializer.Deserialize<List<string>>(definition); // candle broken, need exception handling
            DisplayDefinitions(searchTerm, listOfDefinitions);
        }

        private void DisplayDefinitions(string word, List<string> definitionList)
        {
            for (int i = 0; i < definitionList.Count; i++)
            {
                string msgBoxOutput = definitionList[i] + "\n\nAre you satisfied with this definition?";
                MessageBoxResult result = MessageBox.Show(msgBoxOutput, word, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    return;
                }
            }
            _ = MessageBox.Show("I'm sorry none of these definitions was what you were looking for.", word);
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