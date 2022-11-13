using Microsoft.Xna.Framework.Input;

namespace ImJtool.Managers
{
    /// <summary>
    /// Check keyboard input.
    /// </summary>
    public static class InputManager
    {
        static bool[] KeyPress { get; set; }
        static bool[] KeyHold { get; set; }
        static bool[] KeyRelease { get; set; }
        public static void Initialize()
        {
            KeyPress = new bool[256];
            KeyHold = new bool[256];
            KeyRelease = new bool[256];

            Jtool.Instance.Window.KeyDown += (s, e) =>
            {
                if (!KeyHold[(int)e.Key])
                {
                    KeyPress[(int)e.Key] = true;
                    KeyHold[(int)e.Key] = true;
                }

            };

            Jtool.Instance.Window.KeyUp += (s, e) =>
            {
                KeyRelease[(int)e.Key] = true;
                KeyHold[(int)e.Key] = false;
            };
        }

        public static void ClearPressAndRelease()
        {
            for (int i = 0; i < KeyPress.Length; i++)
            {
                KeyPress[i] = false;
                KeyRelease[i] = false;
            }
        }

        public static bool IsKeyPress(Keys key)
        {
            return KeyPress[(int)key];
        }
        public static bool IsKeyHold(Keys key)
        {
            return KeyHold[(int)key];
        }
        public static bool IsKeyRelease(Keys key)
        {
            return KeyRelease[(int)key];
        }
    }
}
