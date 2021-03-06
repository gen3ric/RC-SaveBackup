﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Saved_Game_Backup.ViewModel;

namespace Saved_Game_Backup
{
    public class PrefSaver {

        public PrefSaver(){}

        public UserPrefs LoadPrefs() {
            UserPrefs prefs;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Save Backup Tool\\";
            if (Directory.Exists(path))
            {
                using (Stream input = File.OpenRead(path + "UserPrefs.dat")) {
                    var bf = new BinaryFormatter();
                    prefs = (UserPrefs)bf.Deserialize(input);
                }

                return prefs;
            }

            MessageBox.Show("If you see this, something went wrong \r\nloading user preferences.");
            return new UserPrefs(0,5, "", new ObservableCollection<Game>());
        }

        public void SavePrefs(UserPrefs prefs) {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Save Backup Tool\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (Stream output = File.Create(path + "UserPrefs.dat")) {
                var bf = new BinaryFormatter();
                bf.Serialize(output, prefs);

            }

        }

        public static bool CheckForPrefs() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Save Backup Tool\\UserPrefs.dat";
            if (File.Exists(path))
                return true;
            return false;
        }

    }
}
