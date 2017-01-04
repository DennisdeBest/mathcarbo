using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathadorLib
{
    public class Game
    {
        private List<List<int>> gameData;
        private List<int> currentLine;
        private List<string> operations;
        private int lineCount;
        private int _expectedResult;
        private int currentResult;
        public Game(List<List<int>> gameData)
        {
            operations = new List<string> {"+", "-", "*", "/"};
            lineCount = 0;
            this.gameData = gameData;
            Console.WriteLine("Bienvenue a mathCarbo");
            while (lineCount < 5)
            {
                getResult();
                while (currentResult != _expectedResult && currentResult != -1)
                {
                    printLine(currentLine);
                    getUserInput();
                }
                lineCount++;
                currentResult = 0;

            }
        }

        private void getUserInput()
        {
            
            var input = Console.ReadLine();
            if (input == "q")
            {
                currentResult = -1;
                return;
            }
            int a;
            int b;
            string[] splitedInput = input.Split(' ');

            if (int.TryParse(splitedInput[0], out a) && currentLine.Contains(a) && operations.Contains(splitedInput[1]) && int.TryParse(splitedInput[2], out b) && currentLine.Contains(b))
            {
                int c = operationResult(a, b, splitedInput[1]);
                    if (c != -1)
                    {
                        Console.WriteLine(a + splitedInput[1] + b + "=" + c);
                        currentResult = c;
                    }
                    else
                    {
                        Console.WriteLine("Not a valid operation");
                    }
            }
            else
            {
                Console.WriteLine("Not a valid line");
            }
        }

        private int operationResult(int a, int b, string currentOperation)
        {
            int c = -1;
            switch (currentOperation)
            {
                case "+":
                    c = a + b;
                    currentLine.Remove(a);
                    currentLine.Remove(b);
                    currentLine.Add(c);
                    break;
                case "*":
                    c = a * b;
                    currentLine.Remove(a);
                    currentLine.Remove(b);
                    currentLine.Add(c);
                    break;
                case "-":
                    if (a > b)
                    {
                        c = a - b;
                        currentLine.Remove(a);
                        currentLine.Remove(b);
                        currentLine.Add(c);
                    }
                    else
                    {
                        c = -1;
                    }
                    break;

                case "/":
                    if (a % b == 0)
                    {
                        c = a / b;
                        currentLine.Remove(a);
                        currentLine.Remove(b);
                        currentLine.Add(c);
                    }
                    else
                    {
                        c = -1;
                    }
                    break;
            }
            return c;
        }
        private void getResult()
        {
            List<int> cloneList = new List<int>(gameData[lineCount]);
            _expectedResult = cloneList[cloneList.Count - 1];
            cloneList.RemoveAt(cloneList.Count - 1);
            currentLine =  cloneList;
        }

        private void printLine(List<int> input )
        {
            string str = "";
            for (int i = 0; i < input.Count; i++)
            {
                if (i == input.Count - 1)
                {
                    str += input[i];
                }
                else
                {
                    str += input[i] + " - ";
                }
            }
            str += " = " + _expectedResult;
            Console.WriteLine(str);
        }
    }
}
