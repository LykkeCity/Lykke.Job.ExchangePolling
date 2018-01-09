using System.Threading.Tasks;
using Lykke.Job.ExchangePolling.Contract;
using Lykke.Job.ExchangePolling.Core.Domain;

namespace Lykke.Job.ExchangePolling.Core.Services
{
    public interface IQuoteService
    {
        Task HandleQuote(OrderBook quote);

        ExchangeInstrumentQuote Get(string exchangeName, string instrument);
    }
}
