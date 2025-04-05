namespace ContentAggregator.API.Services
{
    public static class ExtensionMethodsAPI
    {
        public static bool IsDevOrQA(this IHostEnvironment environment)
        {
            // Check if the environment name is either "Development" or "QA"
            return environment.IsEnvironment("Development") || environment.IsEnvironment("QA");
        }
    }
}
