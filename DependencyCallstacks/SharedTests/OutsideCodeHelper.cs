namespace OutsideCodeHelper
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class OutsideCodeHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Execute(Action lambda)
        {
            lambda();
        }
    }
}