using System;
using System.Collections.Generic;
using System.Data.SQLite;

using static System.Console;

namespace MathadorLib
{
    public class Game
    {
        private List<List<int>> gameData;
        private List<int> currentLine;
        private List<string> operations;

        private string currentUser;
        private static string lastPlayInfo;

        //Console areas
        private int hCenterConsole;
        private int vCenterConsole;
        private static int[] pointsArea;
        private static int[] gameInputArea;

        //variables to keep track of game progression
        private int lineCount;
        private int _expectedResult;
        private int currentResult;
        private int linesPerGame;
        private int currentGameLastLine;
        private int currentGamePoints;
        private int currentLinePoints;

        //Database connection
        private SQLiteConnection m_dbConnection;

        private Solver solver;
        private Generator generator;
        public Game()
        {
            SQLiteConnection.CreateFile("mathcarbo.sqlite");

            generator = new Generator();

            solver = new Solver();
            m_dbConnection = new SQLiteConnection("Data Source=mathcarbo.sqlite;");
            m_dbConnection.Open();
            //Create the table for the generator if it does not exist
            var sql = "CREATE TABLE IF NOT EXISTS highscores (username varchar(255), points int);";
            var command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            //Set console options
            hCenterConsole = 40;
            vCenterConsole = 10;
            SetWindowSize(hCenterConsole*2, vCenterConsole*2);
            pointsArea = new[] { hCenterConsole * 2 - 20, 1 };
            gameInputArea = new[] { 0, 2 };

            //Initialise gameData
            currentGamePoints = 0;
            linesPerGame = 10;
            currentGameLastLine = linesPerGame;
            operations = new List<string> {"+", "-", "*", "/"};
            lineCount = 0;

            //Write start message and options
            WriteLine("Bienvenue a mathCarbo \n Veullez saisir les opérations entre \n les chiffres de la liste pour arriver au résultat \n " +
                              "Il faut entrer les opérations sous la forme suivante :\n <nombre><espace><operation><espace><nombre> ex : 8 * 9 \n" +
                              "pour abandonner la liste en cours entrez <q> \n");

            //Start of the main menu loop
            while (true)
            {
                WriteLine("\n < j > jouer \n < h > highscores \n < q > quitter");
                var input = ReadKey(true).KeyChar;
                switch (input)
                {
                    case 'j':
                        WriteLine("");
                        WriteLine("Entrez votre nom");
                        var username = ReadLine();
                        currentUser = username;
                        //Start the main game loop
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
        //Main gameloop
    private void gameLoop()
        {
            
            var rejouer = 'o';
            do
            {
                initGameTable();
                generateNewList();
                lineCount = 0;

                while (lineCount < linesPerGame)
                {
                    //Reset console in between lists

                    WritePointsToScreen();
                    getResult();
                    var operationsString = "";
                    var gameLineForDB = getLineForDB(gameData[lineCount]);

                    while ((currentResult != _expectedResult) && (currentResult != -1))
                    {
                        Console.Clear();
                        WritePointsToScreen();
                        printLine(currentLine);
                        var lastOperation = getUserInput();
                        if (lastOperation != "")
                        {
                            operationsString += lastOperation;
                        }
                    }
                    saveLineToGameTable(gameLineForDB, operationsString, currentLinePoints);
                    lineCount++;
                    lastPlayInfo = "          ";
                    currentLinePoints = 0;
                    currentResult = 0;
                }
                Console.Clear();
                SaveGameToDB();
                WriteLine("Vous avez obtenu "+currentGamePoints+ " points\n<o> Rejouer \n<n> Quitter la partie\n<s> Voir la solution");
                rejouer = ReadKey(true).KeyChar;
                if (rejouer == 's')
                {
                    rejouer = solve();
                }
                currentGameLastLine += linesPerGame;
                currentGamePoints = 0;
                WriteLine("");

            } while (rejouer != 'n');

        }
        //Run the solver on all combinations played by the user and display the result on screen
        private char solve()
        {
            var sql = "SELECT * FROM game;";
            var command = new SQLiteCommand(sql, m_dbConnection);
            var Reader = command.ExecuteReader();
            Console.Clear();
            Console.WriteLine("***** Solutions *****");
            while (Reader.Read())
            {
                string list = Convert.ToString(Reader["list"]);
                List<string> solverOutput = solver.solveFromString(list);
                Console.WriteLine("Liste : " + splitListString(list) + Environment.NewLine);
                Console.WriteLine("Votre solution : ("+Reader["points"]+" points)");
                Console.WriteLine(Convert.ToString(Reader["operations"]));
                Console.WriteLine("La meilleure solution : ("+ solverOutput[1]+" points)");
                Console.WriteLine(solverOutput[0] + Environment.NewLine + "--------------------" + Environment.NewLine);
            }
            WriteLine("\n<o> Rejouer \n<n> Quitter la partie");
            char rejouer = ReadKey(true).KeyChar;
            return rejouer;
        }

        //Save the highscores to the database
        private void SaveGameToDB()
        {
            var sql = "INSERT INTO highscores VALUES ('" + currentUser + "', " + currentGamePoints + ");";
            var command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        //Get the highscores from the database
        private void GetHighscores()
        {
            var sql = "SELECT * FROM highscores ORDER BY points;";
            var command = new SQLiteCommand(sql, m_dbConnection);
            var Reader = command.ExecuteReader();
            while (Reader.Read())
                WriteLine(Reader["username"] + " -- " + Reader["points"]);
        }
        //Wait for user input, read "q" to quit else read the line and split it to get the operation wanted by the user
        //Print the result of the operation or an error message to the screen
        private string getUserInput()
        {
            Console.SetCursorPosition(0,0);
            Console.WriteLine(lastPlayInfo);
            var input = "";
            var operation = "";
            Console.SetCursorPosition(gameInputArea[0], gameInputArea[1]);
            input = Console.ReadLine();
            if (input == "q")
            {
                operation = "Canceled" + Environment.NewLine;
                currentResult = -1;
                currentGamePoints -= currentLinePoints;
                currentLinePoints = 0;
                return operation;
            }
            int a;
            int b;
            var splitedInput = input.Split(' ');

            if (splitedInput.Length == 3 && int.TryParse(splitedInput[0], out a) && currentLine.Contains(a) && operations.Contains(splitedInput[1]) && int.TryParse(splitedInput[2], out b) && currentLine.Contains(b))
            {
                var c = operationResult(a, b, splitedInput[1]);
                    if (c != -1)
                    {
                        lastPlayInfo = (a + " " +splitedInput[1] + " " + b + " = " + c +"\n");
                        operation = lastPlayInfo;
                        currentResult = c;
                        return operation;
                    }
                    else
                    {
                        lastPlayInfo = "Not a valid line";
                    }
            }
            else
            {
                lastPlayInfo = "Not a valid line";
            }
            return "";
        }

        //Get the result of an operation depending on the operation
        private int operationResult(int a, int b, string currentOperation)
        {
            int currentOperationPoints = 0;
            var c = -1;

            switch (currentOperation)
            {
                case "+":
                    c = a + b;
                    currentOperationPoints = 1;
                    currentLine.Remove(a);
                    currentLine.Remove(b);
                    currentLine.Add(c);
                    break;
                case "*":
                    c = a * b;
                    currentOperationPoints = 1;
                    currentLine.Remove(a);
                    currentLine.Remove(b);
                    currentLine.Add(c);
                    break;
                case "-":
                    if (a > b)
                    {
                        c = a - b;
                        currentOperationPoints = 2;
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
                        currentOperationPoints = 3;
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
            currentLinePoints += currentOperationPoints;
            currentGamePoints += currentOperationPoints;
            return c;
        }

        //Write points to the corner of the screen and reset the cursor position
        private void WritePointsToScreen()
        {
            int[] oldCursorPosition = new[] { Console.CursorLeft, Console.CursorTop };
            Console.SetCursorPosition(pointsArea[0], pointsArea[1]);
            Console.Write("Points : " + currentGamePoints);
            Console.SetCursorPosition(gameInputArea[0], gameInputArea[1]);
        }

        //Get the result of the current active list
        private void getResult()
        {
            var cloneList = new List<int>(gameData[lineCount]);
            _expectedResult = cloneList[cloneList.Count - 1];
            cloneList.RemoveAt(cloneList.Count - 1);
            currentLine =  cloneList;
        }

        //Print the current active list to the screen
        private void printLine(List<int> input )
        {
            var str = "";
            for (var i = 0; i < input.Count; i++)
                if (i == input.Count - 1)
                    str += input[i];
                else
                    str += input[i] + " - ";
            str += " = " + _expectedResult;
            Console.SetCursorPosition(0,1);
            WriteLine(str);
        }

        private string getLineForDB(List<int> input)
        {
            var str = "";
            for (var i = 0; i < input.Count; i++)
                if (i == input.Count - 1)
                    str += input[i];
                else
                    str += input[i] + "-"; ;

            return str;
        }

        private string splitListString(string input)
        {
            string str = "";
            string[] explodedString = input.Split('-');
            for (var i = 0; i < explodedString.Length; i++)
            {
                if (i == explodedString.Length - 1)
                    str += explodedString[i];
                else if (i == explodedString.Length - 2)
                    str += explodedString[i] + " = ";
                else
                    str += explodedString[i] + " - ";
            }
            return str;

        }
        //Save User gameplay
        private void initGameTable()
        {
            var sql = "DROP TABLE IF EXISTS game;";
            var command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE game (list varchar(255), operations text, points int);";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        private void saveLineToGameTable(string List, string operations, int points)
        {
            var sql = "INSERT INTO game VALUES ('" + List + "', '" + operations + "',"+ points +");";
            var command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        private void generateNewList()
        {
            string path = "generator.txt";
            generator.GenerateRandomLists(10, path);
            //return generator.ReadFromDB();
            gameData =  generator.ReadFromFile(path);
        }
    }
}
