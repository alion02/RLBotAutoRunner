using System;
using System.IO;
using System.Diagnostics;

namespace RLBotAutoRunner
{
    public class FileResource : IUniqueResource, IEquatable<FileResource>
    {
        private FileResource(string fullPath)
        {
            path = fullPath;
        }

        private readonly string path;
        private bool started;
        private Process process;

        public void Start()
        {
            if (!started)
            {
                Console.WriteLine($"Starting unique resource '{path}'...");
                process = Process.Start(new ProcessStartInfo() { FileName = path, WorkingDirectory = Path.GetDirectoryName(path), UseShellExecute = true });
                started = true;
            }
        }
        public void Stop()
        {
            if (started)
            {
                if (!process.HasExited)
                {
                    Console.WriteLine($"Stopping unique resource '{path}'...");
                    process.CloseMainWindow();
                    if (!process.WaitForExit(5000)) // TODO: Close processes asynchronously
                    {
                        Console.WriteLine("Unresponsive, killing...");
                        process.Kill();
                    }
                }
                process.Dispose();
                started = false;
            }
        }

        public override int GetHashCode() => -1757656154 + path.GetHashCode();
        public bool Equals(FileResource other) => other != null && path == other.path;
        public bool Equals(IUniqueResource other) => Equals(other as FileResource);
        public override bool Equals(object obj) => Equals(obj as FileResource);
        public static bool operator ==(FileResource a, FileResource b) => a.Equals(b);
        public static bool operator !=(FileResource a, FileResource b) => !(a == b);

        public static FileResource Create(string fullPath)
        {
            var n = new FileResource(fullPath);
            if (UniqueResource.Global.TryGetValue(n, out var s))
                return (FileResource)s;

            return n;
        }
    }
}
