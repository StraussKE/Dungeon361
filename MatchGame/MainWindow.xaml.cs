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
        private const string DEATH = "☠";
        private const int START_ROW = 1;
        private const int MAX_ROW = 6;
        private const int MAX_COL = 10;
        private const int TOTAL_ROOMS = MAX_COL * MAX_ROW;
        private const int MONSTER_COUNT = 3;
        private const int LOOT_COUNT = 5;

        // File input/output constants
        private const string PROJ_WORKING_DIR = @"D:\GitRepos\C#\Learning Gui\Dungeon361\Dungeon361\";

        // Predefined Wall Layout
        private const string WALL_FILE = "WallLayouts\\WallMap1.json";

        // DictService Files
        private const string PYTHON_PATH = @"C:\Python39\python.exe";
        private const string DICT_SVC_LOC = "DictScrape\\dictionaryService.py";
        private const string DICT_SVC_REQUEST_FILENAME = "request.txt";

        static Dictionary<string, string> monsterDict = new Dictionary<string, string>()
        {
            { "🦄", "unicorn"}, {"ᓚᘏᗢ", "sphinx"}, { "🕷", "spider"}, {"👻", "ghost"},
        };

        private static Dictionary<string, string> lootDict = new Dictionary<string, string>()
        {
            {"🍌", "banana"}, {"🎈", "balloon"}, { "📘", "tome" }, {"🏹", "longbow" },
            {"🗡", "sword" }, { "🛡", "shield"}, {"💣","bomb"},
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

        private static bool notAlone = false;

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
            Dictionary<string, Dictionary<string, bool>> listOfWalls = GetWallList();

            for (int row = START_ROW; row < MAX_ROW; row++)
            {
                for (int col = 0; col < MAX_COL; col++)
                {
                    string roomNumber = Flatten2DcoordsTo1D(col, row).ToString();
                    Button newRoom = BuildRoom(roomNumber);
                    Border roomWalls = DrawWalls(roomNumber, listOfWalls[roomNumber]);
                    PositionOnMap(roomWalls, col, row);
                    PositionOnMap(newRoom, col, row);
                }
            }
        }

        private int Flatten2DcoordsTo1D(int col, int row)
        {
            return col + (row * MAX_COL);
        }

        private Button BuildRoom(string roomNumber)
        {
            Button aRoom = new EmptyRoom(roomNumber);
            aRoom.Click += new RoutedEventHandler(EnterRoom_Click);
            return aRoom;
        }

        private Dictionary<string, Dictionary<string, bool>> GetWallList()
        {
            string wallMap = File.ReadAllText(PROJ_WORKING_DIR + WALL_FILE);
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(wallMap);
        }

        private Border DrawWalls(string roomNumber, Dictionary<string, bool> wallSet)
        {
            Border newRoom = new Border();
            newRoom.Name = "Wall" + roomNumber;
            newRoom.BorderThickness = new Thickness()
            {
                Left = wallSet["west"] ? 1 : 0,
                Right = wallSet["east"] ? 1 : 0,
                Top = wallSet["north"] ? 1 : 0,
                Bottom = wallSet["south"] ? 1 : 0
            };
            newRoom.Background = new SolidColorBrush(Colors.BlanchedAlmond);
            newRoom.BorderBrush = new SolidColorBrush(Colors.SaddleBrown);
            return newRoom;
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
            SetUpTextBoxes();
        }

        private void AddPlayer()
        {
            playerLoc = FindEmpty();
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
            // TODO implement exit click handler for ending game
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

        private void SetUpTextBoxes()
        {
            SetUpHeaderBox();
            SetUpDirectionBox();
            SetUpInfoBox();
        }

        private void SetUpHeaderBox()
        {
            HeaderBar.Text = "Welcome to the Dungeon!!!";
            HeaderBar.Foreground = new SolidColorBrush(Colors.Brown);
            HeaderBar.FontSize = 50;
            HeaderBar.VerticalAlignment = VerticalAlignment.Center;
            HeaderBar.HorizontalAlignment = HorizontalAlignment.Center;
            HeaderBar.SetValue(Grid.RowProperty, 0);
            HeaderBar.SetValue(Grid.ColumnSpanProperty, MAX_COL);
        }
        private void SetUpDirectionBox()
        {
            DirectionBox.Text = "To explore the dungeon, use the mouse to click a room adjacent to your avitar! " +
                "Remember that you can't walk through walls, so you'll only be able to access some rooms.";
            DirectionBox.Foreground = new SolidColorBrush(Colors.Brown);
            DirectionBox.FontSize = 14;
            DirectionBox.VerticalAlignment = VerticalAlignment.Center;
            DirectionBox.HorizontalAlignment = HorizontalAlignment.Center;
            DirectionBox.SetValue(Grid.RowProperty, MAX_ROW + 1);
            DirectionBox.SetValue(Grid.ColumnProperty, 1);
            DirectionBox.SetValue(Grid.ColumnSpanProperty, MAX_COL - 2);

            Engage.Click += new RoutedEventHandler(Engage_Click);
        }
        private void SetUpInfoBox()
        {
            InfoBox.Text = "You awaken confused, finding yourself in an empty room with no idea how you arrived here." +
                           " You have one goal - find your way back out of wherever this is.";
            InfoBox.Foreground = new SolidColorBrush(Colors.Brown);
            InfoBox.FontSize = 20;
            InfoBox.VerticalAlignment = VerticalAlignment.Center;
            InfoBox.HorizontalAlignment = HorizontalAlignment.Center;
            InfoBox.SetValue(Grid.RowProperty, MAX_ROW);
            InfoBox.SetValue(Grid.ColumnProperty, 1);
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
            MovePlayer(room);
            if(playerLoc != null)
            {
                ToggleAdjacentRooms(room, true);
                room.IsHitTestVisible = true;
            }
            else
            {
                ToggleAdjacentRooms(room, false);
                HeaderBarWin();
            }
            room.FontSize = notAlone ? 20 : 30;
        }

        private void HeaderBarWin()
        {
            HeaderBar.Text = "Congratulations, you escaped!!!";
        }
        private void HeaderBarLose()
        {
            HeaderBar.Text = "You have failed to escape the dungeon!!!";
        }

        private void Engage_Click(object sender, RoutedEventArgs e)
        {
            Button engagement = sender as Button;
            if (engagement.Tag.ToString() == "EXIT")
            {
                this.Close();
            }
            else
            {
                string inTheRoom = playerLoc.Content.ToString();
                if (inTheRoom.Contains(PLAYER + " | "))
                {
                    inTheRoom = inTheRoom.Remove(0, 6);
                }
                if (monsterDict.TryGetValue(inTheRoom, out _))
                {
                    InfoBox.Text = "Unfortunately you did not have the prowess to best the beast.  You have been slain.";
                    playerLoc.Content = DEATH + " | " + inTheRoom;
                    EngageButtonUpdate("EXIT");
                    ToggleAdjacentRooms(playerLoc, false);
                    HeaderBarLose();
                    playerLoc.IsHitTestVisible = false;
                    playerLoc = null;
                }
                else if (lootDict.TryGetValue(inTheRoom, out inTheRoom))
                {
                    InfoBox.Text = "You have collected a " + inTheRoom;
                    playerLoc.Content = PLAYER;
                    notAlone = false;
                    playerLoc.Foreground = playerColor;
                    playerLoc.FontSize = 30;
                    EngageButtonUpdate("empty");
                }
            }
        }

        private void MovePlayer(Button room)
        {
            if (room == playerLoc)
            {
                return;
            }
            ToggleAdjacentRooms(playerLoc, false);
            room.Content = RoomContents(room);
            playerLoc.Content = null;
            playerLoc.Foreground = null;
            playerLoc = room.Content.ToString() == "EXIT" ? null : room;
        }

        private string RoomContents(Button room)
        {
            if (room.Content == null)
            {
                room.Foreground = playerColor;
                EngageButtonUpdate("empty");
                notAlone = false;
            }
            else if(room.Content.ToString() == PLAYER)
            {
                // TODO - inventory access
            }
            else if (room.Content.ToString() == "EXIT")
            {
                EngageButtonUpdate("EXIT");
                return "EXIT";
            }
            else
            {
                if (lootDict.ContainsKey(room.Content.ToString()))
                {
                    room.Foreground = lootColor;
                    EngageButtonUpdate("item");
                    // TODO - add item to player inventory
                }
                else if (monsterDict.ContainsKey(room.Content.ToString()))
                {
                    room.Foreground = monsterColor;
                    EngageButtonUpdate("monster");
                    // TODO - implement full battle mechanic
                }
                notAlone = true;
                return PLAYER + " | " + room.Content.ToString();
            }
            return PLAYER;
        }

        private void ToggleAdjacentRooms(Button aRoom, bool toggleChange)
        {
            Dictionary<string, int> coords = GetGridCoords(aRoom);
            Dictionary<string, bool> wallList = WallPositions(Flatten2DcoordsTo1D(coords["col"], coords["row"]));
            List<string> roomsToToggle = GetAdjacentNames(coords, wallList);
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

        private Dictionary<string, bool> WallPositions(int roomNumber)
        {
            Border wallSet = FindWallSet("Wall" + roomNumber);
            Dictionary<string, bool> walls = new Dictionary<string, bool>()
            {
                {"west", wallSet.BorderThickness.Left.ToString() == "1"},
                {"north", wallSet.BorderThickness.Top.ToString() == "1"},
                {"east", wallSet.BorderThickness.Right.ToString() == "1"},
                {"south", wallSet.BorderThickness.Bottom.ToString() == "1"}

            };
            return walls;
        }

        private Border FindWallSet(string wallSetName)
        {
            return map.Children.OfType<Border>().FirstOrDefault(room => room.Name == wallSetName);
        }

        private List<string> GetAdjacentNames(Dictionary<string, int> coords, Dictionary<string, bool> walls)
        {
            List<string> roomList = new List<string>();

            if (!walls["west"])
            {
                roomList.Add(MakeRoomName(coords["col"] - 1, coords["row"]));
            }

            if (!walls["east"])
            {
                roomList.Add(MakeRoomName(coords["col"] + 1, coords["row"]));
            }

            if (!walls["north"])
            {
                roomList.Add(MakeRoomName(coords["col"], coords["row"] - 1));
            }

            if (!walls["south"])
            {
                roomList.Add(MakeRoomName(coords["col"], coords["row"] + 1));
            }
            return roomList;
        }

        private string MakeRoomName(int col, int row)
        {
            return "Room" + Flatten2DcoordsTo1D(col, row);
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
            if(lookingfor == "EXIT")
            {
                return "EXIT";
            }
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
            LookItUp.IsHitTestVisible = true;
            LookUpText.Text = "Click to look up the meaning of " + searchTerm;
            LookItUp.Tag = searchTerm;
        }

        private void EngageButtonUpdate(string roomContentsType)
        {
            if (roomContentsType != "empty")
            {
                Engage.IsHitTestVisible = true;
                Engage.Tag = roomContentsType;
                EngageText.Text = roomContentsType == "monster" ? "Fight" :
                                  roomContentsType == "item" ? "Pick Up" :
                                  roomContentsType == "EXIT" ? "Leave" : "Error";
            }
            else
            {
                Engage.IsHitTestVisible = false;
                EngageText.Text = "";
            }
        }

        private void ConnectDictService()
        {
            LookItUp.Click += new RoutedEventHandler(LookUp_Click);
            LookItUp.IsHitTestVisible = false;
        }

        private void LookUp_Click(object sender, RoutedEventArgs e)
        {
            Button request = sender as Button;
            string searchTerm = request.Tag.ToString();
            string definition = CheckDict(searchTerm);
            List<string> listOfDefinitions = JsonSerializer.Deserialize<List<string>>(definition);
            // TODO - exception handling for corrupt or incomplete .json files from service
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