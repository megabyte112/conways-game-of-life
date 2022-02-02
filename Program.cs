using System;

namespace gameoflife
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new gameoflife())
                game.Run();
        }
    }
}
