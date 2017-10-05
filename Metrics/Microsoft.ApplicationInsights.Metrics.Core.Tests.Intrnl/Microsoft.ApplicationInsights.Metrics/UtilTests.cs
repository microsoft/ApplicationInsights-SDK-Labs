﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.ApplicationInsights.Metrics
{
    /// <summary />
    [TestClass]
    public class UtilTests
    {
        /// <summary />
        [TestMethod]
        public void ValidateNotNull()
        {
            Util.ValidateNotNull("foo", "specified name");
            Assert.ThrowsException<ArgumentNullException>( () => Util.ValidateNotNull(null, "specified name") );
        }

        /// <summary />
        [TestMethod]
        public void EnsureConcreteValue()
        {
            Assert.AreEqual(-1.7976931348623157E+308, Util.EnsureConcreteValue(Double.MinValue));
            Assert.AreEqual(Double.MinValue, Util.EnsureConcreteValue(-1.7976931348623157E+308));

            Assert.AreEqual(1.7976931348623157E+308, Util.EnsureConcreteValue(Double.MaxValue));
            Assert.AreEqual(Double.MaxValue, Util.EnsureConcreteValue(1.7976931348623157E+308));

            Assert.AreEqual(4.94065645841247E-324, Util.EnsureConcreteValue(Double.Epsilon));
            Assert.AreEqual(Double.Epsilon, Util.EnsureConcreteValue(4.94065645841247E-324));

            Assert.AreEqual(0.0, Util.EnsureConcreteValue(Double.NaN));
            Assert.AreEqual(Double.MinValue, Util.EnsureConcreteValue(Double.NegativeInfinity));
            Assert.AreEqual(Double.MaxValue, Util.EnsureConcreteValue(Double.PositiveInfinity));
        }

        /// <summary />
        [TestMethod]
        public void CopyTelemetryContext()
        {
            {
                TelemetryContext source = new TelemetryContext();
                TelemetryContext target = new TelemetryContext();

                Assert.ThrowsException<ArgumentNullException>( () => Util.CopyTelemetryContext(null, target) );
                Assert.ThrowsException<ArgumentNullException>( () => Util.CopyTelemetryContext(source, null) );
            }
            {
                TelemetryContext source = new TelemetryContext();
                TelemetryContext target = new TelemetryContext();

                source.User.AccountId = "A";
                source.User.AuthenticatedUserId = "B";

                target.User.AuthenticatedUserId = "C";
                target.User.Id = "D";

                Util.CopyTelemetryContext(source, target);

                Assert.AreEqual("A", target.User.AccountId);
                Assert.AreEqual("C", target.User.AuthenticatedUserId, "Does not overwrite existing values");
                Assert.AreEqual("D", target.User.Id);
                Assert.AreEqual(null, target.User.UserAgent);
            }
            {
                TelemetryContext source = new TelemetryContext();
                TelemetryContext target = new TelemetryContext();

#pragma warning disable 618     // Even Obsolete Context fields must be copied correctly!
                source.Cloud.RoleInstance = "A";
                source.Cloud.RoleName = "B";
                source.Component.Version = "C";
                source.Device.Id = "D";
                source.Device.Language = "E";
                source.Device.Model = "F";
                source.Device.NetworkType = "G";
                source.Device.OemName = "H";
                source.Device.OperatingSystem = "I";
                source.Device.ScreenResolution = "J";
                source.Device.Type = "K";
                source.InstrumentationKey = "L";
                source.Location.Ip = "M";
                source.Operation.Id = "N";
                source.Operation.Name = "O";
                source.Operation.ParentId = "P";
                source.Operation.SyntheticSource = "Q";
                source.Session.Id = "R";
                source.Session.IsFirst = true;
                source.User.AccountId = "S";
                source.User.AuthenticatedUserId = "T";
                source.User.Id = "U";
                source.User.UserAgent = "V";
#pragma warning restore 618
                source.Properties["Dim 1"] = "W";
                source.Properties["Dim 2"] = "X";
                source.Properties["Dim 3"] = "Y";

                Util.CopyTelemetryContext(source, target);

#pragma warning disable 618
                Assert.AreEqual("A", target.Cloud.RoleInstance);
                Assert.AreEqual("B", target.Cloud.RoleName);
                Assert.AreEqual("C", target.Component.Version);
                Assert.AreEqual("D", target.Device.Id);
                Assert.AreEqual("E", target.Device.Language);
                Assert.AreEqual("F", target.Device.Model);
                Assert.AreEqual("G", target.Device.NetworkType);
                Assert.AreEqual("H", target.Device.OemName);
                Assert.AreEqual("I", target.Device.OperatingSystem);
                Assert.AreEqual("J", target.Device.ScreenResolution);
                Assert.AreEqual("K", target.Device.Type);
                Assert.AreEqual(String.Empty, target.InstrumentationKey);
                Assert.AreEqual("M", target.Location.Ip);
                Assert.AreEqual("N", target.Operation.Id);
                Assert.AreEqual("O", target.Operation.Name);
                Assert.AreEqual("P", target.Operation.ParentId);
                Assert.AreEqual("Q", target.Operation.SyntheticSource);
                Assert.AreEqual("R", target.Session.Id);
                Assert.AreEqual(true, target.Session.IsFirst);
                Assert.AreEqual("S", target.User.AccountId);
                Assert.AreEqual("T", target.User.AuthenticatedUserId);
                Assert.AreEqual("U", target.User.Id);
                Assert.AreEqual("V", target.User.UserAgent);
#pragma warning restore 618

                Assert.IsTrue(target.Properties.ContainsKey("Dim 1"));
                Assert.AreEqual("W", target.Properties["Dim 1"]);

                Assert.IsTrue(target.Properties.ContainsKey("Dim 2"));
                Assert.AreEqual("X", target.Properties["Dim 2"]);

                Assert.IsTrue(target.Properties.ContainsKey("Dim 3"));
                Assert.AreEqual("Y", target.Properties["Dim 3"]);
            }
        }
    }
}