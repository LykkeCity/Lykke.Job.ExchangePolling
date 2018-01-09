using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Job.ExchangePolling.Core.Repositories
{
    public interface IGenericBlobRepository
	{
		[CanBeNull]
		T Read<T>(string blobContainer, string key);

		Task Write<T>(string blobContainer, string key, T obj);

		[ItemCanBeNull]
		Task<T> ReadAsync<T>(string blobContainer, string key);
	}
}
