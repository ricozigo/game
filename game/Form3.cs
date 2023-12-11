using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Gif.Components;
using GifImage;


namespace game
{

    public partial class Form3 : Form
    {
        private const int BoardWidth = 10;
        private ProgressBar ProgressBar;
        private const int BoardHeight = 18;
        private const int BlockSize = 25;
        private readonly Pen gridPen = new Pen(Color.Black);
        private Brush blockBrush;
        private Timer timer;
        private bool inGame = false;
        private Tetromino currentTetromino;
        Random random = new Random();
        private bool gameWon = false;       
       


        private BoardPixel[,] gameField = new BoardPixel[BoardWidth, BoardHeight];
        public Form3()
        {
            InitializeComponent();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            this.KeyDown += Form3_KeyDown;
            pictureBox2.Paint += pictureBox2_Paint;
            InitializeProgressBar();
            currentTetromino = Tetromino.Create((byte)random.Next(1, 8)); // Используйте случайную фигуру
            inGame = true;
            timer.Start();
            //InitializeGifImage();

        }

       




        private void InitializeProgressBar()
        {
            //ProgressBar = new ProgressBar();
            ProgressBar.Maximum = 1000;
            ProgressBar.Value = 1000;
            //ProgressBar.Location = new Point(10, 10); // Adjust the location as needed
            //ProgressBar.Size = new Size(200, 20); // Adjust the size as needed
        }



         private void Timer_Tick(object sender, EventArgs e)
        {
            int newY = currentTetromino.Y;
            newY++;

            if (CheckMove(currentTetromino.X, newY, currentTetromino.Shape))
            {
                currentTetromino.Y = newY;
            }
            else
            {
                AddTetrominoToBoard(currentTetromino);
                CheckAndClearRows();

                // Subtract 100 XP when a row is cleared
              //  ProgressBar.Value -= 100;

                currentTetromino = Tetromino.Create((byte)random.Next(1, 8));

                if (!CheckMove(currentTetromino.X, currentTetromino.Y, currentTetromino.Shape))
                {
                    inGame = false;
                    timer.Stop();
                }
            }
            pictureBox2.Invalidate();
        }



        

        private void Form3_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    MoveTetromino(Direction.RIGHT);
                    break;
                case Keys.Left:
                    MoveTetromino(Direction.LEFT);
                    break;
                case Keys.Down:
                    HandleDownKeyPress();
                    break;
                case Keys.Up:
                    RotateTetromino();
                    break;
            }
        }

        private void HandleDownKeyPress()
        {
            int newY = currentTetromino.Y;
            newY++;
            if (CheckMove(currentTetromino.X, newY, currentTetromino.Shape)) // исправлено
            {
                currentTetromino.Y = newY;
            }
            else // If we can't move down anymore, treat it as if the block has landed
            {
                AddTetrominoToBoard(currentTetromino);
                CheckAndClearRows();
                currentTetromino = Tetromino.Create((byte)random.Next(1, 8));

                if (!CheckMove(currentTetromino.X, currentTetromino.Y, currentTetromino.Shape)) // Проверяем, может ли новое тетромино появиться на поле
                {
                    inGame = false;
                    timer.Stop();
                }
            }
            pictureBox2.Invalidate();
        }


        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics);
            DrawTetromino(e.Graphics, currentTetromino);
            DrawBoard(e.Graphics);

        }

        private void DrawBoard(Graphics g)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    var pixel = gameField[x, y];
                    if (pixel != null && pixel.hasBlock)
                    {
                        DrawTetromino(g, pixel, x, y);
                    }
                }
            }
        }


        private void DrawTetromino(Graphics g, BoardPixel pixel, int x, int y)
        {
            blockBrush = new SolidBrush(pixel.color);
            g.FillRectangle(blockBrush, x * BlockSize, y * BlockSize, BlockSize, BlockSize);

            // Рисуем красивую обводку для блока
            g.DrawRectangle(new Pen(Color.Black), x * BlockSize, y * BlockSize, BlockSize, BlockSize);
        }

        private void DrawGrid(Graphics g)
        {
            for (int x = 0; x <= BoardWidth; x++)
            {
                g.DrawLine(gridPen, x * BlockSize, 0, x * BlockSize, BoardHeight * BlockSize);
            }
            for (int y = 0; y <= BoardHeight; y++)
            {
                g.DrawLine(gridPen, 0, y * BlockSize, BoardWidth * BlockSize, y * BlockSize);
            }
        }

        private void DrawTetromino(Graphics g, Tetromino tetromino)
        {
            if (g == null)
            {
                throw new ArgumentNullException(nameof(g));
            }

            if (tetromino == null)
            {
                // Tetromino has not been assigned yet, so do not draw anything
                return;
            }

            blockBrush = new SolidBrush(tetromino.Color);
            foreach (var p in tetromino.Shape)
            {
                int x = (tetromino.X + p.X) * BlockSize;
                int y = (tetromino.Y + p.Y) * BlockSize;
                g.FillRectangle(blockBrush, x, y, BlockSize, BlockSize);

                // Рисуем только границы для блока
                g.DrawRectangle(new Pen(Color.Black), x, y, BlockSize, BlockSize);
            }
        }

        enum Direction
        {
            LEFT,
            RIGHT,
            DOWN
        }

        private void MoveTetromino(Direction direction)
        {
            if (!inGame)
                return;

            int newX = currentTetromino.X;
            int newY = currentTetromino.Y;

            switch (direction)
            {
                case Direction.LEFT:
                    newX--;
                    break;
                case Direction.RIGHT:
                    newX++;
                    break;
                case Direction.DOWN:
                    newY++;
                    break;
            }

            if (CheckMove(newX, newY, currentTetromino.Shape)) // исправлено
            {
                currentTetromino.X = newX;
                currentTetromino.Y = newY;
                currentTetromino.IsMoving = true; // Set IsMoving to true when moving
            }
            else if (direction == Direction.DOWN)
            {
                AddTetrominoToBoard(currentTetromino); // Добавляем текущее тетромино в игровое поле
                currentTetromino = Tetromino.Create((byte)random.Next(1, 8)); // Новая фигура
                if (!CheckMove(currentTetromino.X, currentTetromino.Y, currentTetromino.Shape)) // Проверяем, может ли новое тетромино появиться на поле
                {
                    inGame = false;
                    timer.Stop();
                    //TODO: Добавьте выполнение необходимых действий при окончании игры, например, показ сообщения о конце игры
                }
                currentTetromino.IsMoving = false; // Set IsMoving to false when the tetromino lands
            }
            pictureBox2.Invalidate();
        }

        private void AddTetrominoToBoard(Tetromino tetromino)
        {
            foreach (var p in tetromino.Shape)
            {
                int x = tetromino.X + p.X;
                int y = tetromino.Y + p.Y;
                if (x >= 0 && x < BoardWidth && y < BoardHeight)
                {
                    if (y < 0)
                        continue;

                    gameField[x, y] = new BoardPixel() { color = tetromino.Color, hasBlock = true };
                }
                else
                {
                    inGame = false;
                    timer.Stop();
                    return;
                }
            }
            if (tetromino.Y + tetromino.Shape.Min(p => p.Y) == 0) // Если тетромино достигает верхней границы доски
            {
                inGame = false;
                timer.Stop();
                MessageBox.Show("Game Over");  // Показывает сообщение об окончании игры
            }
        }


        private bool CheckMove(int x, int y, List<Point> shape)
        {
            foreach (Point point in shape)
            {
                int newX = x + point.X;
                int newY = y + point.Y;

                if (newX < 0 || newX >= BoardWidth || newY >= BoardHeight || newY < 0)
                {
                    return false; // Tetromino is out of bounds
                }

                if (newY >= 0 && newY < BoardHeight && gameField[newX, newY] != null) // Сделал проверку, что блок находится в пределах допустимых значений по Y
                {
                    return false; // Tetromino overlaps with other blocks
                }
            }

            return true; // Move is valid
        }

        private void RotateTetromino()
        {
            if (!inGame)
                return;

            Tetromino rotatedTetromino = new Tetromino(currentTetromino.Shape, currentTetromino.Color);
            rotatedTetromino.Rotate();

            if (CheckMove(currentTetromino.X, currentTetromino.Y, rotatedTetromino.Shape))
            {
                currentTetromino.Shape = rotatedTetromino.Shape;
            }
            pictureBox2.Invalidate();
        }

        class Tetromino
        {
            public List<Point> Shape { get; set; }
            public bool IsMoving { get; set; } = true;
            public Color Color { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public Tetromino(List<Point> shape, Color color)
            {
                Shape = shape;
                Color = color;

                // Начальное положение, учитывающее верхние точки формы
                X = BoardWidth / 2 - 1;
                Y = -shape.Min(p => p.Y);
            }

            public void Rotate()
            {
                List<Point> newShape = new List<Point>();
                foreach (var p in Shape)
                {
                    newShape.Add(new Point(-p.Y, p.X));
                }
                Shape = newShape;
            }

            public static Tetromino Create(byte type)
            {
                Tetromino tetromino = null;
                Color color = GetRandomColor();

                switch (type)
                {
                    case 1:
                        tetromino = GetJ(color);
                        break;
                    case 2:
                        tetromino = GetI(color);
                        break;
                    case 3:
                        tetromino = GetO(color);
                        break;
                    case 4:
                        tetromino = GetL(color);
                        break;
                    case 5:
                        tetromino = GetZ(color);
                        break;
                    case 6:
                        tetromino = GetT(color);
                        break;
                    case 7:
                        tetromino = GetS(color);
                        break;
                }
                return tetromino;
            }

            private static Color GetRandomColor()
            {
                // Generate a random color
                Random random = new Random();
                return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            }

            private static Tetromino GetJ(Color color)
            {
                return new Tetromino(new List<Point>
        {
            new Point(0, -1),
            new Point(0, 0),
            new Point(0, 1),
            new Point(1, -1)
        }, color);
            }

            private static Tetromino GetI(Color color)
            {
                return new Tetromino(new List<Point>
        {
            new Point(0, -1),
            new Point(0, 0),
            new Point(0, 1),
            new Point(0, 2)
        }, color);
            }

            private static Tetromino GetO(Color color)
            {
                return new Tetromino(new List<Point>
        {
            new Point(0, 0),
            new Point(0, 1),
            new Point(1, 0),
            new Point(1, 1)
        }, color);
            }

            private static Tetromino GetL(Color color)
            {
                return new Tetromino(new List<Point>
        {
            new Point(0, -1),
            new Point(0, 0),
            new Point(0, 1),
            new Point(-1, -1)
        }, color);
            }

       

            private static Tetromino GetT(Color color)
            {
                return new Tetromino(new List<Point>
        {
            new Point(0, -1),
            new Point(-1, 0),
            new Point(0, 0),
            new Point(1, 0)
        }, color);
            }

            private static Tetromino GetS(Color color)
            {
                return new Tetromino(new List<Point>
        {
            new Point(0, 0),
            new Point(0, 1),
            new Point(1, 0),
            new Point(-1, 1)
        }, color);
            }

            private static Tetromino GetZ(Color color)
            {
                return new Tetromino(new List<Point>
        {
            new Point(0, 0),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(-1, -1)
        }, color);
            }

        }

        private class BoardPixel
        {
            public Color color { get; set; }
            public bool hasBlock { get; set; }
        }



        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }



        private void CheckAndClearRows()
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                bool rowFilled = true;
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (gameField[x, y] == null || !gameField[x, y].hasBlock)
                    {
                        rowFilled = false;
                        break;
                    }
                }
                if (rowFilled)
                {
                    // Subtract 100 XP when a row is cleared, only if the game is not won
                    if (!gameWon)
                    {
                        ProgressBar.Value = Math.Max(ProgressBar.Minimum, ProgressBar.Value - 1000);

                        // Check if the progress bar has reached its minimum value
                        if (ProgressBar.Value <= ProgressBar.Minimum)
                        {
                            // You Win! Show a message and stop the game
                            inGame = false;
                            timer.Stop();
                            gameWon = true; // Set the flag to true when the game is won
                            MessageBox.Show("You Win!");
                            return;
                        }
                    }

                    // Shift rows above the cleared row down
                    for (int i = y; i > 0; i--)
                    {
                        for (int j = 0; j < BoardWidth; j++)
                        {
                            gameField[j, i] = gameField[j, i - 1];
                        }
                    }

                    // Clear the first row
                    for (int k = 0; k < BoardWidth; k++)
                    {
                        gameField[k, 0] = null;
                    }
                }
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }
    }
}
