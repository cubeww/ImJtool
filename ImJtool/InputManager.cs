using Microsoft.Xna.Framework.Input;
using System;

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

        public bool IsKeyPress(Keys key)
        {
            return state.IsKeyDown(key) && lastState.IsKeyUp(key);
        }
        public bool IsKeyHold(Keys key)
        {
            return state.IsKeyDown(key);
        }
        public bool IsKeyRelease(Keys key)
        {
            return state.IsKeyUp(key) && lastState.IsKeyDown(key);
        }
    }
}
