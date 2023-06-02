namespace CSE.MRTK.Toolkit.DebugConsole
{
    /// <summary>
    /// Settings for debug console.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the indication whether the UI is shown at startup.
        /// </summary>
        public bool ShowAtStartup { get; set; }

        /// <summary>
        /// Gets or sets the indication whether the log is also saved to file.
        /// </summary>
        public bool SaveToFile { get; set; }
    }
}
