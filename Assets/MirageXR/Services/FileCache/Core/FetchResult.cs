namespace MirageXR.FileCache
{
    /// <summary>Outcome of a download by an <see cref="IRemoteFileFetcher"/>.</summary>
    public enum FetchStatus
    {
        /// <summary>The file was downloaded to the destination path.</summary>
        Ok,

        /// <summary>The server confirmed that the cached version is still current (HTTP 304).</summary>
        NotModified,

        /// <summary>Network or server error; the destination file is not usable.</summary>
        Failed,
    }

    /// <summary>Result of <see cref="IRemoteFileFetcher.FetchToFileAsync"/>; create it with the factory methods.</summary>
    public sealed class FetchResult
    {
        private FetchResult(FetchStatus status, string eTag, string lastModified, string error)
        {
            Status = status;
            ETag = eTag;
            LastModified = lastModified;
            Error = error;
        }

        /// <summary>Whether the file was downloaded, confirmed as unchanged or could not be fetched.</summary>
        public FetchStatus Status { get; }

        /// <summary>ETag header of the response, or <c>null</c> if the server sent none.</summary>
        public string ETag { get; }

        /// <summary>Last-Modified header of the response, or <c>null</c> if the server sent none.</summary>
        public string LastModified { get; }

        /// <summary>Description of the problem if <see cref="Status"/> is <see cref="FetchStatus.Failed"/>, otherwise <c>null</c>.</summary>
        public string Error { get; }

        /// <summary>The file was downloaded completely to <see cref="FetchRequest.DestinationPath"/>.</summary>
        public static FetchResult Ok(string eTag = null, string lastModified = null)
        {
            return new FetchResult(FetchStatus.Ok, eTag, lastModified, null);
        }

        /// <summary>The server confirmed that the cached version is still current; nothing was downloaded.</summary>
        public static FetchResult NotModified(string eTag = null, string lastModified = null)
        {
            return new FetchResult(FetchStatus.NotModified, eTag, lastModified, null);
        }

        /// <summary>Network or server error; any file at the destination path must not be used.</summary>
        public static FetchResult Failed(string error)
        {
            return new FetchResult(FetchStatus.Failed, null, null, error);
        }
    }
}
