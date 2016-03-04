namespace LLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal sealed class FileWriter : IDisposable
    {
        private readonly string _directory;

        private readonly Dictionary<DateTime, FileStream> _streams;

        private readonly Timer _timer;

        private readonly object _lock;

        internal FileWriter(string directory)
        {
            _directory = directory;
            _streams = new Dictionary<DateTime, FileStream>();
            _lock = new object();
            _timer = new Timer(ClosePastStreams, null, 0, (long)TimeSpan.FromHours(2).TotalMilliseconds);
        }

        public void Dispose()
        {
            _timer.Dispose();
            CloseAllStreams();
        }

        internal void Append(DateTime date, string content)
        {
            lock (_lock)
            {
                var stream = GetStream(date.Date);
                var bytes = Encoding.UTF8.GetBytes(content + Environment.NewLine);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }

        private void ClosePastStreams(object ignored)
        {
            lock (_lock)
            {
                var today = DateTime.Today;
                foreach (var stream in _streams)
                {
                    if (stream.Key < today)
                        stream.Value.Dispose();
                }
            }
        }

        private void CloseAllStreams()
        {
            lock (_lock)
            {
                foreach (var stream in _streams.Values)
                    stream.Dispose();

                _streams.Clear();
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "It's disposed on this class Dispose.")]
        private FileStream GetStream(DateTime date)
        {
            // Opening the stream if needed
            if (!_streams.ContainsKey(date))
            {
                // Building stream's filepath
                var filename = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log";
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
