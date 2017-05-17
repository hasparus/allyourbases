using System;
using System.IO;
using Sophie.Utils;

namespace Sophie.IO
{
    public class StdinIOController : IOController
    {
        public StdinIOController()
        {
            // [ Note ]
            // Bad news: .NET Core lacks OpenStandardInput(int) method
            // for extending ReadLine buffer size.
            // Good news: In .NET Core console app running on Ubuntu
            // there is no limit on ReadLine buffer size.


        }

    }
}
