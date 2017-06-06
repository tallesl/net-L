namespace LLibrary
{
    using FreshLibrary;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Formats and logs the given information.
    /// Use Register() to register a new format and Log() to log something.
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

        private LineFormatter _formatter;

        public L()
        {
            _directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            _writer = new FileWriter(_directory);
            _cleaner = null;
            _lock = new object();
            _formatter = new LineFormatter();
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
        /// If the format doesn't exists it does nothing.
        /// </summary>
        /// <param name="name">Name of the registered format to use</param>
        /// <param name="args">Arguments used when formating</param>
        /// <returns>True if the format exists and the logging was made, false otherwise.</returns>
        public bool Log(string name, params object[] args)
        {
            var now = DateTime.Now;

            var line = _formatter.Format(now, name, args);

            if (line == null)
                return false;

            _writer.Append(now, line);

            return true;
        }

        /// <summary>
        /// Register a new log format.
        /// The given format is used with string.Format, for further format info refer to it's documentation.
        /// </summary>
        /// <param name="name">Format's name</param>
        public void Register(string name)
        {
            Register(name, string.Empty);
        }

        /// <summary>
        /// Register a new log format.
        /// The given format is used with string.Format, for further format info refer to it's documentation.
        /// </summary>
        /// <param name="name">Format's name</param>
        /// <param name="format">The format (optional)</param>
        public void Register(string name, string format)
        {
            _formatter.Register(name, format);
        }

        /// <summary>
        /// Unregisters the given format.
        /// </summary>
        /// <param name="name">Format's name</param>
        /// <returns>True if the format was found and unregistered, false otherwise.</returns>
        public bool Unregister(string name)
        {
            return _formatter.Unregister(name);
        }

        /// <summary>
        /// Unregister all formats.
        /// </summary>
        public void UnregisterAll()
        {
            _formatter.UnregisterAll();
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
