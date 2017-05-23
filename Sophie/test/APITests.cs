using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Sophie.Test
{
    [TestFixture]
    class APITests
    {
        [Test]
        public void OpensDatabase()
        {
            var res = Logic.Call("open",
                JObject.Parse("{ \"baza\": \"hasparus\", \"login\": \"beata\", \"password\": \"beata\" }"));

            Assert.AreEqual(res["status"].ToString(), "Ok");
        }
    }
}
