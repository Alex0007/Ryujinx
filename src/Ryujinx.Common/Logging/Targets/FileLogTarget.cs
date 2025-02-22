using Ryujinx.Common.Logging.Formatters;
using System;
using System.IO;
using System.Linq;

namespace Ryujinx.Common.Logging.Targets
{
    public class FileLogTarget : ILogTarget
    {
        private readonly StreamWriter _logWriter;
        private readonly ILogFormatter _formatter;
        private readonly string _name;

        string ILogTarget.Name { get => _name; }

        public FileLogTarget(string path, string name)
            : this(path, name, FileShare.Read, FileMode.Append)
        { }

        public FileLogTarget(string path, string name, FileShare fileShare, FileMode fileMode)
        {
            // Ensure directory is present
            DirectoryInfo logDir = new(Path.Combine(path, "Logs"));
            logDir.Create();

            // Clean up old logs, should only keep 3
            FileInfo[] files = logDir.GetFiles("*.log").OrderBy((info => info.CreationTime)).ToArray();
            for (int i = 0; i < files.Length - 2; i++)
            {
                files[i].Delete();
            }

            string version = ReleaseInformation.GetVersion();

            // Get path for the current time
            path = Path.Combine(logDir.FullName, $"Ryujinx_{version}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

            _name = name;
            _logWriter = new StreamWriter(File.Open(path, fileMode, FileAccess.Write, fileShare));
            _formatter = new DefaultLogFormatter();
        }

        public void Log(object sender, LogEventArgs args)
        {
            _logWriter.WriteLine(_formatter.Format(args));
            _logWriter.Flush();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _logWriter.WriteLine("---- End of Log ----");
            _logWriter.Flush();
            _logWriter.Dispose();
        }
    }
}
