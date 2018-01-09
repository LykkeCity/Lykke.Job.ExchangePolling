using System.Threading.Tasks;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core.Domain;

namespace Lykke.Job.LykkeJob.Core.Services
{
    public interface IQuoteService
    {
        Task HandleQuote(ExchangeBestPrice quote);

        ExchangeInstrumentQuote Get(string exchangeName, string instrument);
    }
}
