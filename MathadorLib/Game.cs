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
        private int lineCount;
        private int result;
        public Game(List<List<int>> gameData)
        {
            lineCount = 0;
            this.gameData = gameData;
            Console.WriteLine("Bienvenue a mathCarbo");
            while (lineCount < 5)
            {
                getResult();
                printLine(currentLine);
                getUserInput();

            }
        }

        private void getUserInput()
        {
            var input = Console.ReadKey().KeyChar;
            if (input == ' ')
            {
                Console.WriteLine("Pressed space");
            }
            else
            {
                Console.WriteLine(input);
            }
        }
        private void getResult()
        {
            List<int> cloneList = new List<int>(gameData[lineCount]);
            result = cloneList[cloneList.Count - 1];
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
            str += " = " + result;
            Console.WriteLine(str);
        }
    }
}
