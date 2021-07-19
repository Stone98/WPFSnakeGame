using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WPFSnakeGame
{
    class GameSound
    {
        MediaPlayer mediaPlayer;     
        public string Source { get; set; }
        public GameSound(string source)
        {
            string exePath = Environment.CurrentDirectory;
            string soundPath = Path.Combine(exePath, source);
            this.Source = source;
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(soundPath));
        }
        public void PlayAndLoop()
        {
            mediaPlayer.Play();
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }

        public void Stop()
        {
            mediaPlayer.Stop();
        }
        public void Play()
        {
            mediaPlayer.Play();
        }
    }
}
