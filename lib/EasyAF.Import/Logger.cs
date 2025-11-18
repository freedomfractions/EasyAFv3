using System;
using System.IO;
using System.Text;

namespace EasyAF.Import
{
    public enum LogMode { Standard, Verbose }

    public interface ILogger
    {
        void Error(string category, string message, object? data = null);
        void Info(string category, string message, object? data = null);
        void Verbose(string category, string message, object? data = null);
        bool IsVerbose { get; }
    }

    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly LogMode _mode;
        private readonly object _lock = new();
        public bool IsVerbose => _mode == LogMode.Verbose;

        public FileLogger(string logFilePath, LogMode mode = LogMode.Standard)
        {
            _logFilePath = logFilePath; _mode = mode;
        }

        public void Error(string category, string message, object? data = null)
        {
            Write("ERROR", category, message, data, always: true);
        }
        public void Info(string category, string message, object? data = null)
        {
            Write("INFO", category, message, data, always: true);
        }
        public void Verbose(string category, string message, object? data = null)
        {
            if (IsVerbose)
            {
                Write("VERBOSE", category, message, data, always: false);
            }
        }

        private void Write(string level, string category, string message, object? data, bool always)
        {
            var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var line = new StringBuilder().Append('[').Append(ts).Append("] [").Append(level).Append("] [").Append(category).Append("] ").Append(message);
            if (data != null)
            {
                line.Append(" | Data: ").Append(data);
            }
            var text = line.ToString();
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, text + Environment.NewLine);
            }
            if (level == "ERROR" || always)
            {
                Console.WriteLine(text);
            }
        }
    }
}
