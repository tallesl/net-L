namespace LogThis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Handles file writing.
    /// </summary>
    internal class FileWriter
    {
        /// <summary>
        /// Path of the directory of the log files.
        /// </summary>
        private readonly string _directory;

        /// <summary>
        /// Open streams (values) and their respective dates (keys).
        /// </summary>
        private readonly Dictionary<DateTime, FileStream> _streams;

        /// <summary>
        /// Stream collection lock.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// Register an event handler to close the opened streams when the process exits.
        /// <param name="directory">Path of the directory of the log files.</param>
        /// </summary>
        public FileWriter(string directory)
        {
            _directory = directory;
            _streams = new Dictionary<DateTime, FileStream>();
            _lock = new object();

            // Closing open streams when the application exits
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => CloseAllStreams();

            // Periodically attempting to clean up yesterday's stream
            new Timer(e =>
            {
                ClosePastStreams();
            }, null, 0, (long)TimeSpan.FromHours(2).TotalMilliseconds);
        }

        /// <summary>
        /// Asynchronously rites the passed content to the correspondent file.
        /// The file used is determined by the content's date.
        /// </summary>
        /// <param name="date">Content's date</param>
        /// <param name="content">Content to be written</param>
        public void Write(DateTime date, string content)
        {
            lock (_lock)
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                var stream = GetStream(date.Date);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }

        /// <summary>
        /// Closes past (older than today) open streams.
        /// </summary>
        private void ClosePastStreams()
        {
            lock (_lock)
            {
                var today = DateTime.Today;
                foreach (var stream in _streams) if (stream.Key < today) stream.Value.Dispose();
            }
        }

        /// <summary>
        /// Closes all open streams.
        /// </summary>
        private void CloseAllStreams()
        {
            lock (_lock)
            {
                foreach (var stream in _streams.Values) stream.Dispose();
                _streams.Clear();
            }
        }

        /// <summary>
        /// Gets an open stream.
        /// This method DOESN'T lock the stream collection (make sure the caller does).
        /// </summary>
        /// <param name="date">Stream's date</param>
        /// <returns>An open stream</returns>
        private FileStream GetStream(DateTime date)
        {
            // Opening the stream if needed
            if (!_streams.ContainsKey(date))
            {
                // Building stream's filepath
                var filename = date.ToString("yyyy-MM-dd") + ".log";
                var filepath = Path.Combine(_directory, filename);

                // Making sure the directory exists
                Directory.CreateDirectory(_directory);

                // Opening the stream
                _streams[date] = File.Open(filepath, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            return _streams[date];
        }
    }
}
