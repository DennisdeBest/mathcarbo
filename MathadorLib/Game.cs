using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
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
        private static int[] timeArea;
        private static int[] pointsArea;
        private static int[] gameInputArea;

        //Timer and max time per line
        private Timer timer;
        private static int timeLeft = 30;

        //variables to keep track of game progression
        private int lineCount;
        private int _expectedResult;
        private int currentResult;
        private int linesPerGame;
        private int currentGameLastLine;
        private int currentGamePoints;
        private int currentLinePoints;

        private static bool timeUp = false;
        //Database connection
        private SQLiteConnection m_dbConnection;

        public Game(List<List<int>> gameData)
        {
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
            timeArea = new[] {hCenterConsole*2 - 20, 0};
            pointsArea = new[] { hCenterConsole * 2 - 20, 1 };
            gameInputArea = new[] { 0, 2 };

            //Initialise gameData
            currentGamePoints = 0;
            linesPerGame = 10;
            currentGameLastLine = linesPerGame;
            operations = new List<string> {"+", "-", "*", "/"};
            lineCount = 0;
            this.gameData = gameData;

            //Write start message and options
            WriteLine("Bienvenue a mathCarbo \n Veullez saisir les opérations entre \n les chiffres de la liste pour arriver au résultat \n " +
                              "Il faut entrer les opérations sous la forme suivante :\n <nombre><espace><operation><espace><nombre> ex : 8 * 9 \n" +
                              "pour abandonner la liste en cours entrez <q> \n");

            //Start of the main menu loop
            while (true)
            {
                WriteLine("\n < j > jouer \n < h > highscores \n < q > quitter");
                var input = ReadKey().KeyChar;
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

        //Print the time to the corner of the screen every second and reset the cursor position
    private static void TimerCallback(object o)
    {
        string timeString = "";
        timeLeft--;
        timeUp = false;
      if (timeLeft == 0)
            {
                //If the time is up simulate input to skip this line
                InputSimulator inputSimulator = new InputSimulator();
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN, VirtualKeyCode.VK_Q, VirtualKeyCode.RETURN);
                timeUp = true;
                timeLeft = 30;
            }
        if (timeLeft < 10)
        {
            timeString = "0" + Convert.ToString(timeLeft);
        }
        else
        {
            timeString = Convert.ToString(timeLeft);
        }
            int[] oldCursorPosition = new[] { Console.CursorLeft, Console.CursorTop };
            SetCursorPosition(timeArea[0], timeArea[1]);
            // Display the date/time when this method got called.
            WriteLine("Time left : " + timeString);
            SetCursorPosition(oldCursorPosition[0], oldCursorPosition[1]);
        // Force a garbage collection to occur for this demo.
        GC.Collect();
    }

        //Main gameloop
    private void gameLoop()
        {
            
            var rejouer = 'o';
            do
            {
                while (lineCount < currentGameLastLine)
                {
                    //Set timer
                    //timer = new Timer(TimerCallback, null, 0, 1000);
                    //Reset time and console in between lists
                    Console.Clear();
                    timeLeft = 30;
                    WritePointsToScreen();
                    getResult();

                    while ((currentResult != _expectedResult) && (currentResult != -1))
                    {
                        WritePointsToScreen();
                        printLine(currentLine);
                        getUserInput();
                    }
                    lineCount++;
                    lastPlayInfo = "          ";
                    currentLinePoints = 0;
                    currentResult = 0;
                }
                timer.Dispose();
                Console.Clear();
                SaveGameToDB();
                WriteLine("Vous avez obtenu "+currentGamePoints+ " points\nRejouer ? o/n");
                rejouer = ReadKey().KeyChar;
                currentGameLastLine += linesPerGame;
                currentGamePoints = 0;
                WriteLine("");

            } while (rejouer != 'n');

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
        private void getUserInput()
        {
            Console.SetCursorPosition(0,0);
            Console.WriteLine(lastPlayInfo);
            var input = "";
            Console.SetCursorPosition(gameInputArea[0], gameInputArea[1]);
            try
            {
                input = Reader.ReadLine(5000);
            }
            catch (TimeoutException)
            {
                lastPlayInfo = "Time up";
                currentResult = -1;
                return;
            }
            if (input == "q")
            {
                currentResult = -1;
                currentGamePoints -= currentLinePoints;
                return;
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
                        currentResult = c;
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
    }
}
