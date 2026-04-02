using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SMARTrackMobile.Services;

public class BlobUploadService
{
    private readonly SettingsService _settings;

    public BlobUploadService(SettingsService settings)
    {
        _settings = settings;
    }

    public async Task<(bool Ok, string Message, Uri? BlobUri)> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var s = _settings.Load();

        if (string.IsNullOrWhiteSpace(s.AzureBlobConnectionString))
            return (false, "AzureBlobConnectionString is not configured.", null);

        if (string.IsNullOrWhiteSpace(s.AzureBlobContainerName))
            return (false, "AzureBlobContainerName is not configured.", null);

        try
        {
            var container = new BlobContainerClient(s.AzureBlobConnectionString, s.AzureBlobContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

            var blobName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}_{fileName}";
            var blob = container.GetBlobClient(blobName);

            await blob.UploadAsync(
                content,
                new BlobHttpHeaders { ContentType = contentType },
                cancellationToken: ct);

            return (true, "Uploaded successfully.", blob.Uri);
        }
        catch (Exception ex)
        {
            return (false, $"Upload failed: {ex.Message}", null);
        }
    }
}
