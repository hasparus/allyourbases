using System;
using System.IO;
using Sophie.Utils;

namespace Sophie.IO
{
    public class StdinIOController : IOController
    {
        private const int ReadLineBufferSize = 1 << 13 /*8192*/; 
        private void ChangeReadlineBufferSize()
        {
            
        }

        public StdinIOController()
        {
        }

    }
}
