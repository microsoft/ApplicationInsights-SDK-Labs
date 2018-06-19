namespace Test.Library.Inputs.NamedPipeInput
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Library.Inputs.NamedPipeInput;

    [TestClass]
    public class NamedPipeInputTests
    {
        [TestMethod]
        public void NamedPipeInputTests_StartsAndStops()
        {
            // ARRANGE
            var input = new NamedPipeInput();

            // ACT
            input.Start();

            // ASSERT
        }
    }
}
