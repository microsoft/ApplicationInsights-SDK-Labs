namespace Microsoft.ApplicationInsights.Extensibility
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OutsideCodeHelper;

    [TestClass]
    public class DependencyCallstacksTelemetryInitializerTests
    {
        private static readonly string CallStackIdentifier = "__MSCallStack";
        private static readonly string TestString = "TEST";
        private static readonly string CallStackEntryRegexFormat = "^{0},{1},[A-z0-9:.]+$";

        [TestMethod]
        public void InitalizerShouldHandleNullTelemetry()
        {
            // No assert here. If the initializer fails this test it will throw an exception
            ITelemetryInitializer initializer = new DependencyCallstacksTelemetryInitializer();
            initializer.Initialize(null);
        }

        [TestMethod]
        public void InitializerShouldAddCallStackForDependencyTelemetry()
        {
            ITelemetry telemetry = new DependencyTelemetry();
            ITelemetryInitializer initializer = new DependencyCallstacksTelemetryInitializer();

            // There should be no call stack info to start
            Assert.IsFalse(
                telemetry.Context.Properties.ContainsKey(CallStackIdentifier),
                "Call stack should not exist before initialization");

            // Initializing generates call stack info
            // Use OutsideCodeHelper so that part of the stack is detected as the user's code
            OutsideCodeHelper.Execute(() => { initializer.Initialize(telemetry); });
            Assert.IsTrue(
                telemetry.Context.Properties.ContainsKey(CallStackIdentifier), 
                "Call stack should exist");
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(telemetry.Context.Properties[CallStackIdentifier]),
                "Call stack should not be empty");
            Assert.IsTrue(
                telemetry.Context.Properties[CallStackIdentifier].Contains(typeof(OutsideCodeHelper).Name),
                "Call stack should successfully report called methods");
            
            // Initializing again does not overwrite call stack info
            telemetry.Context.Properties[CallStackIdentifier] = TestString;
            initializer.Initialize(telemetry);
            Assert.IsTrue(
                telemetry.Context.Properties[CallStackIdentifier].Equals(TestString),
                "Subsequent calls to initialize should not overwrite stored call stack");
        }

        [TestMethod]
        public void InitializerShouldNotAddCallStackForNonDependency()
        {
            ITelemetry telemetry = new EventTelemetry();
            ITelemetryInitializer initializer = new DependencyCallstacksTelemetryInitializer();

            // Use OutsideCodeHelper so that part of the stack is detected as the user's code
            OutsideCodeHelper.Execute(() => { initializer.Initialize(telemetry); });
            Assert.IsFalse(
                telemetry.Context.Properties.ContainsKey(CallStackIdentifier),
                "Call stack should not exist");
        }

        [TestMethod]
        public void InitializerShouldCorrectlyFormatCallStack()
        {
            ITelemetry telemetry = new DependencyTelemetry();
            ITelemetryInitializer initializer = new DependencyCallstacksTelemetryInitializer();

            // There should be no call stack info to start
            Assert.IsFalse(
                telemetry.Context.Properties.ContainsKey(CallStackIdentifier),
                "Call stack should not exist before initialization");

            // Use OutsideCodeHelper so that part of the stack is detected as the user's code
            OutsideCodeHelper.Execute(() => { initializer.Initialize(telemetry); });

            // The above line should be the only entry in the stack. Nothing else is "the user's code".
            // We'll verify format by making a regex that tests a line of the generated stack against
            // the correct namespace and function, allowing a variable filename/linenumber
            MethodInfo methodInfo = ((Action<Action>)OutsideCodeHelper.Execute).Method;
            Regex callStackEntryRegex = new Regex(string.Format(
                CultureInfo.InvariantCulture,
                CallStackEntryRegexFormat,
                methodInfo.Name,
                methodInfo.ReflectedType.FullName.Split('.').Last()));

            string[] callStack = telemetry.Context.Properties[CallStackIdentifier].Split(
                new char[] { '\n' }, 
                StringSplitOptions.RemoveEmptyEntries);

            Assert.IsTrue(
                callStack.Length == 1, 
                "Expected only one call stack entry");
            Assert.IsTrue(
                callStackEntryRegex.IsMatch(callStack[0]),
                "'{0}' does not follow the format: functionName,className,fileInfo\\n",
                callStack[0]);
        }
    }
}