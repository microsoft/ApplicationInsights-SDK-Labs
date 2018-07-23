namespace Library
{
    using Inputs;
    using Inputs.GrpcInput;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using System;

    public class Library
    {
        private readonly TelemetryClient telemetryClient = new TelemetryClient();

        //!!!
        private readonly IInput gRpcInput = new GrpcInput(50001);

        public void Run()
        {
            // start the inputs
            this.gRpcInput.Start(this.OnBatchReceived);
        }

        public void Stop()
        {
            this.gRpcInput.Stop();
        }

        /// <summary>
        /// Processes an incoming telemetry batch
        /// </summary>
        private void OnBatchReceived(Inputs.Contracts.TelemetryBatch batch)
        {
            // send incoming telemetry items to the telemetryClient
            foreach (Inputs.Contracts.Telemetry telemetry in batch.Items)
            {
                ITelemetry convertedTelemetry = null;

                try
                {
                    switch (telemetry.DataCase)
                    {
                        case Inputs.Contracts.Telemetry.DataOneofCase.Event:
                            convertedTelemetry = TelemetryConverter.ConvertEventToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.Message:
                            convertedTelemetry = TelemetryConverter.ConvertTraceToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.Metric:
                            convertedTelemetry = TelemetryConverter.ConvertMetricToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.Exception:
                            convertedTelemetry = TelemetryConverter.ConvertExceptionToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.Dependency:
                            convertedTelemetry = TelemetryConverter.ConvertDependencyToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.Availability:
                            convertedTelemetry = TelemetryConverter.ConvertAvailabilityToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.PageView:
                            convertedTelemetry = TelemetryConverter.ConvertPageViewToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.Request:
                            convertedTelemetry = TelemetryConverter.ConvertRequestToSdkApi(telemetry);
                            break;
                        case Inputs.Contracts.Telemetry.DataOneofCase.None:
                            throw new ArgumentException(FormattableString.Invariant($"Empty telemetry item encountered"));
                        default:
                            throw new ArgumentException(FormattableString.Invariant($"Unknown telemetry item type encountered"));
                    }
                }
                catch (Exception e)
                {
                    // an unexpected issue during conversion
                    // log and carry on
                    Common.Diagnostics.Log(FormattableString.Invariant($"Could not convert an incoming telemetry item. {e.ToString()}"));
                }

                if (convertedTelemetry != null)
                {
                    this.telemetryClient.Track(convertedTelemetry);
                }
            }
        }
    }
}
