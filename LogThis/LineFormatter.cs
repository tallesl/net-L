namespace LogThis
{
    using System;
    using System.Collections.Generic;

    internal class LineFormatter
    {
        private Dictionary<string, string> _formats;

        private readonly object _lock;

        private int _longestLabel;

        internal LineFormatter()
        {
            _formats = new Dictionary<string, string>();
            _lock = new object();
            _longestLabel = 0;
        }

        internal void Register(string formatName, string format)
        {
            format = format ?? string.Empty;
            lock (_lock)
            {
                _formats[formatName] = format;
                _longestLabel = Math.Max(_longestLabel, formatName.Length);
            }
        }

        internal bool Unregister(string formatName)
        {
            lock (_lock)
            {
                return _formats.Remove(formatName);
            }
        }

        internal void UnregisterAll()
        {
            lock (_lock)
            {
                _formats.Clear();
            }
        }

        internal string GetFormat(string formatName)
        {
            lock (_lock)
            {
                string format = null;
                _formats.TryGetValue(formatName, out format);
                return format;
            }
        }

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

            return string.Join(" ", formattedDate, label, content);
        }
    }
}
