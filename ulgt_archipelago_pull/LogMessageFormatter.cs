using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace ulgtArchipelagoPull
{


    public sealed class LogMessageFormatter : ConsoleFormatter
    {
        public LogMessageFormatter() : base("messageOnly") { }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            textWriter.WriteLine(message);

            if (logEntry.Exception != null)
            {
                textWriter.WriteLine(logEntry.Exception);
            }
        }
    }
}

