using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sophie
{
    public struct CallResult
    {
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

        public readonly Status State;

        public CallResult(Status state)
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
            return ToJObject().ToString(Formatting.None);
        }
    }
}
