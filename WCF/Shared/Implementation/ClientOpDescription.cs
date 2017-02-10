using System;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal struct ClientOpDescription
    {
        public String Action { get; set; }
        public String Name { get; set; }
        public bool IsOneWay { get; set; }

        public static ClientOpDescription FromDescription(OperationDescription description)
        {
            return new ClientOpDescription
            {
                Action = description.Messages[0].Action,
                IsOneWay = description.IsOneWay,
                Name = description.Name
            };
        }
    }
}
