using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Breakout
{
    public partial class MainWindow : Window
    {
        public class Coordinates
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
        
        private List<Rectangle> bricks = new List<Rectangle>();     //List of bricks
        private int curretLvl = 0;                                  //current game level
        private int ball_speed;                                     //
        private bool isClockWise = true;                            //
        private int currentDirection = 0;
        private List<string> brickSSS = new List<string>();
        private string[] levelsInfo;
        private string[] brickInfo;
        private DispatcherTimer movingTimer = new DispatcherTimer();
        Rectangle lastCollapsed = default(Rectangle);
        int skipTick = 3;

        public MainWindow()
        {
            InitializeComponent();
            levelsInfo = File.ReadAllLines("GameLevels.txt");

            curretLvl = 0;
            SetInitialState();            
            ClearCanvas();

            BrickGenerator(curretLvl);
        }

        //Used to capture the mouse move and control the paddle
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point CurrentMousePosition = Mouse.GetPosition(this);
            SetPaddlePosition(CurrentMousePosition.X - GamePaddle.ActualWidth / 2);
        }

        private void ClearCanvas()
        {
            foreach (Rectangle item in bricks)
            {
                item.Visibility = System.Windows.Visibility.Collapsed;
            }
            bricks.Clear();
        }

        public void SetInitialState()
        {
            Random rnd = new Random();
            Canvas.SetLeft(GameBall, rnd.Next(0, (int)(GameCanvas.Width - GamePaddle.Width)));
            Canvas.SetTop(GameBall, 430);

            Canvas.SetLeft(GamePaddle, 250);
            Canvas.SetTop(GamePaddle, 450);

            currentDirection = 3;
            ball_speed = 5;
        }               

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateDirection();
            MoveGameBall();
            CheckBreakCollapse();
            if (skipTick>0) skipTick--;
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            StartGameBtn.Opacity = 0;
            StartGameBtn.IsEnabled = false;

            //Here goes the timer
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatcherTimer.Start();
        }

        public void MoveGameBall()
        {
            double tempLeft = Canvas.GetLeft(GameBall);
            double tempTop = Canvas.GetTop(GameBall);

            switch (currentDirection)
            {
                // BR
                case 0:
                    Canvas.SetTop(GameBall, tempTop + ball_speed);
                    Canvas.SetLeft(GameBall, tempLeft + ball_speed);
                    break;
                // BL
                case 1:
                    Canvas.SetTop(GameBall, tempTop + ball_speed);
                    Canvas.SetLeft(GameBall, tempLeft - ball_speed);
                    break;
                // UL
                case 2:
                    Canvas.SetTop(GameBall, tempTop - ball_speed);
                    Canvas.SetLeft(GameBall, tempLeft - ball_speed);
                    break;
                // UR
                case 3:
                    Canvas.SetTop(GameBall, tempTop - ball_speed);
                    Canvas.SetLeft(GameBall, tempLeft + ball_speed);
                    break;
                default: break;
            }
        }
        
        public void UpdateDirection()
        {
            double tempBallLeft = Canvas.GetLeft(GameBall);
            double tempBallTop = Canvas.GetTop(GameBall);

            if (!CheckBottomBreakCollapse())
            {
                if (tempBallLeft >= GameCanvas.Width - GameBall.Width)
                {
                    if (currentDirection == 0)
                    {
                        isClockWise = true;
                        currentDirection = 1;
                    }
                    else
                    {
                        isClockWise = false;
                        currentDirection = 2;
                    }
                }
                else if (tempBallLeft <= 0)
                {
                    if (currentDirection == 2)
                    {
                        isClockWise = true;
                        currentDirection = 3;
                    }
                    else
                    {
                        isClockWise = false;
                        currentDirection = 0;
                    }
                }
                else if (tempBallTop <= 0)
                {
                    if (currentDirection == 3)
                    {
                        isClockWise = true;
                        currentDirection = 0;
                    }
                    else
                    {
                        isClockWise = false;
                        currentDirection = 1;
                    }
                }
                else if (tempBallTop >= GameCanvas.Height - GameBall.Width)
                {
                    if (currentDirection == 1)
                    {
                        isClockWise = true;
                        currentDirection = 2;
                    }
                    else
                    {
                        isClockWise = false;
                        currentDirection = 3;
                    }
                }
            }
        }

        public bool CheckBottomBreakCollapse()
        {
            double paddleTop = Canvas.GetTop(GamePaddle);
            double paddleLeft = Canvas.GetLeft(GamePaddle);
            double paddleRight = Canvas.GetLeft(GamePaddle) + GamePaddle.Width;
            double paddleBottom = Canvas.GetTop(GamePaddle) + GamePaddle.Height;
            double ballBottom = Canvas.GetTop(GameBall) + GameBall.Width;
            double ballLeft = Canvas.GetLeft(GameBall) + (GameBall.Width / 2);

            if (ballBottom >= paddleTop && ballLeft > paddleLeft && ballLeft < paddleRight && ballBottom < paddleBottom)
            {
                if (currentDirection == 0)
                {
                    currentDirection = 3;
                }
                else if (currentDirection == 1)
                {
                    currentDirection = 2;
                }
                return true;
            }
            else if (ballBottom > paddleBottom + (GamePaddle.Height / 2))
            {
                //Game Over
                MessageBox.Show("You lost!!!");
                curretLvl = 0;
                SetInitialState();
                ClearCanvas();
                BrickGenerator(curretLvl);
                return false;
            }
            return false;
        }

        public void ChangeBallDirection(Rectangle crashedBrick, Coordinates nearCoordinate)
        {
            switch (currentDirection)
            {
                case 0:

                    if (isClockWise)
                        currentDirection = 1;
                    else
                        currentDirection = 3;
                    break;

                case 1:

                    if (isClockWise)
                        currentDirection = 2;
                    else
                        currentDirection = 0;
                    break;

                case 2:

                    if (isClockWise)
                        currentDirection = 3;
                    else
                        currentDirection = 1;
                    break;

                case 3:

                    if (isClockWise)
                        currentDirection = 0;
                    else
                        currentDirection = 2;
                    break;
            }
        }


        private void BrickGenerator(int currentLvl)
        {
            Rectangle rct;

            try
            {
                bricks.Clear();

                if (levelsInfo.Length <= currentLvl)
                {
                    movingTimer.Stop();
                    MessageBox.Show("You won. Congratulation!!!");
                }
                else
                {
                    brickInfo = levelsInfo[currentLvl].Split(',');

                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            rct = new Rectangle
                            {
                                Opacity = 1
                            };

                            if (!string.IsNullOrWhiteSpace(brickInfo[(j + (i * 10))]))
                            {
                                int brickType = Convert.ToInt16(brickInfo[(j + (i * 10))]);

                                switch (brickType)
                                {
                                    case 0:
                                        break;

                                    case 1:
                                        rct.Fill = Brushes.Green;
                                        break;

                                    case 2:
                                        rct.Fill = Brushes.Red;
                                        break;

                                    case 3:
                                        rct.Fill = Brushes.Blue;
                                        break;
                                }

                                rct.Height = 25;
                                rct.Width = 60;
                                rct.RadiusX = 1;
                                rct.RadiusY = 1;
                                Canvas.SetLeft(rct, (j * 60));
                                Canvas.SetTop(rct, (i * 25));
                                bricks.Insert((j + (i * 10)), rct);
                                GameCanvas.Children.Insert(0, rct);
                            }
                            else
                            {
                                rct.Visibility = System.Windows.Visibility.Collapsed;
                                bricks.Add(rct);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<Coordinates> GetBorderPoints()
        {
            List<Coordinates> pointLists = new List<Coordinates>();
            Coordinates TL = new Coordinates { X = (int)Canvas.GetLeft(GameBall), Y = (int)Canvas.GetTop(GameBall) };
            Coordinates TR = new Coordinates { X = (int)(Canvas.GetLeft(GameBall) + GameBall.ActualWidth), Y = (int)Canvas.GetTop(GameBall) };
            Coordinates BR = new Coordinates { X = (int)(Canvas.GetLeft(GameBall) + GameBall.ActualWidth), Y = (int)(Canvas.GetTop(GameBall) + GameBall.ActualHeight) };
            Coordinates BL = new Coordinates { X = (int)Canvas.GetLeft(GameBall), Y = (int)(Canvas.GetTop(GameBall) + GameBall.ActualHeight) };
            pointLists.Add(TL);
            pointLists.Add(TR);
            pointLists.Add(BR);
            pointLists.Add(BL);

            return pointLists;
        }

        private bool CheckBreakCollapse()
        {
            bool neighbourBrick = false;
            double ballBottom = Canvas.GetTop(GameBall) + GameBall.Width;
            double ballLeft = Canvas.GetLeft(GameBall) - (GameBall.Width / 2);
            double ballRight = Canvas.GetLeft(GameBall) + GameBall.Width;
            double ballTop = Canvas.GetTop(GameBall);
            double ballCenterX = Canvas.GetTop(GameBall) + (GameBall.Width / 2);
            double ballCenterY = Canvas.GetLeft(GameBall) + (GameBall.Width / 2);
            double brickTop;
            double brickBottom;
            double brickLeft;
            double brickRight;
            var ballCoordinate = GetBorderPoints();
            
            var conflictedBrick = bricks.FirstOrDefault(brick => ballCoordinate.Any(ballPoint =>
                                                                     ballPoint.X >= Canvas.GetLeft(brick) &&
                                                                     ballPoint.X <= Canvas.GetLeft(brick) + brick.Width &&
                                                                     ballPoint.Y <= Canvas.GetTop(brick) + brick.Height &&
                                                                     ballPoint.Y >= Canvas.GetTop(brick)));

            if (conflictedBrick != null)
            {
                Rectangle cBrick = conflictedBrick;
                
                if (lastCollapsed == cBrick && skipTick > 0)
                {
                    skipTick--;
                    return false;
                }
                else
                {
                    skipTick = 5;
                    lastCollapsed = cBrick;
                }
                
                var hitCoordinate = ballCoordinate.Where(ballPoint =>
                                        ballPoint.X >= Canvas.GetLeft(cBrick) &&
                                        ballPoint.X <= Canvas.GetLeft(cBrick) + cBrick.Width &&
                                        ballPoint.Y <= Canvas.GetTop(cBrick) + cBrick.Height &&
                                        ballPoint.Y >= Canvas.GetTop(cBrick)).FirstOrDefault();
                
                if (cBrick == default(Rectangle))
                    MessageBox.Show("Error");

                brickTop = Canvas.GetTop(cBrick);
                brickBottom = Canvas.GetTop(cBrick) + cBrick.Height;
                brickLeft = Canvas.GetLeft(cBrick);
                brickRight = Canvas.GetLeft(cBrick) + cBrick.Width;

                if (hitCoordinate.Y >= brickTop && //top
                    hitCoordinate.X < brickRight &&
                    hitCoordinate.X > brickLeft &&
                    ballTop < brickTop)
                {
                    isClockWise = currentDirection == 0 ? false : true;
                }
                else if (hitCoordinate.X >= brickLeft && //left
                         hitCoordinate.Y < brickBottom &&
                         hitCoordinate.Y > brickTop &&
                         ballLeft < brickLeft)
                {
                    isClockWise = currentDirection == 3 ? false : true;
                }
                else if (hitCoordinate.Y <= brickBottom && //bottom
                        hitCoordinate.X < brickRight &&
                        hitCoordinate.X > brickLeft &&
                        ballBottom > brickBottom)
                {
                    isClockWise = currentDirection == 3 ? true : false;
                }
                else if (hitCoordinate.X <= brickRight && //right
                         hitCoordinate.Y < brickBottom &&
                         hitCoordinate.Y > brickTop &&
                         ballRight > brickRight)
                {
                    isClockWise = currentDirection == 1 ? false : true;
                }
                else if (hitCoordinate.X == brickRight && //TR
                         hitCoordinate.Y == brickTop)
                {
                    if (currentDirection == 0)
                    {
                        isClockWise = false;
                    }
                    else if (currentDirection == 2)
                    {
                        isClockWise = true;
                    }
                    else // currentDirection == 1
                    {
                        for (int i = 0; i < bricks.Count(); i++)
                        {
                            if (Canvas.GetLeft(bricks.ElementAt(i)) == brickRight)
                            {
                                neighbourBrick = true;
                                isClockWise = true;
                                break;
                            }
                        }
                        if (neighbourBrick == false)
                        {
                            isClockWise = false;
                        }
                        neighbourBrick = false;
                    }
                }
                else if (hitCoordinate.X == brickLeft && //TL
                         hitCoordinate.Y == brickTop)
                {
                    if (currentDirection == 1)
                    {
                        isClockWise = true;
                    }
                    else if (currentDirection == 3)
                    {
                        isClockWise = false;
                    }
                    else //currentDirection == 0
                    {
                        for (int i = 0; i < bricks.Count(); i++)
                        {
                            if (Canvas.GetLeft(bricks.ElementAt(i)) + bricks.ElementAt(i).ActualWidth == brickLeft)
                            {
                                neighbourBrick = true;
                                isClockWise = false;
                                break;
                            }
                        }
                        if (neighbourBrick == false)
                        {
                            isClockWise = true;
                        }
                        neighbourBrick = false;
                    }
                }
                else if (hitCoordinate.X == brickLeft && //BL
                         hitCoordinate.Y == brickBottom)
                {
                    if (currentDirection == 2)
                    {
                        isClockWise = false;
                    }
                    else if (currentDirection == 0)
                    {
                        isClockWise = true;
                    }
                    else //currentDirection == 3
                    {
                        for (int i = 0; i < bricks.Count(); i++)
                        {
                            if (Canvas.GetLeft(bricks.ElementAt(i)) + bricks.ElementAt(i).ActualWidth == brickLeft)
                            {
                                neighbourBrick = true;
                                isClockWise = true;
                                break;
                            }
                        }
                        if (neighbourBrick == false)
                        {
                            isClockWise = false;
                        }
                        neighbourBrick = false;
                    }
                }
                else if (hitCoordinate.X == brickRight && //BR
                         hitCoordinate.Y == brickBottom)
                {
                    if (currentDirection == 3)
                    {
                        isClockWise = true;
                    }
                    else if (currentDirection == 1)
                    {
                        isClockWise = false;
                    }
                    else //currentDirection == 2
                    {
                        for(int i = 0; i < bricks.Count(); i++)
                        {
                            if (Canvas.GetLeft(bricks.ElementAt(i)) == brickRight)
                            {
                                neighbourBrick = true;
                                isClockWise = false;
                                break;
                            }
                        }
                        if (neighbourBrick == false)
                        {
                            isClockWise = true;
                        }
                        neighbourBrick = false;
                    }
                }

                ChangeBallDirection(cBrick, hitCoordinate);
                int index = bricks.IndexOf(cBrick);

                if (index < 0)
                    MessageBox.Show("Incorrect brick");


                if (brickInfo[index] == "3")
                {
                    cBrick.Fill = Brushes.Red;
                    brickInfo[index] = "2";
                }
                else if (brickInfo[index] == "2")
                {
                    cBrick.Fill = Brushes.Green;
                    brickInfo[index] = "1";
                }
                else
                {
                    GameCanvas.Children.Remove(cBrick);
                    conflictedBrick.Visibility = System.Windows.Visibility.Collapsed;
                    bricks.Remove(cBrick);
                    brickInfo[index] = "0";
                }

                brickInfo = brickInfo.Where(s => s.ToString() != "0").ToArray();

                if (bricks.Where(s => s.Visibility == System.Windows.Visibility.Visible).Count() == 0)
                {
                    MessageBox.Show(string.Format("You have completed Stage : {0}!!! ", curretLvl + 1));
                    ball_speed ++;
                    BrickGenerator(++curretLvl);
                    return true;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        //Sets the paddle horizontal position according to parameter X.
        public void SetPaddlePosition(double positionX)
        {
            if (positionX <= 0)
            {
                Canvas.SetLeft(GamePaddle, 0);
            }
            else if (positionX >= GameCanvas.Width - GamePaddle.Width)
            {
                Canvas.SetLeft(GamePaddle, GameCanvas.Width - GamePaddle.Width);
            }
            else
            {
                Canvas.SetLeft(GamePaddle, positionX);
            }
        }
    }
}