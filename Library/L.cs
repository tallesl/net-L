namespace LLibrary
{
    using FolderCleaning;
    using System;
    using System.IO;

    /// <summary>
    /// Formats and logs the given information.
    /// Use Log.Register() to register a new format and Log.This() to log something.
    /// </summary>
    public static class L
    {
        /// <summary>
        /// Path of the directory of the log files.
        /// </summary>
        public static readonly string Directory;

        private static FileWriter _writer;

        private static FolderCleaner _cleaner;

        private static object _lock;

        private static LineFormatter _formatter;

        static L()
        {
            Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            _writer = new FileWriter(Directory);
            _cleaner = null;
            _lock = new object();
            _formatter = new LineFormatter();
        }

        /// <summary>
        /// Sets the logger to clean itself (files older than 10 days).
        /// </summary>
        public static bool CleanItself
        {
            get
            {
                lock (_lock)
                {
                    return _cleaner != null;
                }
            }

            set
            {
                lock (_lock)
                {
                    _cleaner = new FolderCleaner(L.Directory, TimeSpan.FromDays(10), TimeSpan.FromHours(8),
                        FileTimestamps.Creation);

                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                        {
                            lock (_lock)
                            {
                                if (_writer != null)
                                    _writer.Dispose();

                                if (_cleaner != null)
                                    _cleaner.Dispose();
                            }
                        };
                }
            }
        }

        /// <summary>
        /// Formats the given information and logs it.
        /// If the format doesn't exists it does nothing.
        /// </summary>
        /// <param name="formatName">Name of the registered format to use</param>
        /// <param name="args">Arguments used when formating</param>
        /// <returns>True if the format exists and the logging was made, false otherwise.</returns>
        public static bool Log(string formatName, params object[] args)
        {
            var now = DateTime.Now;

            var line = _formatter.Format(now, formatName, args);

            if (line == null)
                return false;

            _writer.Append(now, line);

            return true;
        }

        /// <summary>
        /// Register a new log format.
        /// The given format is used with string.Format; for further format info refer to it's documentation.
        /// </summary>
        /// <param name="formatName">Format's name</param>
        /// <param name="format">The format (optional)</param>
        public static void Register(string formatName, string format = "")
        {
            _formatter.Register(formatName, format);
        }

        /// <summary>
        /// Unregisters the given format.
        /// </summary>
        /// <param name="formatName">Format's name</param>
        /// <returns>True if the format was found and unregistered, false otherwise.</returns>
        public static bool Unregister(string formatName)
        {
            return _formatter.Unregister(formatName);
        }

        /// <summary>
        /// Unregister all formats.
        /// </summary>
        public static void UnregisterAll()
        {
            _formatter.UnregisterAll();
        }
    }
}
