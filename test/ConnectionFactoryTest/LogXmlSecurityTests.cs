using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using log4net.Core;
using log4net.Layout;
using log4net.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConnectionFactoryTest
{
    [TestClass]
    public class LogXmlSecurityTests
    {
        [TestMethod]
        public void XmlLayout_CaracteresProibidosNosMetadados_PreservaEventoXmlValido()
        {
            ValidarEvento(new XmlLayout());
        }

        [TestMethod]
        public void XmlLayoutSchemaLog4j_CaracteresProibidosNosMetadados_PreservaEventoXmlValido()
        {
            ValidarEvento(new XmlLayoutSchemaLog4J());
        }

        private static void ValidarEvento(XmlLayoutBase layout)
        {
            const string mensagem = "Registro de auditoria preservado \U0001F600";
            var propriedades = new PropertiesDictionary();
            propriedades["chave\u0000final"] = "valor\u0001final";

            var evento = new LoggingEvent(typeof(LogXmlSecurityTests),
                log4net.LogManager.GetRepository(typeof(LogXmlSecurityTests).Assembly), new LoggingEventData
            {
                LoggerName = "ConnectionFactory.Security",
                Level = Level.Info,
                Message = mensagem,
                ThreadName = "teste",
                TimeStampUtc = DateTimeOffset.UtcNow.UtcDateTime,
                Identity = "identidade\u0000final",
                UserName = "usuario",
                Domain = "teste",
                Properties = propriedades
            });

            layout.ActivateOptions();
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                layout.Format(writer, evento);
                var xml = writer.ToString();
                var documento = XDocument.Parse(
                    "<root xmlns:log4j=\"http://jakarta.apache.org/log4j/\" xmlns:log4net=\"urn:log4net\">" + xml + "</root>");

                Assert.AreEqual(1, documento.Root.Elements().Count());
                Assert.AreEqual("event", documento.Root.Elements().Single().Name.LocalName);
                Assert.IsTrue(documento.Descendants().Any(elemento =>
                    elemento.Name.LocalName == "message" && elemento.Value == mensagem));
                Assert.IsTrue(xml.Contains("chave"));
                Assert.IsTrue(xml.Contains("valor"));
                Assert.IsTrue(xml.Contains("identidade"));
                Assert.IsFalse(xml.Contains("\u0000"));
                Assert.IsFalse(xml.Contains("\u0001"));
            }
        }
    }
}
