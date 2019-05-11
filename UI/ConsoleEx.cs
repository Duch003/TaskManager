using System;
using System.Collections.Generic;
using System.Text;

namespace UI
{
    public static class ConsoleEx
    {
        public static void WriteLine(string text, ConsoleColor color)
        {
            SetColor(color);
            Console.WriteLine(text);
            RestoreColor();
        }

        public static void Write(string text, ConsoleColor color)
        {
            SetColor(color);
            Console.Write(text);
            RestoreColor();
        }

        public static void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public static void Write(string text)
        {
            Console.Write(text);
        }

        public static void Write<T>(params T[] text)
        {
            foreach(var item in text)
            {
                Console.Write($"{item}; ");
            }
        }

        private static void SetColor(ConsoleColor color) => Console.ForegroundColor = color;

        private static void RestoreColor() => Console.ForegroundColor = ConsoleColor.Gray;
    }
}
