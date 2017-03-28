namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;

    public class MockOperationContext : IOperationContext, IOperationContextState
    {
        private Dictionary<string, object> stateDictionary;

        public MockOperationContext()
        {
            this.IncomingProperties = new Dictionary<string, object>();
            this.IncomingHeaders = new Dictionary<string, object>();
            this.OutgoingProperties = new Dictionary<string, object>();
            this.ContractName = "IFakeService";
            this.ContractNamespace = "urn:fake";
            this.Request = new RequestTelemetry();
            this.Request.GenerateOperationId();
            this.OwnsRequest = true;
            this.stateDictionary = new Dictionary<string, object>();
        }

        public IDictionary<string, object> IncomingProperties { get; private set; }

        public IDictionary<string, object> OutgoingProperties { get; private set; }

        public IDictionary<string, object> IncomingHeaders { get; private set; }

        public string OperationId
        {
            get { return this.Request.Id; }
        }

        public RequestTelemetry Request { get; private set; }

        public bool OwnsRequest { get; private set; }

        public Uri EndpointUri { get; set; }

        public Uri ToHeader { get; set; }

        public string OperationName { get; set; }

        public string ContractName { get; set; }

        public string ContractNamespace { get; set; }

        public ServiceSecurityContext SecurityContext { get; set; }

        public bool HasIncomingMessageProperty(string propertyName)
        {
            return this.IncomingProperties.ContainsKey(propertyName);
        }

        public object GetIncomingMessageProperty(string propertyName)
        {
            return this.IncomingProperties[propertyName];
        }

        public bool HasOutgoingMessageProperty(string propertyName)
        {
            return this.OutgoingProperties.ContainsKey(propertyName);
        }

        public object GetOutgoingMessageProperty(string propertyName)
        {
            return this.OutgoingProperties[propertyName];
        }

        public T GetIncomingMessageHeader<T>(string name, string ns)
        {
            object value;
            if (this.IncomingHeaders.TryGetValue(ns + "#" + name, out value))
            {
                return (T)value;
            }

            return default(T);
        }

        public void AddIncomingMessageHeader<T>(string name, string ns, T value)
        {
            this.IncomingHeaders.Add(ns + "#" + name, value);
        }

        public void SetHttpHeaders(HttpRequestMessageProperty httpHeaders)
        {
            this.IncomingProperties[HttpRequestMessageProperty.Name] = httpHeaders;
        }

        public void SetState(string key, object value)
        {
            this.stateDictionary.Add(key, value);
        }

        public bool TryGetState<T>(string key, out T value)
        {
            value = default(T);
            object obj = null;
            if (this.stateDictionary.TryGetValue(key, out obj))
            {
                value = (T)obj;
                return true;
            }

            return false;
        }
    }
}