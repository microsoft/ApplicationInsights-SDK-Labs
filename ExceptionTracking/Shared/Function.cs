using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ApplicationInsights.ExceptionTracking
{
    public class Function
    {
        public string Name { get; set; }

        public uint ArgumentsCount { get; set; }

        public string AssemblyName { get; set; }

        public string ModuleName { get; set; }
    }
}
