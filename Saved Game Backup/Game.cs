﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saved_Game_Backup
{
    public class Game {
        public string Name;
        public string Path;

        public Game() {
            
        }

        public Game(string name, string path) {
            Name = name;
            Path = path;
        }

        public override string ToString() {
            return Name;
        }
    }
}