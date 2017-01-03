using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MathadorLib
{
    public class Generator
    {
        readonly Random _random = new Random();
        private List<int> _output = new List<int>();
        private int _result;

        public Generator()
        {
        }
        public List<int> RandomList()
        {
            _output = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                _output.Add(_random.Next(1,13));
            }
            for (int i = 0; i < 2; i++)
            {
                _output.Add(_random.Next(1, 25));
            }

            return _output;
        }

        public void PerformRandomOperations(List<int> inputList)
        {
            List<int> cloneList = new List<int>(inputList);
            int numberOfOperations = _random.Next(2, 5);
            for (int i = 1; i < numberOfOperations; i++)
            {
                var numbers = GetTwoAtRandomIndexes(cloneList);
                int a = numbers[0];
                int b = numbers[1];
                int c;
                int indexOfOperation = _random.Next(1, 5);
                for (int j = 0; j < 4; j++)
                {
                    if (j == 0)
                    {
                        c = a + b;
                        WriteListToFile(ListToString(cloneList), @"C:\VisualStudioProjects\CoursDotNet\data\generator.txt");
                        File.AppendAllLines(@"C:\VisualStudioProjects\CoursDotNet\data\generator.txt", new List<string> { Convert.ToString(a) + "+" + Convert.ToString(b) });
                        cloneList.Add(c);
                    }
                    else if (j == 1)
                    {
                        c = a * b;
                        WriteListToFile(ListToString(cloneList), @"C:\VisualStudioProjects\CoursDotNet\data\generator.txt");
                        File.AppendAllLines(@"C:\VisualStudioProjects\CoursDotNet\data\generator.txt", new List<string> { Convert.ToString(a) + "x" + Convert.ToString(b) });
                        cloneList.Add(c);
                    }
                    else if (j == 2)
                    {
                        if (a < b)
                        {
                            c = b - a;

                            WriteListToFile(ListToString(cloneList), @"C:\VisualStudioProjects\CoursDotNet\data\generator.txt");
                            File.AppendAllLines(@"C:\VisualStudioProjects\CoursDotNet\data\generator.txt", new List<string> { Convert.ToString(b) + "-" + Convert.ToString(a) });
                        }
                        else if (a > b)
                        {
                            c = a - b;
                            WriteListToFile(ListToString(cloneList), @"C:\VisualStudioProjects\CoursDotNet\data\generator.txt");
                            File.AppendAllLines(@"C:\VisualStudioProjects\CoursDotNet\data\generator.txt", new List<string> { Convert.ToString(a) + "-" + Convert.ToString(b) });
                        }
                        else
                        {
                            continue;
                        }
                        cloneList.Add(c);
                    }
                     
                    
                 else if (j == 4)
                    {
                        if (a % b == 0)
                        {
                            c = a / b;
                            WriteListToFile(ListToString(cloneList), @"C:\VisualStudioProjects\CoursDotNet\data\generator.txt");
                            File.AppendAllLines(@"C:\VisualStudioProjects\CoursDotNet\data\generator.txt", new List<string> { Convert.ToString(a) + "/" + Convert.ToString(b) });
                        }
                        else if (b % a == 0)
                        {
                            c = b / a;
                            WriteListToFile(ListToString(cloneList), @"C:\VisualStudioProjects\CoursDotNet\data\generator.txt");
                            File.AppendAllLines(@"C:\VisualStudioProjects\CoursDotNet\data\generator.txt", new List<string> { Convert.ToString(b) + "/" + Convert.ToString(a) });
                        }
                        else
                        {
                            continue;
                        }
                        cloneList.Add(c);
                    }
                }
            }
            _result = cloneList[cloneList.Count-1];
        }

        public int[] GetTwoAtRandomIndexes(List<int> inputList)
        {
            int index = _random.Next(0, inputList.Count);
            int a = inputList[index];
            inputList.RemoveAt(index);
            index = _random.Next(0, inputList.Count);
            int b = inputList[index];
            inputList.RemoveAt(index);
            return new int[] {a,b};
        }

        private void WriteListToFile(string input, string path)
        {
            File.AppendAllLines(path, new List<string> { ToString(), Environment.NewLine });
        }

        public override string ToString()
        {
            string str = "";
            foreach (int item in _output)
            {
                str += Convert.ToString(item) + "-";
            }
            str += _result;
            return str;
        }

        public string ListToString(List<int> inputlist)
        {
            string str = "";
            foreach (int item in inputlist)
            {
                str += Convert.ToString(item) + "-";
            }
            str += " res : " + _result;
            return str;
        }
        public void GenerateRandomLists(int amount, string path)
        {
            for (int i = 0; i < amount; i++)
            {
                PerformRandomOperations(RandomList());

                WriteListToFile(Environment.NewLine, path);
            }
        }
    }


}
