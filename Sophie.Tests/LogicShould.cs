using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;
using Sophie;
using Xunit.Abstractions;

namespace Sophie.Tests
{
    public class LogicShould
    {
        private readonly ITestOutputHelper _output;
        public LogicShould(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SetupDatabase()
        {
            JObject jo = JObject.Parse(
                "{ \"baza\": \"hasparus\", \"login\": \"beata\", \"password\": \"beata\" }");
            var res = Logic.Call("open", jo);
            Assert.Equal(Logic.Result.Ok.State.ToString(), res["status"]);
        }

        [Theory]
        [InlineData(@"{ ""daza"": ""hasparus"", ""login"": ""beata"", ""password"": ""beata""}")]
        [InlineData(@"{ ""baza"": ""wrongName"", ""login"": ""beata"", ""password"": ""beata""}")]
        [InlineData(@"{ ""baza"": ""hasparus"", ""login"": ""wrong user"", ""password"": ""beata""}")]
        [InlineData(@"{ ""baza"": ""hasparus"", ""login"": ""beata"", ""password"": ""wrong password""}")]
        [InlineData(@"{ ""baza"": ""hasparus"", ""login"": ""wrong user""}")]
        public void FailBaseSetupWithWrongParameters(string s)
        {
            var res = Logic.Call("open", JObject.Parse(s));
            Assert.Equal(Logic.Result.Status.Error, Logic.Result.Error.State);
        }

        [Theory, InlineData("organiser", "secret", "newlogin", "newpassword")]
        public void AddOrganiser(string funcName, string[] params ParametersEncoder)
        {
            var jo = JObject.Parse(
                @"{ ""organizer"": { ""secret"": ""d8578edf8458ce06fbc5bb76a58c5ca4"", ""newlogin"": """ +
                $"testOrganise{new System.Random().Next()}" +
                @", ""newpassword"": ""haslo"" } }");

        }
    }

}
