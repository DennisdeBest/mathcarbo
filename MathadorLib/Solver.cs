using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MathadorLib
{
    public class Solver
    {
        //Save onyl the best result
        private Tuple<List<int>, string, int, int> _bestResult = new Tuple<List<int>, string, int, int>(
            new List<int>(), "", 0, 0);

        private int _expectedRes;
        private Tuple<List<int>, string, int, int> _list;
        //List of tuples obtained for each cycle
        private List<Tuple<List<int>, string, int, int>> _listOfTuples;
        private bool _mathadorFound;


        //Solve from - separted string
        public List<string> SolveFromString(string input)
        {
            //The last number is the expected tesult, the others are the list of numbers to get there
            var explodedInput = input.Split('-');
            var explodedInputList = explodedInput.Select(int.Parse).ToList();
            _expectedRes = explodedInputList[explodedInputList.Count - 1];
            explodedInputList.RemoveAt(explodedInput.Length - 1);
            _mathadorFound = false;

            //initialise the tuples and variables
            _list = new Tuple<List<int>, string, int, int>(explodedInputList, "", 0, 0);

            //Get the amount of numbers available to determine the amount of solving iterations need to be performed
            //around 1sec for 4 numbers, 8secs for 5 and over a minute for 6
            var listSize = _list.Item1.Count;

            //Perform first iteration and remove duplicates from the list of tuples
            _listOfTuples = RemoveDuplicates(AllPossibleResultsOneCycle(_list));

            //There are 4 cycles of operations so we loop 3 times (first cycle is already done above)
            for (var j = 0; j < listSize - 1; j++)
            {
                //Initialise a new resultList
                var resultList = new List<Tuple<List<int>, string, int, int>>();

                //Get a list From the list of lists returned by the last cycle
                foreach (var element in _listOfTuples)
                {
                    if (!_mathadorFound)
                    {
                        foreach (var cycleResultList in AllPossibleResultsOneCycle(element))
                        {
                            resultList.Add(cycleResultList);
                        }
                    }  
                }

                //Remove the duplicates before starting the next cycle
                _listOfTuples = RemoveDuplicates(resultList);
            }
            return new List<string> {_bestResult.Item2, Convert.ToString(_bestResult.Item3)};
        }

        //Return a list of all the possible outcomes from one operation cycle
        public List<Tuple<List<int>, string, int, int>> AllPossibleResultsOneCycle(
            Tuple<List<int>, string, int, int> tuple)
        {
            var resultList = new List<Tuple<List<int>, string, int, int>>();

            for (var i = 0; i < tuple.Item1.Count; i++)
                for (var j = 0; j < tuple.Item1.Count; j++)
                    if (i == j)
                    {
                    }
                    else
                    {
                        var a = tuple.Item1[i];
                        var b = tuple.Item1[j];
                        for (var k = 0; k < 4; k++)
                        {
                            var res = 0;
                            if (k == 0)
                            {
                                res = a + b;
                                CreateAndAddNewList(i, j, res, "+", tuple, resultList);
                            }
                            else if (k == 1)
                            {
                                res = a*b;
                                CreateAndAddNewList(i, j, res, "x", tuple, resultList);
                            }
                            else if (k == 2)
                            {
                                if ((b != 0) && (a%b == 0))
                                {
                                    if (a == 0)
                                        res = 0;
                                    else
                                        res = a/b;
                                    CreateAndAddNewList(i, j, res, "/", tuple, resultList);
                                }
                            }
                            else if (k == 3)
                            {
                                if (a > b)
                                {
                                    res = a - b;
                                    CreateAndAddNewList(i, j, res, "-", tuple, resultList);
                                }
                            }
                        }
                    }
            resultList = RemoveDuplicates(resultList);
            return resultList;
        }

        //Create a new list from the old one without the two values used to calculate the result but with the result
        //Tuples can't be modified so a list is created from the tuple and then a new tuple is returned
        private void CreateAndAddNewList(int i, int j, int res, string symbol, Tuple<List<int>, string, int, int> tuple,
            List<Tuple<List<int>, string, int, int>> resultTuple)
        {
            var listClone = new List<int>(tuple.Item1);
            var resultString = "";
            if (tuple.Item2 != "")
            {
                resultString += Environment.NewLine;
            }
            resultString += CreateOperationString(listClone[i], listClone[j], symbol, res);

            listClone.RemoveAt(i);
            if (i < j)
                listClone.RemoveAt(j - 1);
            else
                listClone.RemoveAt(j);
            listClone.Add(res);
            var points = tuple.Item3;
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
            var outputTuple = new Tuple<List<int>, string, int, int>(listClone, tuple.Item2 + resultString, points, 0);
            if ((res == _expectedRes) && (outputTuple.Item3 > _bestResult.Item3))
                _bestResult = outputTuple;

            //If the output tuple uses all operations set it as the best solution
            if ((res == _expectedRes) && isMathador(outputTuple))
            {
                var mathadorTuple =
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
                var operations = new List<string> {"x", "-", "/", "+"};
                if (operations.All(s => input.Item2.Contains(s)))
                {
                    _mathadorFound = true;
                    return true;
                }
            }
            return false;
        }

        private string CreateOperationString(int a, int b, string operand, int res)
        {
            if ((operand == "x") || (operand == "+"))
                if (a < b)
                {
                    var c = a;
                    a = b;
                    b = c;
                }
            return Convert.ToString(a) + " " + operand + " " + Convert.ToString(b) + " = " + Convert.ToString(res);
        }

        public string ListToString(List<int> list)
        {
            var s = "";
            foreach (var element in list)
                s += Convert.ToString(element) + " - ";
            return s;
        }

        private List<Tuple<List<int>, string, int, int>> RemoveDuplicates(
            List<Tuple<List<int>, string, int, int>> inputList)
        {
            //Duplicate the input list and create a new output list
            var cloneList = new List<Tuple<List<int>, string, int, int>>(inputList);
            var outputList = new List<Tuple<List<int>, string, int, int>>();
            var outputList2 = new List<Tuple<List<int>, string, int, int>>();
            for (var i = 0; i < cloneList.Count - 1; i++)
            {
                var currentTupleI = cloneList[i];

                for (var j = 0; j < cloneList.Count - 1; j++)
                {
                    var currentTupleJ = cloneList[j];
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