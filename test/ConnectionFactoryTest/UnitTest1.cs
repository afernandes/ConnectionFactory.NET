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
        [TestMethod]
        public void TestCommandWithDynamicParameters()
        {
            try
            {
                using (var conn = new CfConnection("teste"))
                {
                    var cmd = conn.CreateCfCommand();

                    DbDataReader result = (DbDataReader)cmd.ExecuteReader(CfCommandType.Text,
                        "select * from sisuser.sis_user where login = @login",
                        new { login = "andersonn" });

                    if (result.Read())
                    {
                        var ret = result["login"].ToString();
                        Assert.IsTrue(!string.IsNullOrEmpty(ret));
                    }
                }

                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestCommandWithExpandoObjectParameters()
        {
            try
            {
                using (var conn = new CfConnection("teste"))
                {
                    var cmd = conn.CreateCfCommand();

                    var paramters = new ExpandoObject() as IDictionary<string, object>;
                    paramters.Add("login", "andersonn");

                    DbDataReader result = (DbDataReader)cmd.ExecuteReader(CfCommandType.Text,
                        "select * from sisuser.sis_user where login = @login",
                        paramters);

                    if (result.Read())
                    {
                        var ret = result["login"].ToString();
                        Assert.IsTrue(!string.IsNullOrEmpty(ret));
                    }
                }

                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }

    }
}
