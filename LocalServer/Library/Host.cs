namespace Microsoft.LocalForwarder.Library
{
    using System;

    public class Host
    {
        private Library library;

        public void Start(string configuration)
        {
            try
            {
                Common.Diagnostics.Log("Starting the library...");

                this.library = new Library(configuration);

                this.library.Run();

                Common.Diagnostics.Log("The library is running");
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Unexpected error at start-up of the library. {e.ToString()}"));
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                this.library?.Stop();

                Common.Diagnostics.Log("The library is stopped");
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Unexpected error while stopping the library. {e.ToString()}"));
                throw;
            }
        }
    }
}