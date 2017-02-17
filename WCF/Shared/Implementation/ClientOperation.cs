using System;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal struct ClientOperation
    {
        public String Action { get; private set; }
        public String Name { get; private set; }
        public bool IsOneWay { get; private set; }

        public ClientOperation(OperationDescription description)
        {
            Action = description.Messages[0].Action;
            IsOneWay = description.IsOneWay;
            Name = description.Name;
        }
    }
}
