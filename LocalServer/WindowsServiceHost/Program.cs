namespace Microsoft.LocalForwarder.WindowsServiceHost
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                // debugging
                var service = new LocalForwarderHostService();

                service.TestStartStop(TimeSpan.FromSeconds(10));
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new LocalForwarderHostService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
