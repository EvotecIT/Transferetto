namespace Transferetto.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class StorageProviderLiveFactAttribute : FactAttribute {
    /// <summary>Initializes a live provider fact and skips it when emulator endpoints are unavailable.</summary>
    public StorageProviderLiveFactAttribute() {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRANSFERETTO_S3_ENDPOINT")) ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRANSFERETTO_AZURE_CONNECTION_STRING"))) {
            Skip = "Local S3 and Azure Blob emulator endpoints are not configured.";
        }
    }
}
