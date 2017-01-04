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
        //Create a random list of ints with the first three between 1 and 12 and the last two between 1 and 24
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

        //Perform random operations on the random list, a minimum of two and a maximum of four operations are aloud
        //If an operation is illegal (- or /) do not count as an operation 
        public void PerformRandomOperations(List<int> inputList)
        {
            //clone the list so we do not modify the original list
            List<int> cloneList = new List<int>(inputList);
            //Get a random number of operations to perform on the list
            int numberOfOperations = _random.Next(2, 5);
            for (int i = 1; i < numberOfOperations; i++)
            {
                //Get 2 random numbers from the list and initialise the int for the result
                var numbers = GetTwoAtRandomIndexes(cloneList);
                int a = numbers[0];
                int b = numbers[1];
                int c;
                //Determine which operation is to be performed next
                int indexOfOperation = _random.Next(1, 5);
                switch(indexOfOperation)
                {
                    case 1:
                        c = a + b;
                        cloneList.Add(c);
                        break;
                    case 2:
                        c = a * b;
                        cloneList.Add(c);
                        break;
                    case 3:
                        if (a < b)
                        {
                            c = b - a;
                        }
                        else if (a > b)
                        {
                            c = a - b;
                        }
                        //If a = b the substraction can not be performed so we put back the ints in the list, decrease the counter and try a different operation
                        else
                        {
                            cloneList.Add(a);
                            cloneList.Add(b);
                            i--;
                            continue;
                        }
                        cloneList.Add(c);
                        break;

                    case 4:
                        if (a % b == 0)
                        {
                            c = a / b;
                        }
                        else if (b % a == 0)
                        {
                            c = b / a;
                        }
                        //If a%b or b%a != 0 the division can not be performed so we put back the ints in the list, decrease the counter and try a different operation
                        else
                        {
                            cloneList.Add(a);
                            cloneList.Add(b);
                            i--;
                            continue;
                        }
                        cloneList.Add(c);
                        break;
                }
            }
            //after all the operations are performed the result is the list int of the clone string as the result is always added to the list last
            _result = cloneList[cloneList.Count-1];
        }
        //Get two ints from the list and remove them from said list
        public int[] GetTwoAtRandomIndexes(List<int> inputList)
        {
            //get the first random index and retrieve the number present at the index in the list before removing it from the list
            int index = _random.Next(0, inputList.Count);
            int a = inputList[index];
            inputList.RemoveAt(index);
            //Do the same for the second int
            index = _random.Next(0, inputList.Count);
            int b = inputList[index];
            inputList.RemoveAt(index);
            //return the two ints we obtained
            return new int[] {a,b};
        }

        //Write a string to the file
        private void WriteListToFile(string input, string path)
        {
            File.AppendAllLines(path, new List<string> { ToString(), input });
        }

        //Override the ToString method to get the desired output string
        public override string ToString()
        {
            string str = "";
            foreach (int item in _output)
            {
                str += Convert.ToString(item) + "-";
            }
            str += "res : " +  _result;
            return str;
        }

        //Write a list of ints to a string
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
            //Empty the list before generating another
            File.WriteAllText(path, String.Empty);
            for (int i = 0; i < amount; i++)
            {
                //Perform random operations on random lists
                PerformRandomOperations(RandomList());
                //if the result is less than 100 write the list to the file
                if (_result <= 100)
                {
                    WriteListToFile("", path);
                }
                //else skip this list and loop one more time
                else
                {
                    i--;
                }
            }
        }
    }


}
