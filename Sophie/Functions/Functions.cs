using Sophie.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Sophie.Functions
{
    public static class Functions
    {
        public static ReferenceHash<string, Func<JObject, JObject>> Hash =
            new ReferenceHash<string, Func<JObject, JObject>>
            {
                {"open", x => throw new NotImplementedException()},
                {"organizer", x => throw new NotImplementedException()},
                {"event", x => throw new NotImplementedException()},
            };
    }
}
