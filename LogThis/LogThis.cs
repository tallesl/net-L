using System;
using System.Collections.Generic;

namespace LogThis
{
    /// <summary>
    /// Formats and logs the given information.
    /// Use Log.Register() to register a new format and Log.This() to log something.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Handles file writing.
        /// </summary>
        private static FileHandler _fileHandler;

        /// <summary>
        /// Registered formats.
        /// </summary>
        private static Dictionary<string, string> _formats;

        /// <summary>
        /// Format collection lock.
        /// </summary>
        private static readonly object _formatsLock;

        /// <summary>
        /// The longest registered format name length.
        /// Used for calculating spacing when putting the format's name in the log.
        /// </summary>
        private static int _longestFormatName;

        /// <summary>
        /// Ctor.
        /// </summary>
        static Log()
        {
            _fileHandler = new FileHandler();
            _formats = new Dictionary<string, string>();
            _formatsLock = new object();
            _longestFormatName = 0;
        }

        /// <summary>
        /// Formats the given information and logs it.
        /// If the format doesn't exists it does nothing.
        /// </summary>
        /// <param name="formatName">Name of the registered format to use</param>
        /// <param name="args">Arguments used when formating</param>
        /// <returns>True if the format exists and the logging was made, false otherwise.</returns>
        public static bool This(string formatName, params object[] args)
        {
            lock (_formatsLock)
            {
                if (_formats.ContainsKey(formatName))
                {
                    var now = DateTime.Now;
                    var format = _formats[formatName];
                    var content = Format(now, formatName, format, args);
                    _fileHandler.Write(now, content);
                    return true;
                }
                else return false;
            }
        }

        /// <summary>
        /// Register a new log format.
        /// The given format is used with string.Format; for further format info refer to it's documentation.
        /// </summary>
        /// <param name="formatName">Format's name</param>
        /// <param name="format">The format (optional)</param>
        public static void Register(string formatName, string format = "")
        {
            format = format ?? "";
            lock (_formatsLock)
            {
                _formats[formatName] = format;
                _longestFormatName = Math.Max(_longestFormatName, formatName.Length);
            }
        }

        /// <summary>
        /// Unregisters the given format.
        /// </summary>
        /// <param name="formatName">Format's name</param>
        /// <returns>True if the format was found and unregistered, false otherwise.</returns>
        public static bool Unregister(string formatName)
        {
            lock (_formatsLock)
            {
                return _formats.Remove(formatName);
            }
        }

        /// <summary>
        /// Unregister all formats.
        /// </summary>
        public static void UnregisterAll()
        {
            lock (_formatsLock)
            {
                _formats.Clear();
            }
        }

        /// <summary>
        /// Formats the given arguments and returns it.
        /// </summary>
        /// <param name="date">The date to be included</param>
        /// <param name="label">Format's name</param>
        /// <param name="format">Format</param>
        /// <param name="args">Format's arguments</param>
        /// <returns>The formatted output</returns>
        private static string Format(DateTime date, string label, string format, params object[] args)
        {
            // date
            var formattedDate = date.ToString("yyyy-MM-dd HH:mm:ss");

            // format's name
            var spacedLabel = label + new string(' ', _longestFormatName - label.Length);

            // the actual content
            var content = string.IsNullOrWhiteSpace(format) ?
                string.Join(" ", args) :
                string.Format(format, args);

            // line ending
            var eol = Environment.NewLine;

            return string.Format("{0} {1} {2} {3}", formattedDate, spacedLabel, content, eol);
        }
    }
}
