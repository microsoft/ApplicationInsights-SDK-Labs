namespace Common
{
    using System.Threading;
    using NLog;

    public class Diagnostics
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static SpinLock spinLock = new SpinLock();

        public static void Log(string message)
        {
            bool lockTaken = false;
            spinLock.Enter(ref lockTaken);

            if (lockTaken)
            {
                // ok to lose the message in an unlikely case that lockTaken is false
                logger.Log(LogLevel.Error, message);

                spinLock.Exit();
            }
        }
    }
}
