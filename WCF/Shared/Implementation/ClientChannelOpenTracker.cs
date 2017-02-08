using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal class ClientChannelOpenTracker : IChannelInitializer
    {
        private TelemetryClient telemetryClient;
        private String contractName;
        private ConditionalWeakTable<object, DependencyTelemetry> pendingOperations;

        public ClientChannelOpenTracker(TelemetryClient client, Type contractType)
        {
            if ( client == null )
            {
                throw new ArgumentNullException(nameof(client));
            }
            if ( contractType == null )
            {
                throw new ArgumentNullException(nameof(contractType));
            }

            telemetryClient = client;
            this.contractName = contractType.FullName;
            pendingOperations = new ConditionalWeakTable<object, DependencyTelemetry>();
        }

        public void Initialize(IClientChannel channel)
        {
            if ( channel == null )
            {
                throw new ArgumentNullException(nameof(channel));
            }
            // The trick lies in the Communication Object state model:
            // https://msdn.microsoft.com/en-us/library/ms789041(v=vs.110).aspx
            // We want to hook into channel.Opening and start the timer
            // After, either Opened will fire (successful) and we'll complete,
            // or, Faulted will fire (failed) and we'll complete. We don't care afterwads about it
            HookEvents(channel);
        }

        private void HookEvents(IClientChannel channel)
        {
            channel.Opening += OnChannelOpening;
            channel.Opened += OnChannelOpened;
            channel.Faulted += OnChannelFaulted;
        }

        private void UnhookEvents(IClientChannel channel)
        {
            channel.Opening -= OnChannelOpening;
            channel.Opened -= OnChannelOpened;
            channel.Faulted -= OnChannelFaulted;
        }

        private void OnChannelOpening(object sender, EventArgs e)
        {
            try
            {
                var channel = (IClientChannel)sender;
                var telemetry = new DependencyTelemetry();
                telemetry.Start();
                telemetry.Type = DependencyConstants.WcfChannelOpen;
                telemetry.Target = channel.RemoteAddress.Uri.Host;
                telemetry.Name = channel.RemoteAddress.Uri.ToString();
                telemetry.Data = contractName;

                pendingOperations.Add(sender, telemetry);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ChannelTrackerError(nameof(OnChannelOpening), ex.ToString());
            }
        }

        private void OnChannelOpened(object sender, EventArgs e)
        {
            try
            {
                UnhookEvents((IClientChannel)sender);
                DependencyTelemetry telemetry;
                if ( !pendingOperations.TryGetValue(sender, out telemetry) )
                {
                    // TODO: handle error
                    return;
                }
                telemetry.Success = true;
                telemetry.Stop();
                telemetryClient.TrackDependency(telemetry);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ChannelTrackerError(nameof(OnChannelOpened), ex.ToString());
            }
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            try
            {
                UnhookEvents((IClientChannel)sender);
                DependencyTelemetry telemetry;
                if ( !pendingOperations.TryGetValue(sender, out telemetry) )
                {
                    // TODO: handle error
                    return;
                }
                telemetry.Success = false;
                telemetry.Stop();
                telemetryClient.TrackDependency(telemetry);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ChannelTrackerError(nameof(OnChannelFaulted), ex.ToString());
            }
        }

    }
}
