using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientTelemetryBindingElement : BindingElement
    {
        public override BindingElement Clone()
        {
            return null;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return default(T);
        }
    }
}
