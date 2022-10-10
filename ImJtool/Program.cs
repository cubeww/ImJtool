using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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

