using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace WPFSnakeGame
{
    class Game
    {
        public Keys KEYS = new Keys();
        const int GridRows = 15;
        const int GridColumns = 17;
        public bool HasPlayerStartedGame = false;
        public bool IsGameOver = false;
        int ApplesEaten = 0;
        int HighScore = 0;
        public GameSound AppleEatenSound;
        public GameSound SnakeMovesSound;
        public GameSound SnakeDeathSound;
        public GameSound BackgroundMusic;
        double SnakeSquareSize = 20;
        bool snakeIsDead = false;
        TextBlock TextBlockApples;
        TextBlock TextBlockScore;

        public System.Windows.Controls.Canvas canvas;
        SnakeLocation AppleLocation = new SnakeLocation()
        {
            RowIndex = 7,
            ColumnIndex = 11,
            Direction = null,
        };
        SnakeLocation[] slippy = null;






        bool isPaused = false;
        Key? pendingKeycode = null;

        string gridColor = "grey";
        string deadColor = "black";


        public Game()
        {
            List<SnakeLocation> list = new List<SnakeLocation>() {
                new SnakeLocation() { RowIndex = 7, ColumnIndex = 4, Direction = "right" },
                new SnakeLocation() { RowIndex = 7, ColumnIndex = 3, Direction = "right" },
                new SnakeLocation() { RowIndex = 7, ColumnIndex = 2, Direction = "right" }
            };
            slippy = list.ToArray();

        }


        string getCellId(SnakeLocation location)
        {
            return "R" + location.RowIndex + "C" + location.ColumnIndex;
        }

        SnakeLocation getRandomLocation()
        {
            Random rnd = new Random();
            int rowIndex = rnd.Next(1, GridRows) - 1;
            int columnIndex = rnd.Next(1, GridColumns) - 1;
            return new SnakeLocation() { RowIndex = rowIndex, ColumnIndex = columnIndex, Direction = null };
        }

        void placeApple()
        {
            AppleLocation = getRandomLocation();
            bool collidesWithSnake = false;
            for (int i = 0; i < this.slippy.Length; i++)
            {
                var snakeSegmentLocation = this.slippy[i];
                if (areSameLocation(AppleLocation, snakeSegmentLocation))
                {
                    collidesWithSnake = true;
                    break; //no sense checking as we are hitting the snake.
                }
            }
            if (collidesWithSnake)
            {
                placeApple(); //try again.
            }
        }

        bool areSameLocation(SnakeLocation locationA, SnakeLocation locationB)
        {
            return locationA.RowIndex == locationB.RowIndex && locationA.ColumnIndex == locationB.ColumnIndex;
        }

        bool didSnakeEatItself()
        {
            if (this.slippy.Length > 1)
            {
                var snakeHeadLocation = this.slippy[0];
                for (int i = 1; i < this.slippy.Length; i++)
                {
                    var snakeBodyLocation = this.slippy[i];
                    if (areSameLocation(snakeHeadLocation, snakeBodyLocation))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool isOutsideGrid(SnakeLocation location)
        {
            if (
              location.RowIndex < 0 ||
              location.ColumnIndex < 0 ||
              location.RowIndex >= GridRows ||
              location.ColumnIndex >= GridColumns
            )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void handleKeyEnum(Key keyCode, bool makeMove)
        {
            if (IsGameOver)
            {
                return;
            }
            SnakeLocation tempLocation = new SnakeLocation()
            {
                RowIndex = this.slippy[0].RowIndex,
                ColumnIndex = this.slippy[0].ColumnIndex,
                Direction = "unknown"
            };

            if (Array.IndexOf(KEYS.KeyUp, keyCode) != -1 && !isPaused)
            {
                tempLocation.RowIndex -= 1;
                tempLocation.Direction = "up";
            }
            else if (Array.IndexOf(KEYS.KeyDown, keyCode) != -1 && !isPaused)
            {
                tempLocation.RowIndex += 1;
                tempLocation.Direction = "down";
            }
            else if (Array.IndexOf(KEYS.KeyLeft, keyCode) != -1 && !isPaused)
            {
                tempLocation.ColumnIndex -= 1;
                tempLocation.Direction = "left";
            }
            else if (Array.IndexOf(KEYS.KeyRight, keyCode) != -1 && !isPaused)
            {
                tempLocation.ColumnIndex += 1;
                tempLocation.Direction = "right";
            }
            else if (Array.IndexOf(KEYS.Pause, keyCode) != -1)
            {
                System.Diagnostics.Debug.WriteLine("pause was pressed");
                if (isPaused)
                {
                    isPaused = false;
                    System.Diagnostics.Debug.WriteLine("game is unpaused");
                }
                else
                {
                    isPaused = true;
                    System.Diagnostics.Debug.WriteLine("game is paused");
                }

                return;
            }
            else
            {
                return;
            }

            if (!makeMove)
            {
                pendingKeycode = keyCode;
                return;
            }

            if (isOutsideGrid(tempLocation))
            {
                killTheSnake();
            }
            else
            {
                //move the snake by placing the new head location and removing the last item.
                if (!isPaused)
                {
                    this.slippy = this.slippy.Prepend(tempLocation).ToArray();
                    if (areSameLocation(AppleLocation, this.slippy[0]))
                    {
                        ApplesEaten++;
                        AppleEatenSound.Play();
                        updateScores();
                        placeApple();
                    }
                    else
                    {
                        //remove the last segment of the snake
                        this.slippy = this.slippy.Take(this.slippy.Length - 1).ToArray();
                        if (didSnakeEatItself())
                        {
                            killTheSnake();
                        }
                    }
                }
            }

            clearBoard();
            drawSnake();
            drawApple();
        }

        void killTheSnake()
        {
            if (!IsGameOver)
            {
                BackgroundMusic.Stop();
                SnakeDeathSound.Play();
                IsGameOver = true;
                gridColor = deadColor;
                snakeIsDead = true;
                clearBoard();
                drawSnake();
                drawApple();
            }
        }

        public bool IsValidMove(Key keyCode)
        {
            string snakeHeadDirection = this.slippy[0].Direction;
            //if keycode is in opposite direction of the head - don't allow move.
            if (
              (snakeHeadDirection == "left" && Array.IndexOf(KEYS.KeyRight, keyCode) != -1) ||
              (snakeHeadDirection == "right" && Array.IndexOf(KEYS.KeyLeft, keyCode) != -1) ||
              (snakeHeadDirection == "up" && Array.IndexOf(KEYS.KeyDown, keyCode) != -1) ||
              (snakeHeadDirection == "down" && Array.IndexOf(KEYS.KeyUp, keyCode) != -1)
            )
            {
                System.Diagnostics.Debug.WriteLine("illegal move " + keyCode);
                return false;
            }

            return Array.IndexOf(KEYS.AllKeys, keyCode) != -1;
        }

        void clearBoard()
        {
            var nextIsOdd = false;
            for (int rowIndex = 0; rowIndex < GridRows; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < GridColumns; columnIndex++)
                {

                    SnakeLocation foo = new SnakeLocation()
                    {
                        RowIndex = rowIndex,
                        ColumnIndex = columnIndex,
                        Direction = null
                    };

                    string cellId = getCellId(foo);
                    var square = getRectangleByName(cellId);
                    if (square != null)
                    {
                        if (snakeIsDead)
                        {
                            square.Fill = Brushes.Black;
                        }
                        else
                        {
                            square.Fill = nextIsOdd ? Brushes.Gray : Brushes.Black;
                        }

                    }
                    nextIsOdd = !nextIsOdd;
                }
            }
        }

        void createGrid()
        {
            double nextX = 0;
            double nextY = 0;
            bool nextIsOdd = false;
            for (int rowIndex = 0; rowIndex < GridRows; rowIndex++)
            {
                nextX = 0;
                nextY += SnakeSquareSize;
                for (int columnIndex = 0; columnIndex < GridColumns; columnIndex++)
                {
                    nextX += SnakeSquareSize;
                    Rectangle rect = new Rectangle
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = nextIsOdd ? Brushes.Gray : Brushes.Black
                    };
                    nextIsOdd = !nextIsOdd;
                    canvas.Children.Add(rect);
                    Canvas.SetTop(rect, nextY);
                    Canvas.SetLeft(rect, nextX);
                    rect.Name = "R" + rowIndex + "C" + columnIndex;
                }
            }
        }

        void drawApple()
        {
            string appleCellId = getCellId(AppleLocation);
            var appleSquare = getRectangleByName(appleCellId);
            if (appleSquare != null)
            {
                (appleSquare as Rectangle).Fill = Brushes.Red;
            }
        }

        Rectangle getRectangleByName(string name)
        {
            return canvas.Children.OfType<Rectangle>().Where(x => x.Name == name).FirstOrDefault();
        }
        void drawSnake()
        {
            //draw the tail first so if it eats itself the head will be on top

            for (int i = this.slippy.Length - 1; i > -1; i--)
            {
                SnakeLocation segment = this.slippy[i];
                string snakeCellId = getCellId(segment);
                string direction = segment.Direction;
                string className = "";
                if (i == 0)
                {
                    className = "snakehead";
                }
                else
                {
                    var prevSegment = this.slippy[i - 1];
                    var prevDirection = prevSegment.Direction;
                    className = i == this.slippy.Length - 1 ? "snaketail" : "snakebody";
                    if (prevDirection != direction)
                    {
                        if (direction == "right" && prevDirection == "down")
                        {
                            className = "snakebodycurve";
                            direction = "curve2";
                        }
                        else if (direction == "right" && prevDirection == "up")
                        {
                            className = "snakebodycurve";
                            direction = "curve4";
                        }
                        else if (direction == "down" && prevDirection == "left")
                        {
                            className = "snakebodycurve";
                            direction = "curve4";
                        }
                        else if (direction == "down" && prevDirection == "right")
                        {
                            className = "snakebodycurve";
                            direction = "curve3";
                        }
                        else if (direction == "up" && prevDirection == "right")
                        {
                            className = "snakebodycurve";
                            direction = "curve";
                        }
                        else if (direction == "up" && prevDirection == "left")
                        {
                            className = "snakebodycurve";
                            direction = "curve2";
                        }
                        else if (direction == "left" && prevDirection == "down")
                        {
                            className = "snakebodycurve";
                            direction = "curve";
                        }
                        else if (direction == "left" && prevDirection == "up")
                        {
                            className = "snakebodycurve";
                            direction = "curve3";
                        }
                    }
                }

                if (IsGameOver)
                {
                    className += " dead";
                }
                var snakeSegment = getRectangleByName(snakeCellId);
                if (snakeSegment != null)
                {
                    if (snakeIsDead)
                    {
                        snakeSegment.Fill = Brushes.White;
                    }
                    else
                    {
                        snakeSegment.Fill = Brushes.Green;
                    }

                }
            }
        }

        public void moveSnake()
        {
            if (HasPlayerStartedGame && !IsGameOver && !isPaused)
            {
                if (pendingKeycode != null)
                {
                    System.Diagnostics.Debug.WriteLine("pendingKeycode " + pendingKeycode);
                    handleKeyEnum(pendingKeycode.Value, true);
                    pendingKeycode = null;
                    return;
                }
                string snakeDirection = slippy[0].Direction;
                if (snakeDirection == "up")
                {
                    handleKeyEnum(KEYS.KeyUp[0], true);
                }
                else if (snakeDirection == "down")
                {
                    handleKeyEnum(KEYS.KeyDown[0], true);
                }
                else if (snakeDirection == "left")
                {
                    handleKeyEnum(KEYS.KeyLeft[0], true);
                }
                else if (snakeDirection == "right")
                {
                    handleKeyEnum(KEYS.KeyRight[0], true);
                }
            }
            var snakeHead = this.slippy[0];
            System.Diagnostics.Debug.WriteLine($"Snake Location {snakeHead.RowIndex} {snakeHead.ColumnIndex} {snakeHead.Direction}");
        }

        void updateScores()
        {
             
            TextBlockApples.Text = "Apples Eaten: " + ApplesEaten;
            if (ApplesEaten > HighScore)
            {
                HighScore = ApplesEaten;
                //TODO localStorage.setItem("HighScore", HighScore.toString());
            }
            TextBlockScore.Text = "High Score: " + HighScore;
        }

        public void go(Canvas canvas, TextBlock TextBlockApples, TextBlock TextBlockScore)
        {
            this.canvas = canvas;
            this.TextBlockApples = TextBlockApples;
            this.TextBlockScore = TextBlockScore;

            AppleEatenSound = new GameSound("Sounds/GetApple.mp3");

            SnakeMovesSound = new GameSound("Sounds/SnakeMoves.mp3");

            SnakeDeathSound = new GameSound("Sounds/SnakeDeath.mp3");

            BackgroundMusic = new GameSound("Sounds/BackgroundMusic.mp3");
            // TODO Fix Score
            // let strHighScore = localStorage.getItem("HighScore");
            //if (strHighScore != null)
            //{
            //    let intHighScore = parseInt(strHighScore, 10);
            //    HighScore = intHighScore;
            //    updateScores();
            //}
            createGrid();
            drawApple();
            drawSnake();



        }


    }
}
