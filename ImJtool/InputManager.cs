using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace ImJtool
{
    /// <summary>
    /// Check keyboard input.
    /// </summary>
    public class InputManager
    {
        public static InputManager Instance => Jtool.Instance.InputManager;

        KeyboardState state;
        KeyboardState lastState;

        Keys[] keys;
        bool[] keyPress;
        bool[] keyRelease;

        public InputManager()
        {
            keys = (Keys[])Enum.GetValues(typeof(Keys));

            state = Keyboard.GetState();
            lastState = state;
        }

        public void Update()
        {
            state = Keyboard.GetState();
        }

        public void AfterUpdate()
        {
            lastState = state;
        }

        public void ClearPressAndRelease()
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keyPress[i] = false;
                keyRelease[i] = false;
            }
        }

        public bool IsKeyPress(Keys key)
        {
            if( state.IsKeyDown(key) && lastState.IsKeyUp(key))
            {
                Debug.WriteLine($"Press {key}");
                return true;
            }

            return false;
        }
        public bool IsKeyHold(Keys key)
        {
            return state.IsKeyDown(key);
        }
        public bool IsKeyRelease(Keys key)
        {
            if( state.IsKeyUp(key) && lastState.IsKeyDown(key))
            {
                Debug.WriteLine($"Release {key}");
                return true;
            }
            return false;
        }
    }
}
