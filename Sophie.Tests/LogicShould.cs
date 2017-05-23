using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;
using Sophie;
using Xunit.Abstractions;

namespace Sophie.Tests
{
    public class LogicShould : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public LogicShould(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("Test started.");
        }

        #region facts

        public void Dispose()
        {
            Logic.Call("drop_the_base", JObject.FromObject(new { secret = 42 }));
        }

        [Fact]
        public void DropDatabase()
        {
            SetupDatabase();
            var res = Logic.Call("drop_all", JObject.FromObject(new { secret = 42 }));
            Assert.Equal(CallResult.Status.Ok, ExtractStatus(res));
        }

        [Fact]
        public void SetupDatabase()
        {
            JObject jo = JObject.Parse(
                "{ \"baza\": \"testbase\", \"login\": \"beata\", \"password\": \"beata\" }");

            var res = Logic.Call("open", jo);
            Assert.Equal(CallResult.Status.Ok, Enum.Parse(typeof(CallResult.Status), res["status"].ToString()));
        }

        #endregion

        #region theories

        [Theory,
         InlineData(@"{ ""daza"": ""testbase"", ""login"": ""beata"", ""password"": ""beata""}"),
         InlineData(@"{ ""baza"": ""noBaseLikeThat"", ""login"": ""beata"", ""password"": ""beata""}"),
         InlineData(@"{ ""baza"": ""testbase"", ""login"": ""wrong user"", ""password"": ""beata""}"),
         InlineData(@"{ ""baza"": ""testbase"", ""login"": ""beata"", ""password"": ""wrong password""}"),
         InlineData(@"{ ""baza"": ""testbase"", ""login"": ""wrong user""}")]
        public void FailBaseSetupWithWrongParameters(string s)
        {
            var res = Logic.Call("open", JObject.Parse(s));
            Assert.Equal(
                CallResult.Status.Error,
                ExtractStatus(res));
        }

        [Theory,
            InlineData("organizer",
            "secret", "d8578edf8458ce06fbc5bb76a58c5ca4",
            "newlogin", "Jarek",
            "newpassword", "Puszek")
            ]
        public void DelegateIndepententProcedure(string funcName, params string[] parameters)
        {
            SetupDatabase();
            InvokeProcedure(funcName, parameters);
        }

        private void InvokeProcedure(string funcName, params string[] parameters)
        {
            var paramsDict = new Dictionary<string, string>();
            for (var i = 1; i < parameters.Length; i += 2)
                paramsDict[parameters[i - 1]] = parameters[i];

            var jo = JObject.FromObject(paramsDict);
            var res = Logic.Call(funcName, jo);
            Assert.Equal(
                CallResult.Status.Ok,
                ExtractStatus(res));
        }

        private static CallResult.Status ExtractStatus(JObject jo)
            => (CallResult.Status) Enum.Parse(typeof(CallResult.Status), jo["status"].ToString());

        #endregion
    }

}
