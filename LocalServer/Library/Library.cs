namespace Library
{
    using Inputs.Contracts;
    using Inputs.GrpcInput;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Opencensus.Proto.Exporter;
    using Opencensus.Proto.Trace;
    using System;
    using System.Linq;
    using Exception = System.Exception;

    public class Library
    {
        private readonly TelemetryClient telemetryClient = new TelemetryClient();

        private readonly GrpcAiInput gRpcAiInput;
        private readonly GrpcOpenCensusInput gRpcOpenCensusInput;

        public Library()
        {
            try
            {
                this.gRpcAiInput = new GrpcAiInput("localhost", 50001);
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"Could not create the gRPC AI channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not create the gRPC AI channel. {e.ToString()}"), e);
            }

            try
            {
                this.gRpcOpenCensusInput = new GrpcOpenCensusInput("127.0.0.1", 50002);
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"Could not create the gRPC OpenCensus channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not create the gRPC OpenCensus channel. {e.ToString()}"), e);
            }
        }

        public void Run()
        {
            try
            {
                this.gRpcAiInput.Start(this.OnBatchReceived);
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"Could not start the gRPC AI channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not start the gRPC AI channel. {e.ToString()}"), e);
            }

            try
            {
                this.gRpcOpenCensusInput.Start(this.OnBatchReceived);
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"Could not start the gRPC OpenCensus channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not start the gRPC OpenCensus channel. {e.ToString()}"), e);
            }
        }

        public void Stop()
        {
            try
            {
                this.gRpcAiInput.Stop();
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Could not stop the gRPC AI channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not stop the gRPC AI channel. {e.ToString()}"), e);
            }

            try
            {
                this.gRpcOpenCensusInput.Stop();
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Could not stop the gRPC OpenCensus channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not stop the gRPC OpenCensus channel. {e.ToString()}"), e);
            }
        }

        /// <summary>
        /// Processes an incoming telemetry batch for AI channel.
        /// </summary>
        /// <remarks>This method may be called from multiple threads concurrently.</remarks>
        private void OnBatchReceived(TelemetryBatch batch)
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
                                convertedTelemetry = AiTelemetryConverter.ConvertEventToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.Message:
                                convertedTelemetry = AiTelemetryConverter.ConvertTraceToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.Metric:
                                convertedTelemetry = AiTelemetryConverter.ConvertMetricToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.Exception:
                                convertedTelemetry = AiTelemetryConverter.ConvertExceptionToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.Dependency:
                                convertedTelemetry = AiTelemetryConverter.ConvertDependencyToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.Availability:
                                convertedTelemetry = AiTelemetryConverter.ConvertAvailabilityToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.PageView:
                                convertedTelemetry = AiTelemetryConverter.ConvertPageViewToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.Request:
                                convertedTelemetry = AiTelemetryConverter.ConvertRequestToSdkApi(telemetry);
                                break;
                            case Inputs.Contracts.Telemetry.DataOneofCase.None:
                                throw new ArgumentException(
                                    FormattableString.Invariant($"Empty AI telemetry item encountered"));
                            default:
                                throw new ArgumentException(
                                    FormattableString.Invariant($"Unknown AI telemetry item type encountered"));
                        }
                    }
                    catch (Exception e)
                    {
                        // an unexpected issue during conversion
                        // log and carry on
                        Common.Diagnostics.Log(
                            FormattableString.Invariant(
                                $"Could not convert an incoming AI telemetry item. {e.ToString()}"));
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
                                $"Could not track an incoming AI telemetry item. {e.ToString()}"));
                    }
                }
            }
            catch (Exception e)
            {
                // an unexpected issue while processing the batch
                // log and carry on
                Common.Diagnostics.Log(
                    FormattableString.Invariant(
                        $"Could not process an incoming AI telemetry batch. {e.ToString()}"));
            }
        }

        /// <summary>
        /// Processes an incoming telemetry batch for OpenCensus channel.
        /// </summary>
        /// <remarks>This method may be called from multiple threads concurrently.</remarks>
        private void OnBatchReceived(ExportSpanRequest batch)
        {
            try
            {
                // send incoming telemetry items to the telemetryClient
                foreach (Span span in batch.Spans)
                {
                    try
                    {
                        //!!!
                        Common.Diagnostics.Log($"OpenCensus message received: {batch.Spans.Count} spans, first span: {batch.Spans.First().Name}");

                        this.telemetryClient.TrackSpan(span);
                    }
                    catch (Exception e)
                    {
                        // an unexpected issue while tracking an item
                        // log and carry on
                        Common.Diagnostics.Log(
                            FormattableString.Invariant(
                                $"Could not track an incoming OpenCensus telemetry item. {e.ToString()}"));
                    }
                }
            }
            catch (Exception e)
            {
                // an unexpected issue while processing the batch
                // log and carry on
                Common.Diagnostics.Log(
                    FormattableString.Invariant(
                        $"Could not process an incoming OpenCensus telemetry batch. {e.ToString()}"));
            }
        }
    }
}
