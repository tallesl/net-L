namespace LLibrary
{
    using System;

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
