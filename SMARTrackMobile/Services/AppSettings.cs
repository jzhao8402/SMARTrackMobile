namespace SMARTrackMobile.Services;

public record AppSettings(
    string DeltaSsoUri,
    string Environment,
    bool EnableSounds,
    string AzureBlobConnectionString,
    string AzureBlobContainerName
);
