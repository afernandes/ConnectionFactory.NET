using System;
using ConnectionFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;

namespace ConnectionFactoryTest
{
    [TestClass]
    public class UnitTest1
    {
        protected static readonly bool IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

        protected static string ConnName => IsAppVeyor ? "AppVeyor" : "teste";

        [TestMethod]
        public void TestCommandWithDynamicParameters()
        {
            string returnValue = null;
            using (var conn = new CfConnection(ConnName))
            {
                var cmd = conn.CreateCfCommand();

                DbDataReader result = (DbDataReader)cmd.ExecuteReader(CfCommandType.Text,
                    @"select *
                          from(select 'user1' as login) as t
                          where login = 'user1'",
                    new { login = "user1" });

                if (result.Read())
                {
                    returnValue = result["login"].ToString();
                }
            }

            Assert.AreEqual(returnValue, "user1");

        }

        [TestMethod]
        public void TestCommandWithExpandoObjectParameters()
        {
            string returnValue = null;
            using (var conn = new CfConnection(ConnName))
            {
                var cmd = conn.CreateCfCommand();

                var paramters = new ExpandoObject() as IDictionary<string, object>;
                paramters.Add("login", "andersonn");

                DbDataReader result = (DbDataReader)cmd.ExecuteReader(CfCommandType.Text,
                      @"select *
                          from(select 'user1' as login) as t
                          where login = 'user1'",
                    paramters);

                if (result.Read())
                {
                    returnValue = result["login"].ToString();
                }
            }

            Assert.AreEqual(returnValue, "user1");
        }

    }
}
