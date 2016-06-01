using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal class WcfOperationContext : IOperationContext, IExtension<OperationContext>
    {
        private OperationContext context;
        const String CallContextProperty = "AIWcfOperationContext";

        public String OperationId
        {
            get { return Request.Id; }
        }
        public String OperationName { get; private set; }
        public RequestTelemetry Request { get; private set; }

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
            get { return context.ServiceSecurityContext; }
        }

        public static IOperationContext Current
        {
            get { return GetContext();  }
        }

        private WcfOperationContext(OperationContext operationContext)
        {
            context = operationContext;
            OperationName = DiscoverOperationName(operationContext);
            Request = new RequestTelemetry();
            Request.GenerateOperationId();
        }

        public bool HasIncomingMessageProperty(string propertyName)
        {
            return context.IncomingMessageProperties.ContainsKey(propertyName);
        }

        public object GetIncomingMessageProperty(string propertyName)
        {
            return context.IncomingMessageProperties[propertyName];
        }

        public bool HasOutgoingMessageProperty(String propertyName)
        {
            return context.OutgoingMessageProperties.ContainsKey(propertyName);
        }

        public object GetOutgoingMessageProperty(String propertyName)
        {
            return context.OutgoingMessageProperties[propertyName];
        }

        public T GetIncomingMessageHeader<T>(String name, String ns)
        {
            int index = context.IncomingMessageHeaders.FindHeader(name, ns);
            if ( index >= 0 )
            {
                return context.IncomingMessageHeaders.GetHeader<T>(index);
            }
            return default(T);
        }

        public static WcfOperationContext FindContext(OperationContext owner)
        {
            // don't retrieve a context for a client-side OperationContext
            if ( owner != null && owner.IsClientSideContext() )
            {
                return null;
            }

            WcfOperationContext context = null;
            if ( owner != null )
            {
                context = owner.Extensions.Find<WcfOperationContext>();
            }
            if ( context == null )
            {
                context = CallContext.GetData(CallContextProperty) as WcfOperationContext;
            }
            return context;
        }

        private static IOperationContext GetContext()
        {
            var owner = OperationContext.Current;
            if ( owner.IsClientSideContext() )
            {
                owner = null;
            }
            WcfOperationContext context = FindContext(owner);

            if ( context == null )
            {
                if ( owner != null )
                {
                    context = new WcfOperationContext(owner);
                    owner.Extensions.Add(context);
                    // backup in case we can't get to the server-side OperationContext later
                    CallContext.SetData(CallContextProperty, context);
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
            var catchAll = operationContext.EndpointDispatcher.DispatchRuntime.UnhandledDispatchOperation;
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
