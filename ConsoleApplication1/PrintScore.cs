﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicScore;

namespace PrintScore {
    class Program {
        static void Main(string[] args) {
            MusicScore.Score score = MusicScore.ScoreBuilder.FromXML(args[0]);
            string name = args[0].Remove(args[0].Length - 4);
            score.PrintPartWise(name + "_PARTWISE.txt");
            score.PrintTimeWise(name + "_TIMEWISE.txt");
        }
    }
}