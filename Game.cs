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
        GameSound AppleEatenSound;
        public GameSound SnakeMovesSound;
        GameSound SnakeDeathSound;
        public GameSound BackgroundMusic;
        double SnakeSquareSize = 20;
        bool snakeIsDead = false;
        TextBlock TextBlockApples;
        TextBlock TextBlockScore;
        System.Windows.Controls.Canvas canvas;
        SnakeLocation AppleLocation = null;
        SnakeLocation[] slippy = null;
        bool isPaused = false;
        Key? pendingKeycode = null;
        string gridColor = "grey";
        string deadColor = "black";
        public Game()
        {
            Init();
        }

        private void Init()
        {
            List<SnakeLocation> list = new List<SnakeLocation>() {
                new SnakeLocation() { RowIndex = 7, ColumnIndex = 4, Direction = "right" },
                new SnakeLocation() { RowIndex = 7, ColumnIndex = 3, Direction = "right" },
                new SnakeLocation() { RowIndex = 7, ColumnIndex = 2, Direction = "right" }
            };
            HasPlayerStartedGame = false;
            IsGameOver = false;
            ApplesEaten = 0;
            SnakeSquareSize = 20;
            snakeIsDead = false;
            slippy = list.ToArray();

            AppleLocation = new SnakeLocation()
            {
                RowIndex = 7,
                ColumnIndex = 11,
                Direction = null,
            };

        }
        /// <summary>
        /// Gets the row and column of the cell
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        string GetCellId(SnakeLocation location)
        {
            return "R" + location.RowIndex + "C" + location.ColumnIndex;
        }
        /// <summary>
        /// Gets a random location, primarily used by the apple
        /// </summary>
        /// <returns></returns>
        SnakeLocation GetRandomLocation()
        {
            Random rnd = new Random();
            int rowIndex = rnd.Next(1, GridRows) - 1;
            int columnIndex = rnd.Next(1, GridColumns) - 1;
            return new SnakeLocation() { RowIndex = rowIndex, ColumnIndex = columnIndex, Direction = null };
        }
        /// <summary>
        /// Places apples on the grid
        /// </summary>
        private void PlaceApple()
        {
            AppleLocation = GetRandomLocation();
            bool collidesWithSnake = false;
            for (int i = 0; i < this.slippy.Length; i++)
            {
                var snakeSegmentLocation = this.slippy[i];
                if (AreSameLocation(AppleLocation, snakeSegmentLocation))
                {
                    collidesWithSnake = true;
                    break; //no sense checking as we are hitting the snake.
                }
            }
            if (collidesWithSnake)
            {
                PlaceApple(); //try again.
            }
        }
        /// <summary>
        /// Allows user to restart the game
        /// </summary>
        internal void Restart()
        {
            Init();
            UpdateScores();
            ClearBoard();
            DrawApple();
            DrawSnake();
        }
        /// <summary>
        /// Checks if the two locations are the same, primarily to see if snake ate itself or snake ate an apple
        /// </summary>
        /// <param name="locationA"></param>
        /// <param name="locationB"></param>
        /// <returns>Returns true if they are the same location</returns>
        bool AreSameLocation(SnakeLocation locationA, SnakeLocation locationB)
        {
            return locationA.RowIndex == locationB.RowIndex && locationA.ColumnIndex == locationB.ColumnIndex;
        }
        /// <summary>
        /// Checks if snake ate itself
        /// </summary>
        /// <returns>Returns true if snake ate itself</returns>
        bool DidSnakeEatItself()
        {
            if (this.slippy.Length > 1)
            {
                var snakeHeadLocation = this.slippy[0];
                for (int i = 1; i < this.slippy.Length; i++)
                {
                    var snakeBodyLocation = this.slippy[i];
                    if (AreSameLocation(snakeHeadLocation, snakeBodyLocation))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if snake ran outside the game board
        /// </summary>
        /// <param name="location"></param>
        /// <returns>Returns true if snake ran outside the game board</returns>
        bool IsOutsideGrid(SnakeLocation location)
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
        /// <summary>
        /// Receives a key code and determines if the snake direction should change or the game should be paused
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="makeMove"></param>
        public void HandleKeyEnum(Key keyCode, bool makeMove)
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

            if (IsOutsideGrid(tempLocation))
            {
                KillTheSnake();
            }
            else
            {
                //move the snake by placing the new head location and removing the last item.
                if (!isPaused)
                {
                    this.slippy = this.slippy.Prepend(tempLocation).ToArray();
                    if (AreSameLocation(AppleLocation, this.slippy[0]))
                    {
                        ApplesEaten++;
                        AppleEatenSound.Play();
                        UpdateScores();
                        PlaceApple();
                    }
                    else
                    {
                        //remove the last segment of the snake
                        this.slippy = this.slippy.Take(this.slippy.Length - 1).ToArray();
                        if (DidSnakeEatItself())
                        {
                            KillTheSnake();
                        }
                    }
                }
            }

            ClearBoard();
            DrawSnake();
            DrawApple();
        }
        /// <summary>
        /// Kills the snake
        /// </summary>
        private void KillTheSnake()
        {
            if (!IsGameOver)
            {
                BackgroundMusic.Stop();
                SnakeDeathSound.Play();
                IsGameOver = true;
                gridColor = deadColor;
                snakeIsDead = true;
                ClearBoard();
                DrawSnake();
                DrawApple();
            }
        }
        /// <summary>
        /// Checks if the direction key proposed matches the current direction and rejects the move
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns>Returns true if key proposed matches teh current direction</returns>
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
        /// <summary>
        /// Clears the game board
        /// </summary>
        private void ClearBoard()
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

                    string cellId = GetCellId(foo);
                    var square = GetRectangleByName(cellId);
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
        /// <summary>
        /// Creates the game board
        /// </summary>
        private void CreateGrid()
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
        /// <summary>
        /// Creates the apples
        /// </summary>
        private void DrawApple()
        {
            string appleCellId = GetCellId(AppleLocation);
            var appleSquare = GetRectangleByName(appleCellId);
            if (appleSquare != null)
            {
                (appleSquare as Rectangle).Fill = Brushes.Red;
            }
        }
        /// <summary>
        /// Gets a rectangle by its name (i.e. col, row)
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the rectangle if found</returns>
        private Rectangle GetRectangleByName(string name)
        {
            return canvas.Children.OfType<Rectangle>().Where(x => x.Name == name).FirstOrDefault();
        }
        /// <summary>
        /// Creates the snake
        /// </summary>
        private void DrawSnake()
        {
            //draw the tail first so if it eats itself the head will be on top

            for (int i = this.slippy.Length - 1; i > -1; i--)
            {
                SnakeLocation segment = this.slippy[i];
                string snakeCellId = GetCellId(segment);
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
                var snakeSegment = GetRectangleByName(snakeCellId);
                if (snakeSegment != null)
                {
                    if (snakeIsDead)
                    {
                        snakeSegment.Fill = Brushes.White;
                    }
                    else
                    {
                        snakeSegment.Fill = i == 0 ? Brushes.LawnGreen : Brushes.Green;
                    }

                }
            }
        }
        /// <summary>
        /// Moves the snake
        /// </summary>
        public void MoveSnake()
        {
            if (HasPlayerStartedGame && !IsGameOver && !isPaused)
            {
                if (pendingKeycode != null)
                {
                    System.Diagnostics.Debug.WriteLine("pendingKeycode " + pendingKeycode);
                    HandleKeyEnum(pendingKeycode.Value, true);
                    pendingKeycode = null;
                    return;
                }
                string snakeDirection = slippy[0].Direction;
                if (snakeDirection == "up")
                {
                    HandleKeyEnum(KEYS.KeyUp[0], true);
                }
                else if (snakeDirection == "down")
                {
                    HandleKeyEnum(KEYS.KeyDown[0], true);
                }
                else if (snakeDirection == "left")
                {
                    HandleKeyEnum(KEYS.KeyLeft[0], true);
                }
                else if (snakeDirection == "right")
                {
                    HandleKeyEnum(KEYS.KeyRight[0], true);
                }
            }
            var snakeHead = this.slippy[0];
            System.Diagnostics.Debug.WriteLine($"Snake Location {snakeHead.RowIndex} {snakeHead.ColumnIndex} {snakeHead.Direction}");
        }
        /// <summary>
        /// Updates the scores
        /// </summary>
        private void UpdateScores()
        {

            TextBlockApples.Text = "Apples Eaten: " + ApplesEaten;
            if (ApplesEaten > HighScore)
            {
                HighScore = ApplesEaten;
                GameSettings.Default.HighScore = HighScore;
                GameSettings.Default.Save();
            }
            TextBlockScore.Text = "High Score: " + HighScore;
        }
        /// <summary>
        /// Starts the game
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="TextBlockApples"></param>
        /// <param name="TextBlockScore"></param>
        public void Go(Canvas canvas, TextBlock TextBlockApples, TextBlock TextBlockScore)
        {
            this.canvas = canvas;
            this.TextBlockApples = TextBlockApples;
            this.TextBlockScore = TextBlockScore;

            AppleEatenSound = new GameSound("Sounds/GetApple.mp3");

            SnakeMovesSound = new GameSound("Sounds/SnakeMoves.mp3");

            SnakeDeathSound = new GameSound("Sounds/SnakeDeath.mp3");

            BackgroundMusic = new GameSound("Sounds/BackgroundMusic.mp3");
            HighScore = GameSettings.Default.HighScore;
            UpdateScores();
            CreateGrid();
            DrawApple();
            DrawSnake();



        }


    }
}
