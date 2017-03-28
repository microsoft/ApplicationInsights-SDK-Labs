namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Description;

    internal struct ClientOperation
    {
        public ClientOperation(string contractName, OperationDescription description)
        {
            this.Action = description.Messages[0].Action;
            this.IsOneWay = description.IsOneWay;

            // Doing this here means we won't need to concatenate on each service call
            this.Name = contractName + "." + description.Name;
        }

        public string Action { get; private set; }

        public string Name { get; private set; }

        public bool IsOneWay { get; private set; }
    }
}
