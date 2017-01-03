using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
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
            //Solve();
            Generate();
        }
        private static void Solve()
        {
            List<int> list = new List<int> { 1, 7, 1, 16, 23 };
            Solver solver = new Solver(list, 208);
        }

        private static void Generate()
        {
            Generator generator = new Generator();
            string path = @"C:\VisualStudioProjects\CoursDotNet\data\generator.txt";
            generator.GenerateRandomLists(100, path);
        }
    }
}
