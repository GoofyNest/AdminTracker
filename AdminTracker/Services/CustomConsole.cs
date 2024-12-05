using System;

public static class Custom
{
    private static readonly object _lockObj = new object();

    public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
    {
        lock (_lockObj) // Ensure that only one thread can write to the console at a time
        {
            var prefix = "";
            switch (color)
            {
                case ConsoleColor.DarkYellow:
                    prefix = "[WARN] ";
                    break;

                case ConsoleColor.Magenta:
                case ConsoleColor.DarkMagenta:
                    prefix = "[DEBUG] ";
                    break;

                case ConsoleColor.Red:
                case ConsoleColor.DarkRed:
                    prefix = "[ERROR] ";
                    break;

                case ConsoleColor.Cyan:
                    prefix = "[NOTIFY] ";
                    break;

                case ConsoleColor.Yellow:
                    prefix = "[CHEATER] ";
                    break;

                case ConsoleColor.Blue:
                case ConsoleColor.DarkBlue:
                    prefix = "[INFO] ";
                    break;

                default:
                    prefix = "[NORMAL] ";
                    color = ConsoleColor.Green;
                    break;
            }

            Console.ForegroundColor = color;
            Console.Write(prefix);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"{message}");
        }
    }
}
