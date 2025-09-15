using Extensions.Pack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RunJit.Cli.Test.Help
{
    [Ignore]
    [TestCategory("runjit --help")]
    [TestClass]
    public class HelpTest
    {
        [TestMethod]
        [DataRow("runjit --help", "Help.Outputs.runjit-help.txt")]
        [DataRow("runjit generate --help", "Help.Outputs.runjit-generate-help.txt")]
        [DataRow("runjit generate rest-endpoint --help", "Help.Outputs.runjit-generate-rest-endpoint-help.txt")]
        [DataRow("runjit generate action-controller --help", "Help.Outputs.runjit-generate-action-endpoint-help.txt")]
        [DataRow("runjit generate client --help", "Help.Outputs.runjit-generate-client-help.txt")]
        [DataRow("runjit generate tests --help", "Help.Outputs.runjit-generate-tests-help.txt")]
        [DataRow("runjit new --help", "Help.Outputs.runjit-new-help.txt")]
        [DataRow("runjit new lambda --help", "Help.Outputs.runjit-new-lambda-help.txt")]
        [DataRow("runjit new webapi --help", "Help.Outputs.runjit-new-webapi-help.txt")]
        [DataRow("runjit new rest-endpoint --help", "Help.Outputs.runjit-new-rest-endpoint-help.txt")]
        [DataRow("runjit database --help", "Help.Outputs.runjit-database-help.txt")]
        [DataRow("runjit database delete --help", "Help.Outputs.runjit-database-delete-help.txt")]
        [DataRow("runjit database delete table --help", "Help.Outputs.runjit-database-delete-table-help.txt")]
        [DataRow("runjit database new --help", "Help.Outputs.runjit-database-new-help.txt")]
        [DataRow("runjit database new table --help", "Help.Outputs.runjit-database-new-table-help.txt")]
        public async Task Should_Print_Out_Expected_Help_Infos_On_Pulse_Command(string parameters,
                                                                                string expectedOutput)
        {
            await using var sw = new StringWriter();
            Console.SetOut(sw);

            var paramterAsArray = parameters.Split(" ");
            var exitCode = await Program.Main(paramterAsArray).ConfigureAwait(false);
            var output = sw.ToString();

            Assert.AreEqual(0, exitCode, output);

            var fileContent = EmbeddedFile.GetFileContentFrom(expectedOutput);

            Assert.AreEqual(fileContent, output);
        }
    }
}
