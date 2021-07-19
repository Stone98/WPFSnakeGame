using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFSnakeGame
{
    class Keys
    {
        public Key[] KeyUp { get; set; }
        public Key[] KeyRight { get; set; }
        public Key[] KeyLeft { get; set; }
        public Key[] KeyDown { get; set; }
        public Key[] Pause { get; set; }
        public Key[] AllKeys { get; set; }

        public Keys()
        {
            this.KeyUp = new Key[] { Key.W, Key.Up };
            this.KeyRight = new Key[] { Key.D, Key.Right };
            this.KeyLeft = new Key[] { Key.A, Key.Left };
            this.KeyDown = new Key[] { Key.S, Key.Down };
            this.Pause = new Key[] { Key.P, Key.Space };
            this.AllKeys = new Key[] { };

            var allKeys = new List<Key>();
            allKeys.AddRange(this.KeyUp);
            allKeys.AddRange(this.KeyRight);
            allKeys.AddRange(this.KeyLeft);
            allKeys.AddRange(this.KeyDown);
            allKeys.AddRange(this.Pause);
            allKeys.AddRange(this.AllKeys);

            this.AllKeys = allKeys.ToArray();
        }
    }
}
