namespace LLibrary
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Optional configurations that can be used when initializing the logger.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct LConfiguration
    {
        /// <summary>
        /// True to use UTC time rather than local time.
        /// Defaults to false.
        /// </summary>
        public bool UseUtcTime { get; set; }

        /// <summary>
        /// If other than null it sets to delete any file in the log folder that is older than the specified time.
        /// Defaults to null.
        /// </summary>
        public TimeSpan? DeleteOldFiles { get; set; }

        /// <summary>
        /// Format string to use when calling DateTime.Format.
        /// Defaults to "yyyy-MM-dd HH:mm:ss".
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Directory where to create the log files.
        /// Defautls to a local "logs" directory.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Labels enabled to be logged by the library.
        /// An attempt to log with a label that is not enabled is simply ignored, no error is raised.
        /// Leave it empty or null to enable any label, which is the default.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] EnabledLabels { get; set; }
    }
}
