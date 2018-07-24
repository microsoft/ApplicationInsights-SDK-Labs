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
                Console.WriteLine("Starting Local Forwarder...");

                Library localForwarder = new Library();
                localForwarder.Run();

                Console.WriteLine("Local Forwarder is running. Press any key to stop.");

                Console.ReadKey();

                localForwarder.Stop();

                Console.WriteLine("Local Forwarder is stopped");
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}
