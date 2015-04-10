namespace LogThis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Handles line formatting.
    /// </summary>
    internal class LineFormatter
    {
        /// <summary>
        /// Registered formats.
        /// </summary>
        private Dictionary<string, string> _formats;

        /// <summary>
        /// Format collection lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// The longest registered format name length.
        /// Used for calculating spacing when putting the format's name in the log.
        /// </summary>
        private int _longestLabel;

        /// <summary>
        /// Ctor.
        /// </summary>
        internal LineFormatter()
        {
            _formats = new Dictionary<string, string>();
            _lock = new object();
            _longestLabel = 0;
        }

        /// <summary>
        /// Register a new log format.
        /// The given format is used with string.Format; for further format info refer to it's documentation.
        /// </summary>
        /// <param name="formatName">Format's name</param>
        /// <param name="format">The format</param>
        internal void Register(string formatName, string format)
        {
            format = format ?? string.Empty;
            lock (_lock)
            {
                _formats[formatName] = format;
                _longestLabel = Math.Max(_longestLabel, formatName.Length);
            }
        }

        /// <summary>
        /// Unregisters the given format.
        /// </summary>
        /// <param name="formatName">Format's name</param>
        /// <returns>True if the format was found and unregistered, false otherwise.</returns>
        internal bool Unregister(string formatName)
        {
            lock (_lock)
            {
                return _formats.Remove(formatName);
            }
        }

        /// <summary>
        /// Unregister all formats.
        /// </summary>
        internal void UnregisterAll()
        {
            lock (_lock)
            {
                _formats.Clear();
            }
        }

        /// <summary>
        /// Returns the format of the given format's name.
        /// </summary>
        /// <param name="formatName">Format's name</param>
        /// <returns>The format if any or null</returns>
        internal string GetFormat(string formatName)
        {
            lock (_lock)
            {
                string format = null;
                _formats.TryGetValue(formatName, out format);
                return format;
            }
        }

        /// <summary>
        /// Formats the given arguments and returns it.
        /// </summary>
        /// <param name="date">The date to be included</param>
        /// <param name="formatName">Format's name</param>
        /// <param name="args">Format's arguments</param>
        /// <returns>The formatted output or null if the format wasn't found</returns>
        internal string Format(DateTime date, string formatName, params object[] args)
        {
            // format
            var format = GetFormat(formatName);
            if (format == null) return null;

            // date
            var formattedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // label
            var label = formatName + new string(' ', _longestLabel - formatName.Length);

            // the actual content
            var content = format == string.Empty ? string.Join(" ", args) : string.Format(format, args);

            // line ending
            var eol = Environment.NewLine;

            return string.Format("{0} {1} {2} {3}", formattedDate, label, content, eol);
        }
    }
}
