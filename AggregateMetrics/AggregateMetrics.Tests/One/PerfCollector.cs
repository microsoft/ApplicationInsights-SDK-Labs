namespace AggregateMetrics.Tests.One
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal class PerfCollector
    {
        private readonly Stopwatch stopWatch;
        private readonly TestContext testContext;
        private readonly string prepDataPath;

        private long privateMemorySizeStart = 0;
        private long pagedMemorySizeStart = 0;
        private long virtualMemorySizeStart = 0;
        private long workingSetStart = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        internal PerfCollector(TestContext testContext)
        {
            string prepDataPath = Environment.GetEnvironmentVariable("PREP_DATA_PATH");
            if (!string.IsNullOrWhiteSpace(prepDataPath) && Directory.Exists(prepDataPath))
            {
                this.prepDataPath = prepDataPath;
            }

            this.stopWatch = new Stopwatch();
            this.testContext = testContext;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            using (Process process = Process.GetCurrentProcess())
            {
                this.privateMemorySizeStart = process.PrivateMemorySize64;
                this.pagedMemorySizeStart = process.PagedMemorySize64;
                this.virtualMemorySizeStart = process.VirtualMemorySize64;
                this.workingSetStart = process.WorkingSet64;
            }

            this.stopWatch.Start();
        }

        public bool ShouldSubmitPerfData
        {
            get
            {
#if DEBUG
                return false;
#else
                return this.prepDataPath != null;
#endif
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Convert.ToString(System.Int64)")]
        public void StopAndSubmitPerfData()
        {
            this.stopWatch.Stop();

            long elapsedTime = this.stopWatch.ElapsedMilliseconds;
            long privateMemorySizeDelta = 0;
            long pagedMemorySizeDelta = 0;
            long virtualMemorySizeDelta = 0;
            long workingSetDelta = 0;

            using (Process process = Process.GetCurrentProcess())
            {
                privateMemorySizeDelta = (process.PrivateMemorySize64 - this.privateMemorySizeStart) / 1024;
                pagedMemorySizeDelta = (process.PagedMemorySize64 - this.pagedMemorySizeStart) / 1024;
                virtualMemorySizeDelta = (process.VirtualMemorySize64 - this.virtualMemorySizeStart) / 1024;
                workingSetDelta = (process.WorkingSet64 - this.workingSetStart) / 1024;
            }

            if (!this.ShouldSubmitPerfData)
            {
                return;
            }

            var counterResults = new List<CounterResult>();

            PerfData data = new PerfData()
            {
                Name = Path.GetExtension(this.testContext.FullyQualifiedTestClassName).TrimStart('.') + "_" + this.testContext.TestName,
                ScenarioResult = new ScenarioResult()
                {
                    Name = this.testContext.TestName,
                    CounterResults = counterResults
                }
            };

            counterResults.Add(new CounterResult()
            {
                Name = "ExecutionTime",
                Default = true,
                Units = "MilliSeconds",
                Value = Convert.ToString(elapsedTime)
            });

            counterResults.Add(new CounterResult()
            {
                Name = "PrivateMemorySizeDelta",
                Units = "KBytes",
                Value = Convert.ToString(privateMemorySizeDelta)
            });

            counterResults.Add(new CounterResult()
            {
                Name = "PagedMemorySizeDelta",
                Units = "KBytes",
                Value = Convert.ToString(pagedMemorySizeDelta)
            });

            counterResults.Add(new CounterResult()
            {
                Name = "VirtualMemorySizeDelta",
                Units = "KBytes",
                Value = Convert.ToString(virtualMemorySizeDelta)
            });

            counterResults.Add(new CounterResult()
            {
                Name = "WorkingSetDelta",
                Units = "KBytes",
                Value = Convert.ToString(workingSetDelta)
            });

            string dropPath = Path.Combine(this.prepDataPath, @"MetricsAggregations", DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss", CultureInfo.CurrentCulture));
            if (!Directory.Exists(dropPath))
            {
                Directory.CreateDirectory(dropPath);
            }

            data.WriteXml(dropPath);
        }
    }
}
