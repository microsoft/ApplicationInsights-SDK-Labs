namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Security.Principal;
    using System.ServiceModel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        public void UserIdCopiedFromRequestIfPresent()
        {
            const string UserName = "MyUserName";
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("http://localhost/Service1.svc");
            context.OperationName = "GetData";

            context.Request.Context.User.Id = UserName;

            var initializer = new UserTelemetryInitializer();
            var telemetry = new EventTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(UserName, telemetry.Context.User.Id);
        }
    }
}
