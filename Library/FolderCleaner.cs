namespace LLibrary
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    internal sealed class FolderCleaner : IDisposable
    {
        private readonly string _directory;

        private readonly OpenStreams _openStreams;

        private readonly TimeSpan _threshold;

        private readonly object _cleanLock;

        private readonly Timer _timer;

        internal FolderCleaner(string path, OpenStreams streams, TimeSpan threshold, TimeSpan interval)
        {
            _directory = path;
            _openStreams = streams;
            _threshold = threshold;
            _cleanLock = new object();
            _timer = new Timer(Clean, null, TimeSpan.Zero, interval);
        }

        public void Dispose()
        {
            lock (_cleanLock)
            {
                _timer.Dispose();
            }
        }

        private void Clean(object ignored)
        {
            lock (_cleanLock)
            {
                if (Directory.Exists(_directory))
                {
                    var now = DateTime.Now;
                    var openFiles = _openStreams.Filepaths();
                    var files = Directory.GetFiles(_directory).Except(openFiles);

                    foreach (var filepath in files)
                    {
                        var file = new FileInfo(filepath);
                        var lifetime = now - file.CreationTime;

                        if (lifetime >= _threshold)
                            file.Delete();
                    }
                }
            }
        }
    }
}
