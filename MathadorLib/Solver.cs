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
        private Tuple<List<int>, string, int, int> list;
        private Tuple<List<int>, string, int, int> bestResult = new Tuple<List<int>, string, int, int>(new List<int>(),"",0,0);
        //Path for Data File
        //List of lists obtained for each cycle
        private List<Tuple<List<int>, string, int, int>> listOfTuples;
        private int expectedRes;
        public Solver(List<int> list, int res)
        {
            expectedRes = res;
            List<Tuple<List<int>, string>> DistinctResultList = new List<Tuple<List<int>, string>> { };
            //Set path for saved Data
            string path = @"C:\VisualStudioProjects\CoursDotNet\data\solver.txt";
            File.WriteAllText(path, string.Empty);

            //initialise the tuples and variables
            this.list = new Tuple<List<int>, string, int, int>(list, "", 0, 0);

            int listSize = this.list.Item1.Count;
            //Create Regex for result
            string regStr = " " + Convert.ToString(res) + "$";
            var lastNumber = new Regex(@regStr);

            //Remove duplicates from the list of tuples
            listOfTuples = RemoveDuplicates(AllPossibleResultsOneCycle(this.list));

            //Maximum allowed result


            //There are 4 cycles of operations so we loop 3 times (first cycle is already done above)
            for (int j = 0; j < listSize - 1; j++)
            {
                //Write the solutions to the file
                /*
       foreach (Tuple<List<int>, string, int, int> element in listOfTuples)
       {
           //File.AppendAllLines(pathData, new List<string> { listToString(element.Item1), element.Item2 });

           if (lastNumber.IsMatch(element.Item2))
           {
               File.AppendAllLines(path, new List<string> { listToString(element.Item1), element.Item2,"Points : " + Convert.ToString(element.Item3), Environment.NewLine });
           }
     
            }
                  */
                //Initialise a new resultList
                List<Tuple<List<int>, string, int, int>> ResultList = new List<Tuple<List<int>, string, int, int>> { };

                //Get a list From the list of lists returned by the last cycle
                foreach (Tuple<List<Int32>, string, int, int> element in listOfTuples)
                {
                    foreach (Tuple<List<int>, string, int, int> cycleResultList in AllPossibleResultsOneCycle(element))
                    {
                        //Add the returned list to the new result list
                        ResultList.Add(cycleResultList);
                    }
                }
                //Remove the duplicates before starting the next cycle
                listOfTuples = RemoveDuplicates(ResultList);
            }
            File.AppendAllLines(path, new List<string> { listToString(bestResult.Item1), bestResult.Item2, "Points : " + Convert.ToString(bestResult.Item3), Environment.NewLine });
        }

        //Return a list of all the possible outcomes from one operation cycle
        public List<Tuple<List<int>, string, int, int>> AllPossibleResultsOneCycle(Tuple<List<int>, string, int, int> tuple)
        {
            int res = 0;
            //Operations.Add(Environment.NewLine + listToString(list));
            List<Tuple<List<int>, string, int, int>> ResultList = new List<Tuple<List<int>, string, int, int>> { };


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
                                CreateAndAddNewList(i, j, res, "+", tuple, ResultList);
                            }
                            else if (k == 1)
                            {
                                res = a * b;
                                CreateAndAddNewList(i, j, res, "x", tuple, ResultList);
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
                                    CreateAndAddNewList(i, j, res, "/", tuple, ResultList);
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
                                    CreateAndAddNewList(i, j, res, "-", tuple, ResultList);
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
            ResultList = RemoveDuplicates(ResultList);
            return ResultList;
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
            if (outputTuple.Item3 > bestResult.Item3 && res == expectedRes)
            {
                bestResult = outputTuple;
            }
            resultTuple.Add(outputTuple);
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

        public string listToString(List<int> list)
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