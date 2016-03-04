namespace LLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal class FileWriter
    {
        private readonly string _directory;

        private readonly Dictionary<DateTime, FileStream> _streams;

        private readonly object _lock;

        internal FileWriter(string directory)
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

        internal void Append(DateTime date, string content)
        {
            lock (_lock)
            {
                var stream = GetStream(date.Date);
                if (stream.Length > 0) content = (Environment.NewLine + Environment.NewLine + content);
                var bytes = Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }

        private void ClosePastStreams()
        {
            lock (_lock)
            {
                var today = DateTime.Today;
                foreach (var stream in _streams) if (stream.Key < today) stream.Value.Dispose();
            }
        }

        private void CloseAllStreams()
        {
            lock (_lock)
            {
                foreach (var stream in _streams.Values) stream.Dispose();
                _streams.Clear();
            }
        }

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
