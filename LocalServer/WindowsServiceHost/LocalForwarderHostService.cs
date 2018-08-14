namespace Microsoft.LocalForwarder.WindowsServiceHost
{
    using System;
    using Microsoft.LocalForwarder.Library;
    using System.ServiceProcess;
    using System.IO;
    using System.Threading;

    public partial class LocalForwarderHostService : ServiceBase
    {
        private Host host;

        public LocalForwarderHostService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Unit tests only.
        /// </summary>
        internal void TestStartStop(TimeSpan timeToRun)
        {
            this.OnStart(null);

            Thread.Sleep(timeToRun);

            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Common.Diagnostics.LogInfo("Starting Windows Service...");
            Common.Diagnostics.LogTrace(FormattableString.Invariant($"Current directory is set to {Directory.GetCurrentDirectory()}"));

            try
            {
                Common.Diagnostics.LogInfo("Starting the host...");

                string config = ReadConfiguration();

                this.host = new Host();

                this.host.Run(config, TimeSpan.FromSeconds(5));

                Common.Diagnostics.LogInfo("The host is running");
            }
            catch (Exception e)
            {
                Common.Diagnostics.LogError(FormattableString.Invariant($"Unexpected error while starting the host. {e.ToString()}"));
                throw;
            }
        }

        protected override void OnStop()
        {
            Common.Diagnostics.LogInfo("Stopping Windows Service...");

            try
            {
                Common.Diagnostics.LogInfo("Stopping the host...");

                this.host.Stop();
            
                Common.Diagnostics.LogInfo("The host is stopped.");
            }
            catch (Exception e)
            {
                Common.Diagnostics.LogError(FormattableString.Invariant($"Unexpected error while stopping the host. {e.ToString()}"));
            }

            Common.Diagnostics.LogInfo("Windows Service is stopped");
        }

        private static string ReadConfiguration()
        {
            try
            {
                return File.ReadAllText("LocalForwarder.config");
            }
            catch (Exception e)
            {
                throw new ArgumentException(FormattableString.Invariant($"Could not read the configuration file. {e.ToString()}"), e);
            }
        }
    }
}
