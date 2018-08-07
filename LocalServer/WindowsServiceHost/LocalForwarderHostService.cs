namespace Microsoft.LocalForwarder.WindowsServiceHost
{
    using System;
    using Microsoft.LocalForwarder.Library;
    using System.ServiceProcess;
    using System.IO;

    public partial class LocalForwarderHostService : ServiceBase
    {
        private Host host;

        public LocalForwarderHostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            Common.Diagnostics.Log("Starting Windows Service...");
            Common.Diagnostics.Log(FormattableString.Invariant($"Looking for configuration in {Directory.GetCurrentDirectory()}"));

            try
            {
                Common.Diagnostics.Log("Starting the host...");

                string config = ReadConfiguration();

                this.host = new Host();

                host.Start(config);

                Common.Diagnostics.Log("The host is running.");
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Unexpected error while starting the host. {e.ToString()}"));
                throw;
            }
        }

        protected override void OnStop()
        {
            Common.Diagnostics.Log("Stopping Windows Service...");

            try
            {
                Common.Diagnostics.Log("Stopping the host...");

                this.host.Stop();

                Common.Diagnostics.Log("The host is stopped.");
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Unexpected error while stopping the host. {e.ToString()}"));
            }
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
