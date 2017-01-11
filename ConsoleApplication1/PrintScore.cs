using System;
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

            Console.WriteLine("Mais ativo : {0}", score.MostActiveIndex());
            Console.WriteLine("Mais ativo (pausas) : {0}", score.MostActiveIndex(true));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Menos ativo : {0}", score.LeastActiveIndex());
            Console.WriteLine("Menos ativo (pausas) : {0}", score.LeastActiveIndex(true));
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("RoleCounts :");
            List<int> counts = score.RoleCounts();
            foreach ( var num in counts) {
                Console.Write(" {0}", num);
            }
            Console.WriteLine();
        }
    }
}
