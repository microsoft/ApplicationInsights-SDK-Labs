using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Concurrent;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal class WcfOperationContext : IOperationContext, IExtension<OperationContext>, IOperationContextState
    {
        private OperationContext context;
        private ConcurrentDictionary<String, Object> stateDicctionary;

        const String CallContextProperty = "AIWcfOperationContext";

        public String OperationId
        {
            get { return Request.Id; }
        }
        public String OperationName { get; private set; }
        public RequestTelemetry Request { get; private set; }
        public bool OwnsRequest { get; private set; }

        public String ContractName
        {
            get { return context.EndpointDispatcher.ContractName; }
        }
        public String ContractNamespace
        {
            get { return context.EndpointDispatcher.ContractNamespace; }
        }
        public Uri EndpointUri
        {
            get { return context.EndpointDispatcher.EndpointAddress.Uri; }
        }
        public Uri ToHeader
        {
            get { return context.IncomingMessageHeaders.To; }
        }
        public ServiceSecurityContext SecurityContext
        {
            get { return GetSecurityContext(); }
        }

        public static IOperationContext Current
        {
            get { return GetContext();  }
        }

        private WcfOperationContext(OperationContext operationContext, RequestTelemetry httpCtxTelemetry)
        {
            context = operationContext;
            stateDicctionary = new ConcurrentDictionary<string, object>();
            OperationName = DiscoverOperationName(operationContext);
            if ( httpCtxTelemetry != null )
            {
                Request = httpCtxTelemetry;
                OwnsRequest = false;
            } else
            {
                Request = new RequestTelemetry();
                Request.GenerateOperationId();
                OwnsRequest = true;
            }
            WcfEventSource.Log.OperationContextCreated(Request.Id, OwnsRequest);
        }

        public bool HasIncomingMessageProperty(string propertyName)
        {
            if ( String.IsNullOrEmpty(propertyName) )
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            try
            {
                return context.IncomingMessageProperties.ContainsKey(propertyName);
            } catch ( ObjectDisposedException )
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed(propertyName);
                return false;
            }
        }

        public object GetIncomingMessageProperty(string propertyName)
        {
            if ( String.IsNullOrEmpty(propertyName) )
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            try
            {
                return context.IncomingMessageProperties[propertyName];
            } catch ( ObjectDisposedException )
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed(propertyName);
                return null;
            }
        }

        public bool HasOutgoingMessageProperty(String propertyName)
        {
            if ( String.IsNullOrEmpty(propertyName) )
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            try
            {
                return context.OutgoingMessageProperties.ContainsKey(propertyName);
            } catch ( ObjectDisposedException )
            {
                // WCF message has been closed already
                WcfEventSource.Log.ResponseMessageClosed(propertyName);
                return false;
            }
        }

        public object GetOutgoingMessageProperty(String propertyName)
        {
            if ( String.IsNullOrEmpty(propertyName) )
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            try
            {
                return context.OutgoingMessageProperties[propertyName];
            } catch ( ObjectDisposedException )
            {
                // WCF message has been closed already
                WcfEventSource.Log.ResponseMessageClosed(propertyName);
                return null;
            }
        }

        public T GetIncomingMessageHeader<T>(String name, String ns)
        {
            if ( String.IsNullOrEmpty(name) )
            {
                throw new ArgumentNullException(nameof(name));
            }
            if ( String.IsNullOrEmpty(ns) )
            {
                throw new ArgumentNullException(nameof(ns));
            }
            try
            {
                int index = context.IncomingMessageHeaders.FindHeader(name, ns);
                if ( index >= 0 )
                {
                    return context.IncomingMessageHeaders.GetHeader<T>(index);
                }
            } catch ( ObjectDisposedException )
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed(ns + "#" + name);
            }
            return default(T);
        }

        //
        // In the normal case, we'll use the OperationContext
        // found in the local thread. However, there are cases this won't work:
        // - When async calls have been done and .NET < 4.6
        // - Within a *client* side OperationContextScope
        // To work around this, we store a copy of our context on the
        // thread's LogicalCallContext, so that it gets moved from thread to thread
        // Because this field is not serializable, we store an
        // ObjectHandle instead.
        //
        public static WcfOperationContext FindContext(OperationContext owner)
        {
            // don't retrieve a context for a client-side OperationContext
            if ( owner != null && owner.IsClientSideContext() )
            {
                return null;
            }

            WcfOperationContext context = null;
            if ( context == null && owner != null )
            {
                context = owner.Extensions.Find<WcfOperationContext>();
            }
            if ( context == null )
            {
                var handle = CallContext.LogicalGetData(CallContextProperty) as ObjectHandle;
                if ( handle != null )
                {
                    context = handle.Unwrap() as WcfOperationContext;
                }
            }
            return context;
        }

        private ServiceSecurityContext GetSecurityContext()
        {
            try
            {
                return this.context.ServiceSecurityContext;
            } catch ( ObjectDisposedException )
            {
                // WCF message has been closed already
                WcfEventSource.Log.RequestMessageClosed("ServiceSecurityContext");
                return null;
            }
        }

        private static IOperationContext GetContext()
        {
            var owner = OperationContext.Current;
            if ( owner != null && owner.IsClientSideContext() )
            {
                owner = null;
            }
            WcfOperationContext context = FindContext(owner);

            if ( context == null )
            {
                if ( owner != null )
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

        void IExtension<OperationContext>.Attach(OperationContext owner)
        {
        }

        void IExtension<OperationContext>.Detach(OperationContext owner)
        {
        }

        void IOperationContextState.SetState(string key, object value)
        {
            stateDicctionary[key] = value;
        }

        bool IOperationContextState.TryGetState<T>(string key, out T value)
        {
            value = default(T);
            object storedValue = null;
            
            if ( stateDicctionary.TryGetValue(key, out storedValue) )
            {
                value = (T)storedValue;
                return true;
            }
            return false;
        }

        private String DiscoverOperationName(OperationContext operationContext)
        {
            var runtime = operationContext.EndpointDispatcher.DispatchRuntime;
            String action = operationContext.IncomingMessageHeaders.Action;
            if ( !String.IsNullOrEmpty(action) )
            {
                foreach ( var op in runtime.Operations )
                {
                    if ( op.Action == action )
                    {
                        return op.Name;
                    }
                }
            } else
            {
                // WebHttpDispatchOperationSelector will stick the
                // selected operation name into a message property
                return GetWebHttpOperationName(operationContext);
            }
            var catchAll = runtime.UnhandledDispatchOperation;
            if ( catchAll != null )
            {
                return catchAll.Name;
            }
            return "*";
        }

        private string GetWebHttpOperationName(OperationContext operationContext)
        {
            var name = WebHttpDispatchOperationSelector.HttpOperationNamePropertyName;
            if ( HasIncomingMessageProperty(name) )
            {
                return GetIncomingMessageProperty(name) as String;
            }
            return "<unknown>";
        }
    }
}
