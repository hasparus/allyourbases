﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sophie.IO;

namespace Sophie
{
    internal static class Debug
    {
        public static Action<string> Log = 
            s => Console.Error.WriteLine(IoController.LineNumber + ": " + s);
    }
}
    