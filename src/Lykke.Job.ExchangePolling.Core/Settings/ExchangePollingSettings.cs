using Lykke.Job.LykkeJob.Core.Settings.JobSettings;
using Lykke.Job.LykkeJob.Core.Settings.SlackNotifications;

namespace Lykke.Job.LykkeJob.Core.Settings
{
    public class ExchangePollingSettings
    {
        public ExchangePollingJobSettings ExchangePollingJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
