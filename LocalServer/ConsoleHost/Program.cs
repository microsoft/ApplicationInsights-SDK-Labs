namespace Microsoft.LocalForwarder.ConsoleHost
{
    using System;
    using System.IO;
    using Library;

    class Program
    {
        static void Main(string[] args)
        {
            Common.Diagnostics.LogInfo("Starting the console host...");

            Host host = new Host();

            try
            {
                Common.Diagnostics.LogInfo("Starting the host...");

                string config = ReadConfiguratiion();

                host.Run(config, TimeSpan.FromSeconds(5));

                Common.Diagnostics.LogInfo("The host is running");
            }
            catch (Exception e)
            {
                Common.Diagnostics.LogError(FormattableString.Invariant($"Unexpected error while starting the host. {e.ToString()}"));
                throw;
            }
            finally
            {
                Console.ReadKey();
            }

            try
            {
                Common.Diagnostics.LogInfo("Stopping the console host...");

                Common.Diagnostics.LogInfo("Stopping the host...");

                host.Stop();
                
                Common.Diagnostics.LogInfo("The host is stopped");
            }
            catch (Exception e)
            {
                Common.Diagnostics.LogError(FormattableString.Invariant($"Unexpected error while stopping the host. {e.ToString()}"));
            }
            finally
            {
                Common.Diagnostics.LogInfo("The console host is stopped");
                Console.ReadKey();
            }
        }

        private static string ReadConfiguratiion()
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
