namespace Shopilent.API.IntegrationTests.Endpoints.Administration.RebuildSearchIndex.V1;

public static class RebuildSearchIndexTestDataV1
{
    // Core valid request generator
    public static object CreateValidRequest(
        bool? initializeIndexes = null,
        bool? indexProducts = null,
        bool? forceReindex = null)
    {
        return new
        {
            InitializeIndexes = initializeIndexes ?? true,
            IndexProducts = indexProducts ?? true,
            ForceReindex = forceReindex ?? false
        };
    }

    // Common scenarios
    public static class CommonScenarios
    {
        public static object CreateDefaultRequest() => new
        {
            InitializeIndexes = true,
            IndexProducts = true,
            ForceReindex = false
        };

        public static object CreateInitializeOnlyRequest() => new
        {
            InitializeIndexes = true,
            IndexProducts = false,
            ForceReindex = false
        };

        public static object CreateIndexProductsOnlyRequest() => new
        {
            InitializeIndexes = false,
            IndexProducts = true,
            ForceReindex = false
        };

        public static object CreateForceReindexRequest() => new
        {
            InitializeIndexes = true,
            IndexProducts = true,
            ForceReindex = true
        };

        public static object CreateMinimalRequest() => new
        {
            InitializeIndexes = false,
            IndexProducts = false,
            ForceReindex = false
        };
    }

    // Edge cases
    public static class EdgeCases
    {
        public static object CreateAllFalseRequest() => new
        {
            InitializeIndexes = false,
            IndexProducts = false,
            ForceReindex = false
        };

        public static object CreateOnlyForceReindexRequest() => new
        {
            InitializeIndexes = false,
            IndexProducts = false,
            ForceReindex = true
        };
    }
}