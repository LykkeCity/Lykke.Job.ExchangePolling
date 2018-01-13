namespace Lykke.Job.ExchangePolling.Core
{
    public static class Constants
    {
        public static double PrecisionThreshhold = 0.01;

        public static string DiffOrderPrefix = "POSITION_CONTROL_POLLING_";

        public static string BlobContainerName = "exchangepollingjob";
        public static string BlobExchangesCache = "ExchangesCache";
    }
}
