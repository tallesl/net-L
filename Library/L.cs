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
