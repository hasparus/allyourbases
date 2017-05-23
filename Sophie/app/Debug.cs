using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sophie
{
    internal static class Debug
    {
        public static Action<string> Log = Console.Error.WriteLine;
    }
}
