namespace MirageXR.FileCache
{
    public enum FetchStatus
    {
        /// <summary>The file was downloaded to the destination path.</summary>
        Ok,

        /// <summary>The server confirmed that the cached version is still current (HTTP 304).</summary>
        NotModified,

        /// <summary>Network or server error; the destination file is not usable.</summary>
        Failed,
    }

    public sealed class FetchResult
    {
        private FetchResult(FetchStatus status, string eTag, string lastModified, string error)
        {
            Status = status;
            ETag = eTag;
            LastModified = lastModified;
            Error = error;
        }

        public FetchStatus Status { get; }

        public string ETag { get; }

        public string LastModified { get; }

        public string Error { get; }

        public static FetchResult Ok(string eTag = null, string lastModified = null)
        {
            return new FetchResult(FetchStatus.Ok, eTag, lastModified, null);
        }

        public static FetchResult NotModified(string eTag = null, string lastModified = null)
        {
            return new FetchResult(FetchStatus.NotModified, eTag, lastModified, null);
        }

        public static FetchResult Failed(string error)
        {
            return new FetchResult(FetchStatus.Failed, null, null, error);
        }
    }
}
