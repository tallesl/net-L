namespace LLibrary
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;

    internal sealed class FolderCleaner : IDisposable
    {
        private readonly string _path;

        private readonly TimeSpan _threshold;

        private readonly TimeSpan _interval;

        private readonly object _cleanLock;

        private readonly Timer _timer;

        internal FolderCleaner(string path, TimeSpan threshold, TimeSpan interval)
        {
            _path = path;
            _threshold = threshold;
            _interval = interval;
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
                var now = DateTime.Now;
                if (Directory.Exists(_path))
                {
                    foreach (var filepath in Directory.GetFiles(_path))
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
