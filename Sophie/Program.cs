using System;
using System.Text.RegularExpressions;
using Sophie.IO;

namespace Sophie
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string input;
            while ((input = Console.ReadLine()) != null)
            {
                var output = IoController.ExecuteInputLine(input);
                if (output != "") Console.WriteLine(output);
            } 
        }
    }
}
