namespace Microsoft.LocalForwarder.Host
{
    using System;
    using System.IO;
    using Library;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Common.Diagnostics.Log("Starting Local Forwarder...");

                string config = ReadConfiguratiion();
                Library localForwarder = new Library(config);

                localForwarder.Run();

                Common.Diagnostics.Log("Local Forwarder is running");

                Console.ReadKey();

                localForwarder.Stop();

                Common.Diagnostics.Log("Local Forwarder is stopped");
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Unexpected error at start-up. {e.ToString()}"));
            }
            finally
            {
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
