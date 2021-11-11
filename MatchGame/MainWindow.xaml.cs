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
                    int flat = col + row + col*row;
                    if (flat == 0)
                    {
                        roomContents.Tag = "show";
                        roomContents.Content = PLAYER;
                        roomContents.Foreground = new SolidColorBrush(Colors.Blue);
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
                        roomContents.Content = "?";
                    }
                    roomContents.FontSize = 30;
                    roomContents.Height = 76;
                    roomContents.Width = 76;
                    roomContents.SetValue(Grid.ColumnProperty, col);
                    roomContents.SetValue(Grid.RowProperty, row);
                    roomContents.BorderThickness = new Thickness(0);
                    roomContents.Background = new SolidColorBrush(Colors.Tan);
                    roomContents.Click += new RoutedEventHandler(this.EnterRoom_Click);
                    _ = map.Children.Add(roomContents);
                }
            }

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

        private void EnterRoom_Click(object sender, RoutedEventArgs e)
        {
            Button room = sender as Button;

            Dictionary<string, string> symbolNamesDict = new Dictionary<string, string>()
            {
                { "🦄", "Corrupted Unicorn"}, {"ᓚᘏᗢ", "Sphinx"}, { "🕷", "Spider"},
                {"👻", "Ghost"}, {"🍌", "banana"}, {"🎈", "mysterious balloon"},
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
                Button dictCall = new Button();
                dictCall.Content = dictLookup;
                inTheRoom = "You've found a " + dictLookup;
                LookUpText.Text = "Click to look up the meaning of " + dictLookup;
                LookItUp.Tag = dictLookup;
                LookItUp.BorderThickness = new Thickness(2);
                LookItUp.Click += new RoutedEventHandler(this.LookUp_Click);
            }
            InfoBox.Text = inTheRoom;
            InfoBox.FontSize = 20;
        }

        private void LookUp_Click(object sender, RoutedEventArgs e)
        {
            Button request = sender as Button;
            // connect to dictionary service
        }
    }
}