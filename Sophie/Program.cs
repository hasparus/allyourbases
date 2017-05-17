using System;
using System.Text.RegularExpressions;

namespace Sophie
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string input;
            while ((input = Console.ReadLine()) != null)
            {
                Console.WriteLine(
                    /*Regex.Replace(*/Logic.ExecuteInputLine(input)/*, @"\r\n?|\n", " ")*/);
            } 
        }
    }
}
