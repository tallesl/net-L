namespace LLibrary
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Optional configurations that can be used when initializing the logger.
    /// </summary>
    public struct LConfiguration
    {
        /// <summary>
        /// Use UTC time rather than local time.
        /// False by default.
        /// </summary>
        public bool UseUtcTime { get; set; }

        /// <summary>
        /// Sets it to delete any file in the log folder that is older than the specified time.
        /// Disabled by default.
        /// </summary>
        public TimeSpan? DeleteOldFiles { get; set; }

        /// <summary>
        /// Format string to use when calling DateTime.Format.
        /// "yyyy-MM-dd HH:mm:ss" by default.
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Directory where to create the log files.
        /// A local "logs" directory by default.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Labels enabled to be logged by the library.
        /// An attempt to log with a label that is not enabled is simply ignored, no error is raised.
        /// Leave it empty or null to enable any label, which is the default.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "The object it's passed by value (since it's a struct) plus is used just as configuration.")]
        public string[] EnabledLabels { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LConfiguration ? this == (LConfiguration)obj : false;
        }

        public override int GetHashCode()
        {
            return UseUtcTime.GetHashCode();
        }

        public static bool operator ==(LConfiguration a, LConfiguration b)
        {
            return a.UseUtcTime == b.UseUtcTime &&
                a.DeleteOldFiles == b.DeleteOldFiles;
        }

        public static bool operator !=(LConfiguration a, LConfiguration b)
        {
            return !(a == b);
        }
    }
}
