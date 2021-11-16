﻿using System;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MAX_COL = 10;
        const int MAX_ROW = 5;

        const string PLAYER = "🧙‍";

        int [] playerLoc = new int[2]; // col, row

        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            SetUpGame();
        }

        private void SetUpGame()
        {
            List<string> monsters = new List<string>()
            {
                "ᓚᘏᗢ","👻","🕷", "🦄"
            };

            List<string> resources = new List<string>()
            {
                "🍌", "🎈", "📘", "🏹", "🗡", "🛡", "💣","🕯"
            };

            // string death = "☠";

            for (int col = 0; col < MAX_COL; col++)
            {
                for (int row = 0; row < MAX_ROW; row++)
                {
                    // stuff
                    int north = col == 0 ? 0 : random.Next(2);
                    int south = col == MAX_COL - 1 ? 0 : random.Next(2);
                    int east = row == MAX_ROW ? 0 : random.Next(2);
                    int west = row == 0 ? 0 : random.Next(2);

                    Border roomWalls = new Border
                    {
                        BorderThickness = new Thickness(west, north, east, south),
                        BorderBrush = new SolidColorBrush(Colors.Black)
                    };
                    roomWalls.SetValue(Grid.ColumnProperty, col);
                    roomWalls.SetValue(Grid.RowProperty, row);
                    _ = map.Children.Add(roomWalls);

                    Button roomContents = new Button();
                    int flat = Flatten(col, row);
                    if (flat == 0)
                    {
                        roomContents.Tag = "show";
                        roomContents.Content = PLAYER;
                        roomContents.Foreground = new SolidColorBrush(Colors.Blue);
                        roomContents.Click += new RoutedEventHandler(EnterRoom_Click);
                        playerLoc[0] = col;
                        playerLoc[1] = row;
                        roomContents.IsEnabled = true;
                    }
                    // monster gen
                    else
                    {
                        if (flat % 5 == 0)
                        {
                            int selection = random.Next(monsters.Count);
                            roomContents.Tag = monsters[selection];
                            roomContents.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        // item gen
                        else if (flat % 7 == 0)
                        {
                            int selection = random.Next(resources.Count);
                            roomContents.Tag = resources[selection];
                            roomContents.Foreground = new SolidColorBrush(Colors.Green);
                        }
                        //roomContents.Content = "?";
                        roomContents.Content = "" + flat;
                        roomContents.IsEnabled = false;
                    }
                    roomContents.Name = "room" + flat.ToString();
                    roomContents.FontSize = 30;
                    roomContents.Height = 76;
                    roomContents.Width = 76;
                    roomContents.SetValue(Grid.ColumnProperty, col);
                    roomContents.SetValue(Grid.RowProperty, row);
                    roomContents.BorderThickness = new Thickness(0);
                    roomContents.Background = new SolidColorBrush(Colors.Tan);
                    roomContents.Click += new RoutedEventHandler(EnterRoom_Click);

                    
                    _ = map.Children.Add(roomContents);
                }
            }

            OpenRooms(playerLoc[0], playerLoc[1]);

            LookItUp.Click += new RoutedEventHandler(LookUp_Click);
            InfoBox.Text = "Welcome to the Dungeon!!!";
            InfoBox.Foreground = new SolidColorBrush(Colors.Brown);
            InfoBox.FontSize = 50;
            InfoBox.VerticalAlignment = VerticalAlignment.Center;
            InfoBox.HorizontalAlignment = HorizontalAlignment.Center;
            InfoBox.SetValue(Grid.RowProperty, MAX_ROW);
            InfoBox.SetValue(Grid.ColumnSpanProperty, MAX_COL - 1);
            LookItUp.SetValue(Grid.RowProperty, MAX_ROW);
            LookItUp.SetValue(Grid.ColumnProperty, MAX_COL);
        }

        private int Flatten(int col, int row)
        {
            return col + row * MAX_COL;
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
                int westCoord = Flatten(col - 1, row);
                string roomName = "room" + westCoord;
                Button west = map.Children.OfType<Button>().FirstOrDefault(btn => btn.Name == roomName);
                openlist.Add(west);
            }
            if (col != MAX_COL - 1)
            {
                int eastCoord = Flatten(col + 1, row);
                string roomName = "room" + eastCoord;
                Button east = map.Children.OfType<Button>().FirstOrDefault(btn => btn.Name == roomName);
                openlist.Add(east);
            }
            if (row != 0)
            {
                int northCoord = Flatten(col, row - 1);
                string roomName = "room" + northCoord;
                Button north = map.Children.OfType<Button>().FirstOrDefault(btn => btn.Name == roomName);
                openlist.Add(north);
            }
            if (row != MAX_ROW - 1)
            {
                int southCoord = Flatten(col, row + 1);
                string roomName = "room" + southCoord;
                Button south = map.Children.OfType<Button>().FirstOrDefault(btn=> btn.Name == roomName);
                openlist.Add(south);
            }
            foreach(Button room in openlist)
            {
                if(!room.IsEnabled)
                {
                    room.IsEnabled = true;
                }
            }
        }
    }
}