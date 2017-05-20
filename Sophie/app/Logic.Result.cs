using System;
using Humanizer;
using Newtonsoft.Json.Linq;

namespace Sophie
{
    public static partial class Logic
    {
        private static Result Error(string message)
        {
            Console.Error.WriteLine(message);
            return Result.Error;
        }

        public struct Result
        {
            public static Result Ok => 
                new Result(Status.Ok);
            public static Result NotImplemented => 
                new Result(Status.NotImplemented);
            public static Result Error => 
                new Result(Status.Error);

            public enum Status
            {
                Ok,
                NotImplemented,
                Error
            }

            public readonly Status State;

            public Result(Status state)
            {
                State = state;
            }

            public JObject ToJObject() => 
                new JObject
            {
                ["status"] = State.Humanize(),
            };

            public override string ToString()
            {
                return ToJObject().ToString();
            }
        }
        
        
    }
}
