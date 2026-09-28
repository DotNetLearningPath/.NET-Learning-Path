using Microsoft.Extensions.Logging;

namespace InsuranceApp.Api.Logging;

public sealed class DailyFileLoggerProvider : ILoggerProvider
{
    private readonly string _logDirectory;
    private readonly object _syncRoot = new();

    public DailyFileLoggerProvider(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
    }

    public ILogger CreateLogger(string categoryName) =>
        new DailyFileLogger(categoryName, _logDirectory, _syncRoot);

    public void Dispose()
    {
    }

    private sealed class DailyFileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logDirectory;
        private readonly object _syncRoot;

        public DailyFileLogger(
            string categoryName,
            string logDirectory,
            object syncRoot)
        {
            _categoryName = categoryName;
            _logDirectory = logDirectory;
            _syncRoot = syncRoot;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var now = DateTime.Now;
            var fileName = $"{now:yyyy}_{now:MM}_{now:dd}.log";
            var filePath = Path.Combine(_logDirectory, fileName);
            var message = formatter(state, exception);
            var exceptionText = exception is null
                ? string.Empty
                : Environment.NewLine + exception;

            var line =
                $"{now:yyyy-MM-dd HH:mm:ss.fff zzz} [{logLevel}] " +
                $"[{_categoryName}] {message}{exceptionText}{Environment.NewLine}";

            try
            {
                lock (_syncRoot)
                {
                    Directory.CreateDirectory(_logDirectory);
                    File.AppendAllText(filePath, line);
                }
            }
            catch
            {
                // Logging must never interrupt the request being processed.
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
