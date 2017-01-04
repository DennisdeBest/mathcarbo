using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MathadorLib;

namespace Mathador
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create the database if it does not exist
            SQLiteConnection.CreateFile("mathcarbo.sqlite");
            //Solve();
            //Generate random lists of ints and the results
            List<List<int>> generatorResults = Generate();
            Game game = new Game(generatorResults);

        }
        private static void Solve()
        {
            List<int> list = new List<int> { 5, 11, 11, 15, 21 };
            Solver solver = new Solver(list, 2536);
        }

        private static List<List<int>> Generate()
        {
            Generator generator = new Generator();
            string path = "generator.txt";
            generator.GenerateRandomLists(100, path);
            //return generator.ReadFromDB();
            return generator.ReadFromFile(path);
        }
    }
}
