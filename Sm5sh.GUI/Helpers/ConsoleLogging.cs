using System;
using System.Collections.Concurrent;

namespace Microsoft.Extensions.Logging
{
    public class CustomConsoleLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel _logLevel;
        private readonly ConcurrentDictionary<string, ConsoleLogger> _loggers = new ConcurrentDictionary<string, ConsoleLogger>();

        public CustomConsoleLoggerProvider(LogLevel logLevel = LogLevel.Debug)
        {
            _logLevel = logLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new ConsoleLogger(name, _logLevel));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    public class ConsoleLogger : ILogger
    {
        private readonly LogLevel _logLevel;
        private readonly string _name;

        public ConsoleLogger(string name, LogLevel logLevel = LogLevel.Debug)
        {
            _name = name;
            _logLevel = logLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Console.WriteLine($"{logLevel.ToString()} - {eventId.Id} - {_name} - {formatter(state, exception)}");
        }
    }
}