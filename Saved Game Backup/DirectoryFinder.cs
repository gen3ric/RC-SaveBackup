﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace Saved_Game_Backup
{
    public class DirectoryFinder {

        private Dictionary<string, string> _gameSaveDirectories;
        public Dictionary<string, string> GameSaveDirectories {
            get {
                return _gameSaveDirectories;
            }
            set {
                _gameSaveDirectories = value;
            }
        }

        public DirectoryFinder() {
            GenerateDictionary();
        }

        private void GenerateDictionary() {
            _gameSaveDirectories.Add("Terraria", @"C:\Users\Rob\Documents\My Games\Terraria");
        }

        public string FindDirectory(string game) {
            
            if(_gameSaveDirectories.ContainsKey(game))
                return _gameSaveDirectories["game"];
            return "Unknown Game";
            //return _gameSaveDirectories.TryGetValue(game, out "Unknown Game");
        }

        public static ObservableCollection<Game> ReturnGamesList() {
            var gamesList = new ObservableCollection<Game>();
            var lines = File.ReadAllLines(@"C:\Users\Rob\Documents\Visual Studio 2012\Projects\Saved Game Backup\Saved Game Backup\Games.csv");
            foreach (string s in lines) {
                var data = s.Split(','); //Can't split on a space
                gamesList.Add(new Game(data[0], data[1]));
            }
            return gamesList;
        }

        public static string SpecifyFolder() {
            var dialog = new VistaFolderBrowserDialog();
            dialog.ShowDialog();
            return dialog.SelectedPath;
        }

        public static ObservableCollection<string> ReturnGameNames() {
            var gamesNames = new ObservableCollection<string>();
            var lines = File.ReadAllLines(@"C:\Users\Rob\Documents\Visual Studio 2012\Projects\Saved Game Backup\Saved Game Backup\Games.csv");
            foreach (string s in lines)
            {
                var data = s.Split(','); //Can't split on a space
                gamesNames.Add(data[0]);
            }
            return gamesNames;
        }
    }
}