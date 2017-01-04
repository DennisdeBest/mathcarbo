using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

        private string currentUser;

        private int lineCount;
        private int _expectedResult;
        private int currentResult;
        private int linesPerGame;
        private int currentGameLastLine;
        private int currentGamePoints;

        private SQLiteConnection m_dbConnection;

        public Game(List<List<int>> gameData)
        {
            m_dbConnection = new SQLiteConnection("Data Source=mathcarbo.sqlite;");
            m_dbConnection.Open();
            //Create the table for the generator if it does not exist
            string sql = "CREATE TABLE IF NOT EXISTS highscores (username varchar(255), points int);";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            currentGamePoints = 0;
            linesPerGame = 10;
            currentGameLastLine = linesPerGame;
            operations = new List<string> {"+", "-", "*", "/"};
            lineCount = 0;
            this.gameData = gameData;
            Console.WriteLine("Bienvenue a mathCarbo \n Veullez saisir les opérations entre \n les chiffres de la liste pour arriver au résultat \n " +
                              "Il faut entrer les opérations sous la forme suivante :\n <nombre><espace><operation><espace><nombre> ex : 8 * 9 \n" +
                              "pour abandonner la liste en cours entrez <q> \n");
            while (true)
            {
                Console.WriteLine("\n < j > jouer \n < h > highscores \n < q > quitter");
                var input = Console.ReadKey().KeyChar;
                switch (input)
                {
                    case 'j':
                        Console.WriteLine("");
                        Console.WriteLine("Entrez votre nom");
                        var username = Console.ReadLine();
                        currentUser = username;
                        gameLoop();
                        break;
                    case 'q':
                        Environment.Exit(1);
                        break;
                    case 'h':
                        GetHighscores();
                        break;
                }
            }
        }

        private void gameLoop()
        {
            char rejouer = 'o';
            do
            {
                while (lineCount < currentGameLastLine)
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
                SaveGameToDB();
                Console.WriteLine("Rejouer ? o/n");
                rejouer = Console.ReadKey().KeyChar;
                currentGameLastLine += linesPerGame;
                currentGamePoints = 0;
                Console.WriteLine("");

            } while (rejouer != 'n');

        }

        private void SaveGameToDB()
        {
            string sql = "INSERT INTO highscores VALUES ('" + currentUser + "', " + currentGamePoints + ");";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        private void GetHighscores()
        {
            string sql = "SELECT * FROM highscores ORDER BY points;";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                Console.WriteLine(Reader["username"] + " -- " + Reader["points"]);
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
                        Console.WriteLine(a + " " +splitedInput[1] + " " + b + " = " + c +"\n");
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
                    currentGamePoints += 1;
                    currentLine.Remove(a);
                    currentLine.Remove(b);
                    currentLine.Add(c);
                    break;
                case "*":
                    c = a * b;
                    currentGamePoints += 1;
                    currentLine.Remove(a);
                    currentLine.Remove(b);
                    currentLine.Add(c);
                    break;
                case "-":
                    if (a > b)
                    {
                        c = a - b;
                        currentGamePoints += 2;
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
                        currentGamePoints += 3;
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
