using System;
using System.Collections.Generic;
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
using System.Timers;

namespace WPFSnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Game game = new Game();
        Timer timer = new Timer();
        int speed = 100;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            handleKeyUp(e);
        }

        void handleKeyUp(KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("handleKeyUp " + e.Key.ToString());
            if (!game.IsValidMove(e.Key))
            {
                return;
            }

            if (!game.IsGameOver)
            {
                game.SnakeMovesSound.Play();
            }

            if (game.HasPlayerStartedGame)
            {
                game.handleKeyEnum(e.Key, false);
            }
            else
            {
                if (Array.IndexOf(game.KEYS.AllKeys, e.Key) != -1)
                {
                    game.HasPlayerStartedGame = true;
                    game.handleKeyEnum(e.Key, true);
                    game.BackgroundMusic.PlayAndLoop();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            game.go(GameArea, TextBlockApples, TextBlockScore);
            timer.Interval = speed;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() => // timer calls should be thread safe
            {
                game.moveSnake();
            });

        }

    }
}
