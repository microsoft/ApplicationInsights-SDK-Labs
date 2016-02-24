using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class UserTelemetryInitializerTests
    {
        [TestMethod]
        public void AnonymousDoesNotIncludeUserId()
        {
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("http://localhost/Service1.svc");
            context.OperationName = "GetData";

            var authContext = new SimpleAuthorizationContext();
            context.SecurityContext = new ServiceSecurityContext(authContext);

            var initializer = new UserTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.IsNull(telemetry.Context.User.Id);
        }

        [TestMethod]
        public void AuthenticatedRequestFillsUserIdWithUserName()
        {
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("http://localhost/Service1.svc");
            context.OperationName = "GetData";

            var authContext = new SimpleAuthorizationContext();
            authContext.AddIdentity(new GenericIdentity("myuser"));
            context.SecurityContext = new ServiceSecurityContext(authContext);

            var initializer = new UserTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual("myuser", telemetry.Context.User.Id);
        }
    }
}
