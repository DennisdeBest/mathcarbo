using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace MathadorLib
{
    public class Solver
    {
        private Tuple<List<int>, string, int, int> _list;
        private Tuple<List<int>, string, int, int> _bestResult = new Tuple<List<int>, string, int, int>(new List<int>(),"",0,0);
        //Path for Data File
        //List of lists obtained for each cycle
        private List<Tuple<List<int>, string, int, int>> _listOfTuples;
        private int _expectedRes;
        public Solver(List<int> list, int res)
        {
            _expectedRes = res;
            List<Tuple<List<int>, string>> distinctResultList = new List<Tuple<List<int>, string>> { };
            //Set path for saved Data
            string path = @"C:\VisualStudioProjects\CoursDotNet\data\solver.txt";
            File.WriteAllText(path, string.Empty);

            //initialise the tuples and variables
            this._list = new Tuple<List<int>, string, int, int>(list, "", 0, 0);

            int listSize = this._list.Item1.Count;
            //Create Regex for result
            string regStr = " " + Convert.ToString(res) + "$";
            var lastNumber = new Regex(@regStr);

            //Remove duplicates from the list of tuples
            _listOfTuples = RemoveDuplicates(AllPossibleResultsOneCycle(this._list));

            //Maximum allowed result


            //There are 4 cycles of operations so we loop 3 times (first cycle is already done above)
            for (int j = 0; j < listSize - 1; j++)
            {
                //Initialise a new resultList
                List<Tuple<List<int>, string, int, int>> resultList = new List<Tuple<List<int>, string, int, int>> { };

                //Get a list From the list of lists returned by the last cycle
                foreach (Tuple<List<Int32>, string, int, int> element in _listOfTuples)
                {
                    foreach (Tuple<List<int>, string, int, int> cycleResultList in AllPossibleResultsOneCycle(element))
                    {
                        //Add the returned list to the new result list
                        resultList.Add(cycleResultList);
                    }
                }
                //Remove the duplicates before starting the next cycle
                _listOfTuples = RemoveDuplicates(resultList);
            }
            File.AppendAllLines(path, new List<string> { ListToString(_bestResult.Item1), _bestResult.Item2, "Points : " + Convert.ToString(_bestResult.Item3), Environment.NewLine });
        }

        //Return a list of all the possible outcomes from one operation cycle
        public List<Tuple<List<int>, string, int, int>> AllPossibleResultsOneCycle(Tuple<List<int>, string, int, int> tuple)
        {
            int res = 0;
            //Operations.Add(Environment.NewLine + listToString(list));
            List<Tuple<List<int>, string, int, int>> resultList = new List<Tuple<List<int>, string, int, int>> { };


            for (int i = 0; i < tuple.Item1.Count; i++)
            {
                for (int j = 0; j < tuple.Item1.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    else
                    {
                        int a = tuple.Item1[i];
                        int b = tuple.Item1[j];
                        for (int k = 0; k < 4; k++)
                        {
                            if (k == 0)
                            {
                                res = a + b;
                                CreateAndAddNewList(i, j, res, "+", tuple, resultList);
                            }
                            else if (k == 1)
                            {
                                res = a * b;
                                CreateAndAddNewList(i, j, res, "x", tuple, resultList);
                            }
                            else if (k == 2)
                            {
                                if (b != 0 && a % b == 0)
                                {
                                    if (a == 0)
                                    {
                                        res = 0;
                                    }
                                    else
                                    {
                                        res = a / b;
                                    }
                                    CreateAndAddNewList(i, j, res, "/", tuple, resultList);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (k == 3)
                            {
                                if (a > b)
                                {
                                    res = a - b;
                                    CreateAndAddNewList(i, j, res, "-", tuple, resultList);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                    }
                }

            }
            resultList = RemoveDuplicates(resultList);
            return resultList;
        }

        //Create a new list from the old one without the two values used to calculate the result but with the result
        private void CreateAndAddNewList(int i, int j, int res, string symbol, Tuple<List<int>, string, int, int> tuple, List<Tuple<List<int>, string, int, int>> resultTuple)
        {
            var listClone = new List<int>(tuple.Item1);
            string resultString = "";
            if (tuple.Item2 != "")
            {
                resultString += Environment.NewLine;
            }
            resultString += CreateOperationString(listClone[i], listClone[j], symbol, res);

            listClone.RemoveAt(i);
            if (i < j)
            {
                listClone.RemoveAt(j - 1);
            }
            else
            {
                listClone.RemoveAt(j);
            }
            listClone.Add(res);
            int points = tuple.Item3;
            switch (symbol)
            {
                case "+":
                    points += 1;
                    break;
                case "-":
                    points += 2;
                    break;
                case "x":
                    points += 1;
                    break;
                case "/":
                    points += 3;
                    break;
            }
            Tuple<List<int>, string, int, int> outputTuple = new Tuple<List<int>, string, int, int>(listClone, tuple.Item2 + resultString, points, 0);
            if (res == _expectedRes && outputTuple.Item3 > _bestResult.Item3)
            {

                    _bestResult = outputTuple;
            }
            //If the output tuple uses all operations set it as the best solution
            if ( res == _expectedRes && isMathador(outputTuple))
            {
                Tuple<List<int>, string, int, int> mathadorTuple =
                    new Tuple<List<int>, string, int, int>(outputTuple.Item1, outputTuple.Item2,
                        outputTuple.Item3 + 5, 1);
                _bestResult = mathadorTuple;
            }
            resultTuple.Add(outputTuple);
        }

        private bool isMathador(Tuple<List<int>, string, int, int> input)
        {
            if (input.Item1.Count == 1)
            {
                List<string> operations = new List<string> {"x", "-", "/", "+"};
                if (operations.All(s => input.Item2.Contains(s)))
                {
                    return true;
                }
            }
            return false;
        }
        private string CreateOperationString(int a, int b, string operand, int res)
        {
            if (operand == "x" || operand == "+")
            {
                if (a < b)
                {
                    int c = a;
                    a = b;
                    b = c;
                }
            }
            return Convert.ToString(a) + " " + operand + " " + Convert.ToString(b) + " = " + Convert.ToString(res);
        }

        public string ListToString(List<int> list)
        {
            string s = "";
            foreach (int element in list)
            {
                s += Convert.ToString(element) + " - ";
            }
            return s;
        }

        private List<Tuple<List<int>, string, int, int>> RemoveDuplicates(List<Tuple<List<int>, string, int, int>> inputList)
        {
            //Duplicate the input list and create a new output list
            List<Tuple<List<int>, string, int, int>> cloneList = new List<Tuple<List<int>, string, int, int>>(inputList);
            List<Tuple<List<int>, string, int, int>> outputList = new List<Tuple<List<int>, string, int, int>> { };
            List<Tuple<List<int>, string, int, int>> outputList2 = new List<Tuple<List<int>, string, int, int>> { };
            for (int i = 0; i < cloneList.Count - 1; i++)
            {
                Tuple<List<int>, string, int, int> currentTupleI = cloneList[i];

                for (int j = 0; j < cloneList.Count - 1; j++)
                {
                    Tuple<List<int>, string, int, int> currentTupleJ = cloneList[j];
                    if (i == j)
                    {
                    }
                    else if (currentTupleI.Item2 == currentTupleJ.Item2)
                    {
                        cloneList.RemoveAt(j);
                    }
                }
            }
            return cloneList;
        }
    }
}