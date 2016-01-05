namespace SelfHostedWebApiApplication
{
    using Microsoft.Owin.Hosting;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>(@"http://localhost:12345"))
            {
                // http://localhost:12345/api/test/geta
                Console.ReadKey();
            }
        }
    }
}
