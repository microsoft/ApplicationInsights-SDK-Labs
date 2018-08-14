namespace Microsoft.LocalForwarder.Library
{
    using Microsoft.ApplicationInsights;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Hosts Local Forwarder libraries
    /// </summary>
    public class Host
    {
        private readonly object sync = new object();

        private Library library;

        private bool isRunning;

        private readonly TelemetryClient telemetryClient = null;

        public Host()
        {
        }

        /// <summary>
        /// For unit tests only.
        /// </summary>
        internal Host(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }
        
        /// <summary>
        /// Starts the library in a resilient mode.
        /// </summary>
        /// <param name="configuration">Configuration data.</param>
        /// <param name="checkInterval">The period after which to check if the library needs to be restarted.</param>
        /// <remarks>This call will trigger a background thread that will indefinitely try to restart the library in case of a failure.</remarks>
        public void Run(string configuration, TimeSpan checkInterval)
        {
            Task.Run(() =>
            {
                this.isRunning = true;
                this.library = null;
                
                while (true)
                {
                    try
                    {
                        if (this.library?.IsRunning != true && this.isRunning)
                        {
                            // the library is not running, start a new one
                            lock (this.sync)
                            {
                                if (this.library?.IsRunning != true && this.isRunning)
                                {
                                    this.library = Host.StartNewLibrary(configuration, this.telemetryClient);
                                    this.isRunning = true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Common.Diagnostics.LogError(FormattableString.Invariant($"The library has failed to start. We'll restart it in a while. {e.ToString()}"));
                    }

                    Thread.Sleep(checkInterval);
                }
            });
        }

        /// <summary>
        /// Stops the background thread in which the library is running
        /// </summary>
        public void Stop()
        {
            lock(this.sync)
            {
                Host.StopLibrary(this.library);

                this.library = null;
                this.isRunning = false;
            }
        }

        private static Library StartNewLibrary(string configuration, TelemetryClient telemetryClient)
        {
            try
            {
                Common.Diagnostics.LogInfo("Starting the library...");

                Library library = telemetryClient != null ? new Library(configuration, telemetryClient) : new Library(configuration);

                library.Run();

                Common.Diagnostics.LogInfo("The library is running");

                return library;
            }
            catch (Exception e)
            {
                Common.Diagnostics.LogError(FormattableString.Invariant($"Unexpected error at start-up of the library. {e.ToString()}"));
                throw;
            }
        }

        private static void StopLibrary(Library library)
        {
            if(library == null)
            {
                throw new InvalidOperationException(FormattableString.Invariant($"The library is not running yet, try stopping it again in a few moments"));
            }

            try
            {
                library.Stop();

                Common.Diagnostics.LogInfo("The library is stopped");
            }
            catch (Exception e)
            {
                // swallow
                Common.Diagnostics.LogError(FormattableString.Invariant($"Unexpected error while stopping the library. {e.ToString()}"));
            }
        }
    }
}