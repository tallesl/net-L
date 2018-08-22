namespace LLibrary
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Logs in a log file the given information.
    /// </summary>
    public sealed class L : IDisposable
    {
        private static Func<string, string> _sanitizeLabel = label => label.Trim().ToUpperInvariant();

        private LConfiguration _configuration;

        private OpenStreams _openStreams;

        private FolderCleaner _cleaner;

        private object _lock;

        private int _longestLabel;

        private bool _disposed;

        /// <summary>
        /// Constructs the logger using the default configuration.
        /// </summary>
        public L() : this(new LConfiguration()) { }

        /// <summary>
        /// Constructs the logger using the given configuration.
        /// </summary>
        /// <param name="configuration">Configuration to use</param>
        public L(LConfiguration configuration)
        {
            _configuration = configuration;

            if (string.IsNullOrEmpty(_configuration.Directory))
                _configuration.Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            _openStreams = new OpenStreams(_configuration.Directory);

            if (_configuration.DeleteOldFiles.HasValue)
            {
                var min = TimeSpan.FromSeconds(5);
                var max = TimeSpan.FromHours(8);

                var cleanUpTime = new TimeSpan(_configuration.DeleteOldFiles.Value.Ticks / 5);

                if (cleanUpTime < min)
                    cleanUpTime = min;

                if (cleanUpTime > max)
                    cleanUpTime = max;

                _cleaner =
                    new FolderCleaner(
                        _configuration.Directory, _openStreams, _configuration.DeleteOldFiles.Value, cleanUpTime);
            }

            if (string.IsNullOrEmpty(_configuration.DateTimeFormat))
                _configuration.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            _configuration.EnabledLabels =
                (_configuration.EnabledLabels ?? new string[0]).Select(l => _sanitizeLabel(l)).ToArray();

            _lock = new object();
            _longestLabel = 5;
            _disposed = false;
        }

        private DateTime Now
        {
            get
            {
                return _configuration.UseUtcTime ? DateTime.UtcNow : DateTime.Now;
            }
        }

        /// <summary>
        /// Logs the given information.
        /// </summary>
        /// <param name="label">Label to use when logging</param>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
            Justification = "The called function validates it.")]
        public void Log(Enum label, string content) => Log(label.ToString(), content);

        /// <summary>
        /// Formats the given information and logs it.
        /// </summary>
        /// <param name="label">Label to use when logging</param>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void Log(string label, object content)
        {
            if (label == null)
                throw new ArgumentNullException("label");

            if (content == null)
                throw new ArgumentNullException("content");

            label = _sanitizeLabel(label);

            if (_configuration.EnabledLabels.Any() && !_configuration.EnabledLabels.Contains(label))
                return;

            _longestLabel = Math.Max(_longestLabel, label.Length);

            var date = Now;
            var formattedDate = date.ToString(_configuration.DateTimeFormat, CultureInfo.InvariantCulture);
            var padding = new string(' ', _longestLabel - label.Length);

            var line = $"{formattedDate} {label} {padding}{content}";

            lock (_lock)
            {
                if (_disposed)
                    throw new ObjectDisposedException("Cannot access a disposed object.");

                _openStreams.Append(date, line);
            }
        }

        /// <summary>
        /// Logs the given information with DEBUG label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void LogDebug(object content) => Log("DEBUG", content);

        /// <summary>
        /// Logs the given information with INFO label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void LogInfo(object content) => Log("INFO", content);

        /// <summary>
        /// Logs the given information with WARN label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void LogWarn(object content) => Log("WARN", content);

        /// <summary>
        /// Logs the given information with ERROR label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void LogError(object content) => Log("ERROR", content);

        /// <summary>
        /// Logs the given information with FATAL label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void LogFatal(object content) => Log("FATAL", content);

        /// <summary>
        /// Disposes the file writer and the directory cleaner used by this instance.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                if (_openStreams != null)
                    _openStreams.Dispose();

                if (_cleaner != null)
                    _cleaner.Dispose();

                _disposed = true;
            }
        }
    }
}
