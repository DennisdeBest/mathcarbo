using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathadorLib;

namespace Mathador
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int>{7,8,4,3,2};
            Solver solver = new Solver(list, 23);
        }
    }
}
