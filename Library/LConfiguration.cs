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
        /// </summary>
        public bool UseUtcTime { get; set; }

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
            return a.UseUtcTime == b.UseUtcTime;
        }

        public static bool operator !=(LConfiguration a, LConfiguration b)
        {
            return !(a == b);
        }
    }
}
