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
        private readonly bool _useUtcTime;

        private readonly TimeSpan? _deleteOldFiles;

        private readonly string _dateTimeFormat;

        private readonly string _directory;

        private readonly string[] _enabledLabels;

        private readonly object _lock;

        private readonly OpenStreams _openStreams;

        private readonly FolderCleaner _cleaner;

        private int _longestLabel;

        private bool _disposed;

        /// <summary>
        /// Constructs the logger using the given configuration.
        /// </summary>
        /// <param name="useUtcTime">True to use UTC time rather than local time</param>
        /// <param name="deleteOldFiles">
        /// If other than null it sets to delete any file in the log folder that is older than the specified time
        /// </param>
        /// <param name="dateTimeFormat">Format string to use when calling DateTime.Format</param>
        /// <param name="directory">
        /// Directory where to create the log files, null to use a local "logs" directory
        /// </param>
        /// <param name="enabledLabels">
        /// Labels enabled to be logged by the library, an attempt to log with a label that is not enabled is ignored
        /// (no error is raised), null or empty enables all labels
        /// </param>
        public L(
            bool useUtcTime = false, TimeSpan? deleteOldFiles = null, string dateTimeFormat = "yyyy-MM-dd HH:mm:ss",
            string directory = null, params string[] enabledLabels)
        {
            _useUtcTime = useUtcTime;
            _deleteOldFiles = deleteOldFiles;
            _dateTimeFormat = dateTimeFormat;
            _directory = directory ?? Path.Combine(AppContext.BaseDirectory, "logs");
            _enabledLabels = (enabledLabels ?? new string[0]).Select(Normalize).ToArray();
            _lock = new object();
            _openStreams = new OpenStreams(_directory);

            if (_deleteOldFiles.HasValue)
            {
                var min = TimeSpan.FromSeconds(5);
                var max = TimeSpan.FromHours(8);

                var cleanUpTime = new TimeSpan(_deleteOldFiles.Value.Ticks / 5);

                if (cleanUpTime < min)
                    cleanUpTime = min;

                if (cleanUpTime > max)
                    cleanUpTime = max;

                _cleaner = new FolderCleaner(_directory, _openStreams, _deleteOldFiles.Value, cleanUpTime);
            }

            _longestLabel = _enabledLabels.Any() ? _enabledLabels.Select(l => l.Length).Max() : 5;
            _disposed = false;
        }

        private DateTime Now => _useUtcTime ? DateTime.UtcNow : DateTime.Now;

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

            label = Normalize(label);

            if (_enabledLabels.Any() && !_enabledLabels.Contains(label))
                return;

            _longestLabel = Math.Max(_longestLabel, label.Length);

            var date = Now;
            var formattedDate = date.ToString(_dateTimeFormat, CultureInfo.InvariantCulture);
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
        public void Debug(object content) => Log("DEBUG", content);

        /// <summary>
        /// Logs the given information with INFO label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void Info(object content) => Log("INFO", content);

        /// <summary>
        /// Logs the given information with WARN label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void Warn(object content) => Log("WARN", content);

        /// <summary>
        /// Logs the given information with ERROR label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void Error(object content) => Log("ERROR", content);

        /// <summary>
        /// Logs the given information with FATAL label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it</param>
        public void Fatal(object content) => Log("FATAL", content);

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

        private string Normalize(string label) => label.Trim().ToUpperInvariant();
    }
}
