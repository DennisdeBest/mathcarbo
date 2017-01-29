using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using static System.Console;

namespace MathadorLib
{
    public class Game
    {
        private static string _lastPlayInfo;
        private static int[] _pointsArea;
        private static int[] _operationsArea;
        private static int[] _gameInputArea;
        private int _expectedResult;
        private int _currentGameLastLine;
        private int _currentGamePoints;
        private List<int> _currentLine;
        private int _currentLinePoints;
        private int _currentResult;
        private string _currentLineOperations;

        private readonly string _currentUser;
        private List<List<int>> _gameData;
        private readonly Generator _generator;

        //variables to keep track of game progression
        private int _lineCount;
        private readonly int _linesPerGame;

        //Database connection
        private readonly SQLiteConnection _mDbConnection;
        private readonly List<string> _operations;

        private readonly Solver _solver;

        //In the constructor we initiliaze the variables and create the database
        public Game()
        {
            _generator = new Generator();
            _solver = new Solver();

            _mDbConnection = new SQLiteConnection("Data Source=mathcarbo.sqlite; Version=3;");
            _mDbConnection.Open();
            //Create the table for the generator if it does not exist

            var sql = "CREATE TABLE IF NOT EXISTS highscores (username varchar(255), points int);";
            var command = new SQLiteCommand(sql, _mDbConnection);
            command.ExecuteNonQuery();

            //Set console options
            var hCenterConsole = 40;
            var vCenterConsole = 10;
            SetWindowSize(hCenterConsole*2, vCenterConsole*2);
            _pointsArea = new[] {hCenterConsole*2 - 20, 1};
            _operationsArea = new[] { hCenterConsole * 2 - 20, 2 };
            _gameInputArea = new[] {0, 2};

            //Initialise gameData
            _currentGamePoints = 0;
            _linesPerGame = 10;
            _currentGameLastLine = _linesPerGame;
            _operations = new List<string> {"+", "-", "*", "/"};
            _lineCount = 0;
            _currentLineOperations = "";

            //Write start message and options
            WriteLine(
                "Bienvenue a mathCarbo \n Veullez saisir les opérations entre \n les chiffres de la liste pour arriver au résultat \n " +
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
                        _currentUser = username;
                        //Start the main game loop
                        GameLoop();
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
        private void GameLoop()
        {
            var rejouer = 'o';
            do
            {
                InitGameTable();
                GenerateNewList();
                _lineCount = 0;

                while (_lineCount < _linesPerGame)
                {
                    WritePointsToScreen();
                    GetResult();
                    _currentLineOperations = "";
                    var operationsString = "";
                    var gameLineForDb = getLineForDB(_gameData[_lineCount]);
                    var validate = 'o';

                    //While the result isn't the correct one or the users hasn't cancelled
                    while ((_currentResult != -1) && (validate != 'n'))
                    {
                        Clear();
                        WritePointsToScreen();
                        WriteOperationsToScreen();
                        PrintLine(_currentLine);
                        var lastOperation = GetUserInput();
                        if (lastOperation != "")
                        {
                            operationsString += lastOperation;
                        }

                        //If the result is found ask to continue or not, if it's the last number move on to the next line
                        if (_currentResult == _expectedResult)
                        {
                            if (_currentLine.Count == 1)
                            {
                                validate = 'n';
                            }
                            else
                            {
                                Console.WriteLine("Bonne réponse, faire d'autres opérations ? (o/n)");
                                validate = Console.ReadKey(true).KeyChar;
                            }
                        }
                        //Reset line points and move on to the next line if the result was not found and only one number is left
                        else
                        {
                            if (_currentLine.Count == 1)
                            {
                                ResetLinePoints();
                                validate = 'n';
                            }
                        }
                       
                    }
                    //Check if mathador
                    if (_currentLineOperations.Length == 4)
                    {
                        _currentLinePoints += 5;
                        _currentGamePoints += 5;
                        operationsString += "Mathador !!" + Environment.NewLine;
                    }
                    SaveLineToGameTable(gameLineForDb, operationsString, _currentLinePoints);
                    _lineCount++;
                    _lastPlayInfo = "          ";
                    _currentLinePoints = 0;
                    _currentResult = 0;
                }
                Clear();
                SaveGameToDb();
                WriteLine("Vous avez obtenu " + _currentGamePoints +
                          " points\n<o> Rejouer \n<n> Quitter la partie\n<s> Voir la solution");
                rejouer = ReadKey(true).KeyChar;
                if (rejouer == 's')
                    rejouer = Solve();
                _currentGameLastLine += _linesPerGame;
                _currentGamePoints = 0;
                WriteLine("");
            } while (rejouer != 'n');
        }

        //Run the solver on all combinations played by the user and display the result on screen
        private char Solve()
        {
            var sql = "SELECT * FROM game;";
            var command = new SQLiteCommand(sql, _mDbConnection);
            var reader = command.ExecuteReader();
            Clear();
            WriteLine("***** Solutions *****");
            while (reader.Read())
            {
                var list = Convert.ToString(reader["list"]);
                var solverOutput = _solver.SolveFromString(list);
                WriteLine("Liste : " + splitListString(list) + Environment.NewLine);
                WriteLine("Votre solution : (" + reader["points"] + " points)");
                WriteLine(Convert.ToString(reader["operations"]));
                WriteLine("La meilleure solution : (" + solverOutput[1] + " points)");
                WriteLine(solverOutput[0] + Environment.NewLine + "--------------------" + Environment.NewLine);
            }
            WriteLine("\n<o> Rejouer \n<n> Quitter la partie");
            var rejouer = ReadKey(true).KeyChar;
            return rejouer;
        }

        //Save the highscores to the database
        private void SaveGameToDb()
        {
            var sql = "INSERT INTO highscores VALUES ('" + _currentUser + "', " + _currentGamePoints + ");";
            var command = new SQLiteCommand(sql, _mDbConnection);
            command.ExecuteNonQuery();
        }

        //Get the highscores from the database
        private void GetHighscores()
        {
            Console.Clear();
            Console.WriteLine("***** Highscores *****" + Environment.NewLine);
            var sql = "SELECT * FROM highscores ORDER BY points DESC LIMIT 9;";
            var command = new SQLiteCommand(sql, _mDbConnection);
            var reader = command.ExecuteReader();
            var counter = 1;
            while (reader.Read())
            {
                WriteLine(Convert.ToString(counter) + ". " + reader["username"] + " -- " + reader["points"]);
                counter++;
            }
        }

        //Reset all points for current line if expected result was not found or line was abandonned
        private void ResetLinePoints()
        {
            _currentResult = -1;
            _currentGamePoints -= _currentLinePoints;
            _currentLinePoints = 0;
        }

        //Wait for user input, read "q" to quit else read the line and split it to get the operation wanted by the user
        //Print the result of the operation or an error message to the screen
        private string GetUserInput()
        {
            SetCursorPosition(0, 0);
            WriteLine(_lastPlayInfo);
            var input = "";
            var operation = "";
            SetCursorPosition(_gameInputArea[0], _gameInputArea[1]);
            input = ReadLine();
            if (input == "q")
            {
                operation = "Cancelled" + Environment.NewLine;
                ResetLinePoints();
                return operation;
            }
            int a;
            int b;
            var splitedInput = input.Split(' ');

            if ((splitedInput.Length == 3) && int.TryParse(splitedInput[0], out a) && _currentLine.Contains(a) &&
                _operations.Contains(splitedInput[1]) && int.TryParse(splitedInput[2], out b) && _currentLine.Contains(b))
            {
                var c = OperationResult(a, b, splitedInput[1]);
                if (c != -1)
                {
                    _lastPlayInfo = a + " " + splitedInput[1] + " " + b + " = " + c + "\n";
                    operation = _lastPlayInfo;
                    _currentResult = c;
                    //Append operation to currentOperationsLine to check which operations have been used
                    //and if necessary apply bonus points
                    if (!_currentLineOperations.Contains(splitedInput[1]))
                    {
                    _currentLineOperations += splitedInput[1];
                    }
                    return operation;
                }
                _lastPlayInfo = "Not a valid line";
            }
            else
            {
                _lastPlayInfo = "Not a valid line";
            }
            return "";
        }

        //Get the result of an operation depending on the operation
        private int OperationResult(int a, int b, string currentOperation)
        {
            var currentOperationPoints = 0;
            var c = -1;

            switch (currentOperation)
            {
                case "+":
                    c = a + b;
                    currentOperationPoints = 1;
                    _currentLine.Remove(a);
                    _currentLine.Remove(b);
                    _currentLine.Add(c);
                    break;
                case "*":
                    c = a*b;
                    currentOperationPoints = 1;
                    _currentLine.Remove(a);
                    _currentLine.Remove(b);
                    _currentLine.Add(c);
                    break;
                case "-":
                    if (a > b)
                    {
                        c = a - b;
                        currentOperationPoints = 2;
                        _currentLine.Remove(a);
                        _currentLine.Remove(b);
                        _currentLine.Add(c);
                    }
                    else
                    {
                        c = -1;
                    }
                    break;

                case "/":
                    if (a%b == 0)
                    {
                        c = a/b;
                        currentOperationPoints = 3;
                        _currentLine.Remove(a);
                        _currentLine.Remove(b);
                        _currentLine.Add(c);
                    }
                    else
                    {
                        c = -1;
                    }
                    break;
            }
            _currentLinePoints += currentOperationPoints;
            _currentGamePoints += currentOperationPoints;
            return c;
        }

        //Write points to the corner of the screen and reset the cursor position
        private void WritePointsToScreen()
        {
            SetCursorPosition(_pointsArea[0], _pointsArea[1]);
            Write("Points : " + _currentGamePoints);
            SetCursorPosition(_gameInputArea[0], _gameInputArea[1]);
        }

        private void WriteOperationsToScreen()
        {
            SetCursorPosition(_operationsArea[0], _operationsArea[1]);
            Write("operations : " + _currentLineOperations);
            SetCursorPosition(_gameInputArea[0], _gameInputArea[1]);
        }

        //Get the result of the current active list
        private void GetResult()
        {
            var cloneList = new List<int>(_gameData[_lineCount]);
            _expectedResult = cloneList[cloneList.Count - 1];
            cloneList.RemoveAt(cloneList.Count - 1);
            _currentLine = cloneList;
        }

        //Print the current active list to the screen
        private void PrintLine(List<int> input)
        {
            var str = "";
            for (var i = 0; i < input.Count; i++)
                if (i == input.Count - 1)
                    str += input[i];
                else
                    str += input[i] + " - ";
            str += " = " + _expectedResult;
            SetCursorPosition(0, 1);
            WriteLine(str);
        }

        //Create a string from the current played line for the database
        private string getLineForDB(List<int> input)
        {
            var str = "";
            for (var i = 0; i < input.Count; i++)
                if (i == input.Count - 1)
                    str += input[i];
                else
                    str += input[i] + "-";

            return str;
        }

        //From a database string create a human readable string
        private string splitListString(string input)
        {
            var str = "";
            var explodedString = input.Split('-');
            for (var i = 0; i < explodedString.Length; i++)
                if (i == explodedString.Length - 1)
                    str += explodedString[i];
                else if (i == explodedString.Length - 2)
                    str += explodedString[i] + " = ";
                else
                    str += explodedString[i] + " - ";
            return str;
        }

        //Save User gameplay
        private void InitGameTable()
        {
            var sql = "DROP TABLE IF EXISTS game;";
            var command = new SQLiteCommand(sql, _mDbConnection);
            command.ExecuteNonQuery();

            sql = "CREATE TABLE game (list varchar(255), operations text, points int);";
            command = new SQLiteCommand(sql, _mDbConnection);
            command.ExecuteNonQuery();
        }

        private void SaveLineToGameTable(string list, string operations, int points)
        {
            var sql = "INSERT INTO game VALUES ('" + list + "', '" + operations + "'," + points + ");";
            var command = new SQLiteCommand(sql, _mDbConnection);
            command.ExecuteNonQuery();
        }

        //Generate a new list of 10 lines saved to file
        private void GenerateNewList()
        {
            var path = "generator.txt";
            _generator.GenerateRandomLists(10, path);
            _gameData = _generator.ReadFromFile(path);
        }
    }
}