using DotNetTool.Service;

namespace $ProjectName$.Test.Utils
{
    internal sealed class DockerService(IDotNetTool dotNet)
    {
        internal async Task RunAsync(string dynamoDbContainerName)
        {
            var dockerRunResult = await dotNet.RunAsync("docker", $"run -d -p 8001:8000 --name {dynamoDbContainerName} amazon/{dynamoDbContainerName} -jar DynamoDBLocal.jar -sharedDb").ConfigureAwait(false);
            Assert.AreEqual(0, dockerRunResult.ExitCode, $"Dynamo DB: {dynamoDbContainerName} could not be started. Please check if you have docker installed");
        }

        internal async Task StopAsync(string dynamoDbContainerName)
        {
            await dotNet.RunAsync("docker", $"stop {dynamoDbContainerName}").ConfigureAwait(false);
            await dotNet.RunAsync("docker", $"rm {dynamoDbContainerName}").ConfigureAwait(false);
        }
    }
}
