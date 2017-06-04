using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
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
                // polecam zmienić formatowanie z None na Indented, jeśli zamierza Pani to czytać :D
                CallResult.Formatting = Formatting.Indented;

                var output = IoController.ExecuteInputLine(input);
                if (output != "") Console.WriteLine(output);
            } 
        }
    }
}
