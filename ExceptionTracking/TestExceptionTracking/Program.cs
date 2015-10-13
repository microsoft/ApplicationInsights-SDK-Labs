using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestExceptionTracking
{
    class Program
    {
        static void Main(string[] args)
        {
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(10);
            }



            TelemetryClient client = new TelemetryClient();
            var modules = Microsoft.ApplicationInsights.Extensibility.Implementation.TelemetryModules.Instance.Modules;

            client.TrackEvent("Started");

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://getglimpse.com/Docs/Runtime-Policcies");
                httpWebRequest.GetResponse();
            }
            catch (Exception exc)
            {

            }

            client.Flush();

            Console.ReadLine();
        }
    }
}
