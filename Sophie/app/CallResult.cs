using System;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sophie
{
    public struct CallResult
    {
        public static Formatting Formatting = Formatting.None;

        public readonly Status State;
        public readonly JArray Data;


        public override int GetHashCode()
        {
            return (int) State;
        }

        public static CallResult Ok =>
            new CallResult(Status.Ok);

        public static CallResult NotImplemented =>
            new CallResult(Status.NotImplemented);

        public static CallResult Error(string message)
        {
            Debug.Log(message);
            return new CallResult(Status.Error);
        }

        public enum Status
        {
            Ok,
            NotImplemented,
            Error
        }

        public CallResult(Status state, JArray data = null)
        {
            State = state;
            Data = data;
        }

        public JObject ToJObject()
        {
            var jo = new JObject
            {
                ["status"] = State.Humanize()
            };

            if (Data != null && Data.Count > 0)
                jo["data"] = Data;
            return jo;
        }

        public override string ToString()
        {
            return ToJObject().ToString(Formatting);
        }

        public static bool operator ==(CallResult one, CallResult two)
        {
            return one.State == two.State;
        }

        public static bool operator !=(CallResult one, CallResult two)
        {
            return !(one == two);
        }

        public bool Equals(CallResult other)
        {
            return State == other.State;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CallResult && Equals((CallResult) obj);
        }

        public static CallResult FromStatusHumanizedString(string s) 
            => Enum.TryParse(s.Dehumanize(), out Status isCallResult) 
            ? new CallResult(isCallResult) 
            : Error("Coudn't parse. Returning error.");

        public static string DbError = "0";
    }
}
