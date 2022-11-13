using System;

namespace ImJtool
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Run ImJtool
            using var game = Jtool.Instance;
            game.Args = args;
            game.Run();
        }
    }
}

