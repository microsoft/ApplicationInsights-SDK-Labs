namespace Host
{
    using System;
    using Library;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Common.Diagnostics.Log("Starting Local Forwarder...");

                Library localForwarder = new Library();
                localForwarder.Run();

                Common.Diagnostics.Log("Local Forwarder is running. Press any key to stop.");

                Console.ReadKey();

                localForwarder.Stop();

                Common.Diagnostics.Log("Local Forwarder is stopped");
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}
