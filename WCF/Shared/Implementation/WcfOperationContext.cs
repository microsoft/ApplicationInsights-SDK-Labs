namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using Microsoft.ApplicationInsights.DataContracts;

    internal class WcfOperationContext : IOperationContext, IExtension<OperationContext>, IOperationContextState
    {
        private const string CallContextProperty = "AIWcfOperationContext";
        private OperationContext context;
        private ConcurrentDictionary<string, object> stateDicctionary;

        private WcfOperationContext(OperationContext operationContext, RequestTelemetry httpCtxTelemetry)
        {
            this.context = operationContext;
            this.stateDicctionary = new ConcurrentDictionary<string, object>();
            this.OperationName = this.DiscoverOperationName(operationContext);
            if (httpCtxTelemetry != null)
            {
                this.Request = httpCtxTelemetry;
                this.OwnsRequest = false;
            }
            else
            {
                this.Request = new RequestTelemetry();
                this.Request.GenerateOperationId();
                this.OwnsRequest = true;
            }

            WcfEventSource.Log.OperationContextCreated(this.Request.Id, this.OwnsRequest);
        }

        public static IOperationContext Current
        {
            get { return GetContext(); }
        }

        public string OperationId
        {
            get { return this.Request.Id; }
        }

        public string OperationName { get; private set; }

        public RequestTelemetry Request { get; private set; }

        public bool OwnsRequest { get; private set; }

        public string ContractName
        {
            get { return this.context.EndpointDispatcher.ContractName; }
        }

        public string ContractNamespace
        {
            get { return this.context.EndpointDispatcher.ContractNamespace; }
        }

        public Uri EndpointUri
        {
            get { return this.context.EndpointDispatcher.EndpointAddress.Uri; }
        }

        public Uri ToHeader
        {
            get { return this.context.IncomingMessageHeaders.To; }
        }

        public ServiceSecurityContext SecurityContext
        {
            get { return this.GetSecurityContext(); }
        }

        // In the normal case, we'll use the OperationContext
        // found in the local thread. However, there are cases this won't work:
        // - When async calls have been done and .NET < 4.6
        // - Within a *client* side OperationContextScope
        // To work around this, we store a copy of our context on the
        // thread's LogicalCallContext, so that it gets moved from thread to thread
        // Because this field is not serializable, we store an
        // ObjectHandle instead.
        public static WcfOperationContext FindContext(OperationContext owner)
        {
            // don't retrieve a context for a client-side OperationContext
            if (owner != null && owner.IsClientSideContext())
            {
                return null;
            }

            WcfOperationContext context = null;
            if (context == null && owner != null)
            {
                context = owner.Extensions.Find<WcfOperationContext>();
            }

            if (context == null)
            {
                var handle = CallContext.LogicalGetData(CallContextProperty) as ObjectHandle;
                if (handle != null)
                {
                    context = handle.Unwrap() as WcfOperationContext;
                }
            }

            return context;
        }

        public bool HasIncomingMessageProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            try
            {
                return this.context.IncomingMessageProperties.ContainsKey(propertyName);
            }
            catch (ObjectDisposedException)
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed("reading property", propertyName);
                return false;
            }
        }

        public object GetIncomingMessageProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            try
            {
                return this.context.IncomingMessageProperties[propertyName];
            }
            catch (ObjectDisposedException)
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed("reading property", propertyName);
                return null;
            }
        }

        public bool HasOutgoingMessageProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            try
            {
                return this.context.OutgoingMessageProperties.ContainsKey(propertyName);
            }
            catch (ObjectDisposedException)
            {
                // WCF message has been closed already
                WcfEventSource.Log.ResponseMessageClosed("reading property", propertyName);
                return false;
            }
        }

        public object GetOutgoingMessageProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            try
            {
                return this.context.OutgoingMessageProperties[propertyName];
            }
            catch (ObjectDisposedException)
            {
                // WCF message has been closed already
                WcfEventSource.Log.ResponseMessageClosed("reading property", propertyName);
                return null;
            }
        }

        public T GetIncomingMessageHeader<T>(string name, string ns)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(ns))
            {
                throw new ArgumentNullException(nameof(ns));
            }

            try
            {
                var index = this.context.IncomingMessageHeaders.FindHeader(name, ns);
                if (index >= 0)
                {
                    return this.context.IncomingMessageHeaders.GetHeader<T>(index);
                }
            }
            catch (ObjectDisposedException)
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed("reading header", ns + "#" + name);
            }

            return default(T);
        }

        void IExtension<OperationContext>.Attach(OperationContext owner)
        {
        }

        void IExtension<OperationContext>.Detach(OperationContext owner)
        {
        }

        void IOperationContextState.SetState(string key, object value)
        {
            this.stateDicctionary[key] = value;
        }

        bool IOperationContextState.TryGetState<T>(string key, out T value)
        {
            value = default(T);
            object storedValue = null;

            if (this.stateDicctionary.TryGetValue(key, out storedValue))
            {
                value = (T)storedValue;
                return true;
            }

            return false;
        }

        private static IOperationContext GetContext()
        {
            var owner = OperationContext.Current;
            if (owner != null && owner.IsClientSideContext())
            {
                owner = null;
            }

            var context = FindContext(owner);
            if (context == null)
            {
                if (owner != null)
                {
                    context = new WcfOperationContext(owner, PlatformContext.RequestFromHttpContext());
                    owner.Extensions.Add(context);

                    // backup in case we can't get to the server-side OperationContext later
                    CallContext.LogicalSetData(CallContextProperty, new ObjectHandle(context));
                }

                // no server-side OperationContext to attach to
            }

            return context;
        }

        private ServiceSecurityContext GetSecurityContext()
        {
            try
            {
                return this.context.ServiceSecurityContext;
            }
            catch (ObjectDisposedException)
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed("reading", "ServiceSecurityContext");
                return null;
            }
        }

        private string DiscoverOperationName(OperationContext operationContext)
        {
            var runtime = operationContext.EndpointDispatcher.DispatchRuntime;
            string action = operationContext.IncomingMessageHeaders.Action;
            if (!string.IsNullOrEmpty(action))
            {
                foreach (var op in runtime.Operations)
                {
                    if (op.Action == action)
                    {
                        return op.Name;
                    }
                }
            }
            else
            {
                // WebHttpDispatchOperationSelector will stick the
                // selected operation name into a message property
                return this.GetWebHttpOperationName(operationContext);
            }

            var catchAll = runtime.UnhandledDispatchOperation;
            if (catchAll != null)
            {
                return catchAll.Name;
            }

            return "*";
        }

        private string GetWebHttpOperationName(OperationContext operationContext)
        {
            var name = WebHttpDispatchOperationSelector.HttpOperationNamePropertyName;
            if (this.HasIncomingMessageProperty(name))
            {
                return this.GetIncomingMessageProperty(name) as string;
            }

            return "<unknown>";
        }
    }
}
