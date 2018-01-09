using Lykke.Job.ExchangePolling.Core.Settings.JobSettings;
using Lykke.Job.ExchangePolling.Core.Settings.SlackNotifications;

namespace Lykke.Job.ExchangePolling.Core.Settings
{
    public class ExchangePollingSettings
    {
        public ExchangePollingJobSettings ExchangePollingJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
