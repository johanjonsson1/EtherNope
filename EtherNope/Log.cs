using Microsoft.Extensions.Logging;

namespace EtherNope;

public static class Log
{
    public static ILogger CreateEventLogLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddEventLog(settings =>
            {
                settings.SourceName = nameof(EtherNope);
            });
        });

       return loggerFactory.CreateLogger(nameof(EtherNope));
    }
}