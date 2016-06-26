namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using Microsoft.ApplicationInsights.DataContracts;

    internal class NamedCounterValueBase
    {
        private readonly string name;
        private readonly TelemetryContext context;

        private static TelemetryContext CreateContextCopy(TelemetryContext context)
        {
            var newContext = new TelemetryContext();

            CopyContext(context, newContext);

            return newContext;
        }

        private static void CopyContext(TelemetryContext context, TelemetryContext newContext)
        {
            if (!string.IsNullOrWhiteSpace(context.InstrumentationKey))
            {
                newContext.InstrumentationKey = context.InstrumentationKey;
            }

            newContext.Cloud.RoleInstance = context.Cloud.RoleInstance;
            newContext.Cloud.RoleName = context.Cloud.RoleName;
            newContext.Component.Version = context.Component.Version;
            newContext.Device.Id = context.Device.Id;
            newContext.Device.Language = context.Device.Language;
            newContext.Device.Model = context.Device.Model;
            newContext.Device.NetworkType = context.Device.NetworkType;
            newContext.Device.OemName = context.Device.OemName;
            newContext.Device.OperatingSystem = context.Device.OperatingSystem;
            newContext.Device.ScreenResolution = context.Device.ScreenResolution;
            newContext.Device.Type = context.Device.Type;
            newContext.Location.Ip = context.Location.Ip;
            newContext.Operation.Id = context.Operation.Id;
            newContext.Operation.Name = context.Operation.Name;
            newContext.Operation.ParentId = context.Operation.ParentId;
            newContext.Operation.SyntheticSource = context.Operation.SyntheticSource;
            newContext.Session.Id = context.Session.Id;
            newContext.User.AccountId = context.User.AccountId;
            newContext.User.AuthenticatedUserId = context.User.AuthenticatedUserId;
            newContext.User.Id = context.User.Id;
            newContext.User.UserAgent = context.User.UserAgent;

            foreach (var prop in context.Properties)
            {
                newContext.Properties.Add(prop);
            }
        }

        public NamedCounterValueBase(string name, TelemetryContext context)
        {
            this.name = name;
            this.context = CreateContextCopy(context);
        }

        public MetricTelemetry GetInitializedMetricTelemetry()
        {
            var metric = new MetricTelemetry();
            metric.Name = this.name;

            CopyContext(this.context, metric.Context);

            return metric;
        }
    }
}
