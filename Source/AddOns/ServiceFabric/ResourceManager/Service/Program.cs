﻿namespace ResourceManager.SF
{
    using Microsoft.ServiceFabric.Services.Runtime;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;

    internal class Program
    {
        public static void Main(string[] args)
        {
            ConsoleLogger logger = new ConsoleLogger("RMService", null, false);
            try
            {
                ServiceRuntime.RegisterServiceAsync("PPS.VMManagerType",
                        context =>
                        {
                            return new ResourceManagerService(context, logger);
                        })
                    .GetAwaiter()
                    .GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Exiting application");
                throw;
            }
        }
    }
}
