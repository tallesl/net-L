namespace LLibrary
{
    using FreshLibrary;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Logs in a log file the given information.
    /// </summary>
    public sealed class L : IDisposable
    {
        private static readonly object _staticLock = new object();

        private static L _static = null;

        private static ObjectDisposedException ObjectDisposedException
        {
            get
            {
                return new ObjectDisposedException("Cannot access a disposed object.");
            }
        }

        /// <summary>
        /// A static instance for you to use in case you don't want to instantiate and hold a reference yourself.
        /// It hooks its Dispose() method to ProcessExit or DomainUnload, so you don't have to manually dispose it.
        /// You must call InitializeStatic before using this instance.
        /// </summary>
        public static L Static
        {
            get
            {
                lock (_staticLock)
                {
                    if (_static == null)
                        throw new InvalidOperationException("The static instance has not been initialized.");

                    return _static;
                }
            }
        }

        /// <summary>
        /// Initializes the static instance using the default configuration.
        /// </summary>
        public static void InitializeStatic()
        {
            InitializeStatic(new LConfiguration());
        }

        /// <summary>
        /// Initializes the static instance using the given configuration.
        /// </summary>
        /// <param name="configuration">Configuration to use</param>
        public static void InitializeStatic(LConfiguration configuration)
        {
            lock (_staticLock)
            {
                if (_static != null && !_static._disposed)
                    throw new InvalidOperationException("The static instance is already initialized.");

                _static = new L(configuration);

                // http://stackoverflow.com/q/16673332
                if (AppDomain.CurrentDomain.IsDefaultAppDomain())
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) => _static.Dispose();
                else
                    AppDomain.CurrentDomain.DomainUnload += (sender, e) => _static.Dispose();
            }
        }

        private LConfiguration _configuration;

        private string _directory;

        private FileWriter _writer;

        private FreshFolder _cleaner;

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

            _directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            _writer = new FileWriter(_directory);

            if (_configuration.DeleteOldFiles.HasValue)
            {
                var min = TimeSpan.FromSeconds(5);
                var max = TimeSpan.FromHours(8);

                var cleanUpTime = new TimeSpan(_configuration.DeleteOldFiles.Value.Ticks / 5);

                if (cleanUpTime < min)
                    cleanUpTime = min;

                if (cleanUpTime > max)
                    cleanUpTime = max;

                _cleaner = _cleaner ?? new FreshFolder(_directory, _configuration.DeleteOldFiles.Value, cleanUpTime,
                    FileTimestamp.Creation);
            }

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
        /// Formats the given information and logs it.
        /// </summary>
        /// <param name="label">Label to use when logging</param>
        /// <param name="content">A string with a message or an object to call ToString() on it/param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The called function validates it.")]
        public void Log(Enum label, string content, params object[] args)
        {
            Log(label.ToString(), content, args);
        }

        /// <summary>
        /// Formats the given information and logs it.
        /// </summary>
        /// <param name="label">Label to use when logging</param>
        /// <param name="content">A string with a message or an object to call ToString() on it/param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void Log(string label, object content, params object[] args)
        {

            if (label == null)
                throw new ArgumentNullException("label");

            if (content == null)
                throw new ArgumentNullException("content");

            var date = Now;
            var formattedDate = date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            _longestLabel = Math.Max(_longestLabel, label.Length);
            label = label.Trim().ToUpperInvariant() + new string(' ', _longestLabel - label.Length);

            var formattedContent = string.Format(CultureInfo.InvariantCulture, content.ToString(), args);
            var line = string.Join(" ", formattedDate, label, formattedContent);

            lock (_lock)
            {
                if (_disposed)
                    throw ObjectDisposedException;

                _writer.Append(date, line);
            }
        }

        /// <summary>
        /// Formats the given information and logs it with DEBUG label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it/param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogDebug(object content, params object[] args)
        {
            Log("DEBUG", content, args);
        }

        /// <summary>
        /// Formats the given information and logs it with INFO label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it/param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogInfo(object content, params object[] args)
        {
            Log("INFO", content, args);
        }

        /// <summary>
        /// Formats the given information and logs it with WARN label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it/param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogWarn(object content, params object[] args)
        {
            Log("WARN", content, args);
        }

        /// <summary>
        /// Formats the given information and logs it with ERROR label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it/param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogError(object content, params object[] args)
        {
            Log("ERROR", content, args);
        }

        /// <summary>
        /// Formats the given information and logs it with FATAL label.
        /// </summary>
        /// <param name="content">A string with a message or an object to call ToString() on it/param>
        /// <param name="args">Arguments to use along string.Format on the given message</param>
        public void LogFatal(object content, params object[] args)
        {
            Log("FATAL", content, args);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                if (_writer != null)
                    _writer.Dispose();

                if (_cleaner != null)
                    _cleaner.Dispose();

                _disposed = true;
            }
        }
    }
}
