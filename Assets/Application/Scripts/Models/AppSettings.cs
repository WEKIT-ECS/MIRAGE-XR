using System;

/// <summary>
/// Model class for application settings.
/// </summary>
[Serializable]
public class AppSettings
{
    /// <summary>
    /// Gets or sets the client ID of the app registration for the HoloLens App.
    /// </summary>
    public string ClientId;

    /// <summary>
    /// Gets or sets the tenant ID of the app registration for the HoloLens App.
    /// </summary>
    public string TenantId;

    /// <summary>
    /// Gets or sets the scope(s) of the backend API to access.
    /// Mostly something like "api://[client id of backend API]/user_impersonation.
    /// </summary>
    public string Scopes;

    /// <summary>
    /// Gets or sets the resource of the backend API to access.
    /// Mostly something like "api://[client id of backend API].
    /// </summary>
    public string Resource;

    /// <summary>
    /// Gets or sets the base endpoint URL for the backend to access.
    /// </summary>
    public string BaseEndPointUrl;

    /// <summary>
    /// Return if the app settings are valid for use in the application.
    /// We just check if the settings are not empty. Errors can occur
    /// later if the settings are not correct (wrong Client ID for instance).
    /// </summary>
    /// <returns>Valid true or false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ClientId) &&
            !string.IsNullOrEmpty(TenantId) &&
            !string.IsNullOrEmpty(Scopes) &&
            !string.IsNullOrEmpty(Resource) &&
            !string.IsNullOrEmpty(BaseEndPointUrl);
    }
}
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CA1051 // Do not declare visible instance fields
