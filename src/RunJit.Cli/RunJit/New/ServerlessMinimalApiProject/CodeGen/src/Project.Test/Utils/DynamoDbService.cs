using Extensions.Pack;

namespace $ProjectName$.Test.Utils
{
    internal sealed class DynamoDbService(DockerService dockerService,
                                   MigrationScriptExecutor migrationScriptExecutor)
    {
        internal async Task SetupAsync(string dynamoDbContainerName)
        {
            // 1. When we are in debug mode (DEV local) we start the needed docker container
            //    and we have to shut down and remove it ! to have a clean container next startup
            if (typeof(ApiTestBase).Assembly.IsCompiledInDebug())
            {
                // 2. Startup docker container
                await dockerService.RunAsync(dynamoDbContainerName).ConfigureAwait(false);

                // 3. Run migration scripts
                await migrationScriptExecutor.RunMigrationScriptsAsync(typeof(Program).Assembly).ConfigureAwait(false);
            }
        }

        internal async Task TearDown(string dynamoDbContainerName)
        {
            // 3. When we are in debug mode (DEV local) we start the needed docker container
            //    and we have to shut down and remove it ! to have a clean container next startup
            if (typeof(ApiTestBase).Assembly.IsCompiledInDebug())
            {
                // 2. Startup docker container
                await dockerService.StopAsync(dynamoDbContainerName).ConfigureAwait(false);
            }
        }
    }
}
