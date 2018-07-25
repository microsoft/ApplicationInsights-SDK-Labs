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

        private readonly IInput gRpcInput;

        public Library()
        {
            try
            {
                this.gRpcInput = new GrpcInput("localhost", 50001);
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"Could not create the gRPC channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not create the gRPC channel. {e.ToString()}"), e);
            }
        }

        public void Run()
        {
            try
            {
                this.gRpcInput.Start(this.OnBatchReceived);
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"Could not start the gRPC channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not start the gRPC channel. {e.ToString()}"), e);
            }
        }

        public void Stop()
        {
            try
            {
                this.gRpcInput.Stop();
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Could not stop the gRPC channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not stop the gRPC channel. {e.ToString()}"), e);
            }
        }

        /// <summary>
        /// Processes an incoming telemetry batch
        /// </summary>
        private void OnBatchReceived(Inputs.Contracts.TelemetryBatch batch)
        {
            try
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
                                throw new ArgumentException(
                                    FormattableString.Invariant($"Empty telemetry item encountered"));
                            default:
                                throw new ArgumentException(
                                    FormattableString.Invariant($"Unknown telemetry item type encountered"));
                        }
                    }
                    catch (Exception e)
                    {
                        // an unexpected issue during conversion
                        // log and carry on
                        Common.Diagnostics.Log(
                            FormattableString.Invariant(
                                $"Could not convert an incoming telemetry item. {e.ToString()}"));
                    }

                    try
                    {
                        if (convertedTelemetry != null)
                        {
                            this.telemetryClient.Track(convertedTelemetry);
                        }
                    }
                    catch (Exception e)
                    {
                        // an unexpected issue while tracking an item
                        // log and carry on
                        Common.Diagnostics.Log(
                            FormattableString.Invariant(
                                $"Could not track an incoming telemetry item. {e.ToString()}"));
                    }
                }
            }
            catch (Exception e)
            {
                // an unexpected issue while processing the batch
                // log and carry on
                Common.Diagnostics.Log(
                    FormattableString.Invariant(
                        $"Could not process an incoming telemetry batch. {e.ToString()}"));
            }
        }
    }
}
