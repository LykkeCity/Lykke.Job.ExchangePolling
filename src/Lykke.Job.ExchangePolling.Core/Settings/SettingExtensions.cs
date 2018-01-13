using System.Collections.Generic;
using System.Linq;
using Lykke.Job.ExchangePolling.Core.Settings.JobSettings;

namespace Lykke.Job.ExchangePolling.Core.Settings
{
    public static class SettingExtensions
    {
        public static IEnumerable<string> GetHandledExchanges(this ExchangePollingJobSettings settings)
        {
            return typeof(ExchangePollingJobSettings).GetProperties()
                .Where(x => x.PropertyType == typeof(ExchangeSettings))
                .Select(x => ((ExchangeSettings)x.GetValue(settings)).ExchangeName);
        }
    }
}
