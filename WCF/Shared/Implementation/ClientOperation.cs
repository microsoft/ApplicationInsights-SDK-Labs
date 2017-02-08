using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    struct ClientOperation
    {
        public String Action { get; set; }
        public String Name { get; set; }
        public bool IsOneWay { get; set; }

        public static ClientOperation FromDescription(OperationDescription description)
        {
            return new ClientOperation
            {
                Action = description.Messages[0].Action,
                IsOneWay = description.IsOneWay,
                Name = description.Name
            };
        }
    }
}
