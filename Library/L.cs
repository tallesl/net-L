namespace LLibrary
{
    using FreshLibrary;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Logs the given information.
    /// </summary>
    public sealed class L : IDisposable
    {
        private static readonly object _staticLock = new object();

        private static L _static = null;

        /// <summary>
        /// A static instance for you to use in case you don't want to instantiate and hold a reference yourself.
        /// It hooks its Dispose() method to ProcessExit or DomainUnload, so you don't have to manually dispose it.
        /// </summary>
        public static L Static
        {
            get
            {
                lock (_staticLock)
                {
                    if (_static == null)
                    {
                        _static = new L();

                        // http://stackoverflow.com/q/16673332
                        if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                            AppDomain.CurrentDomain.ProcessExit += (sender, e) => _static.Dispose();
                        else
                            AppDomain.CurrentDomain.DomainUnload += (sender, e) => _static.Dispose();
                    }

                    return _static;
                }
            }
        }

        private string _directory;

        private FileWriter _writer;

        private FreshFolder _cleaner;

        private object _lock;

        private int _longestLabel;

        public L()
        {
            _directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            _writer = new FileWriter(_directory);
            _cleaner = null;
            _lock = new object();
            _longestLabel = 5;
        }

        /// <summary>
        /// Sets it to delete any file in its folder that is older than 10 days.
        /// </summary>
        public void CleanItself()
        {
            lock (_lock)
            {
                _cleaner = _cleaner ?? new FreshFolder(_directory, TimeSpan.FromDays(10), TimeSpan.FromHours(8),
                    FileTimestamp.Creation);
            }
        }

        /// <summary>
        /// Formats the given information and logs it.
        /// </summary>
        /// <param name="label">Label to use when logging</param>
        /// <param name="message">Message to logs</param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The called function validates it.")]
        public void Log(Enum label, string message, params object[] args)
        {
            Log(label.ToString(), message, args);
        }

        /// <summary>
        /// Formats the given information and logs it.
        /// </summary>
        /// <param name="label">Label to use when logging</param>
        /// <param name="message">Message to log</param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void Log(string label, string message, params object[] args)
        {
            if (label == null)
                throw new ArgumentNullException("label");

            if (message == null)
                throw new ArgumentNullException("message");

            var now = DateTime.Now;
            var formattedDate = now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            _longestLabel = Math.Max(_longestLabel, label.Length);
            label = label.Trim().ToUpperInvariant() + new string(' ', _longestLabel - label.Length);

            var content = string.Format(CultureInfo.InvariantCulture, message, args);
            var line = string.Join(" ", formattedDate, label, content);

            _writer.Append(now, line);
        }

        /// <summary>
        /// Formats the given information and logs it with DEBUG label.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogDebug(string message, params object[] args)
        {
            Log("DEBUG", message, args);
        }

        /// <summary>
        /// Formats the given information and logs it with INFO label.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogInfo(string message, params object[] args)
        {
            Log("INFO", message, args);
        }

        /// <summary>
        /// Formats the given information and logs it with WARN label.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogWarn(string message, params object[] args)
        {
            Log("WARN", message, args);
        }

        /// <summary>
        /// Formats the given information and logs it with ERROR label.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogError(string message, params object[] args)
        {
            Log("ERROR", message, args);
        }

        /// <summary>
        /// Formats the given information and logs it with FATAL label.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogFatal(string message, params object[] args)
        {
            Log("FATAL", message, args);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_writer != null)
                    _writer.Dispose();

                if (_cleaner != null)
                    _cleaner.Dispose();
            }
        }
    }
}
