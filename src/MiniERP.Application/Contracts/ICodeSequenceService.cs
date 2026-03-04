using System.Threading.Tasks;

namespace MiniERP.Application.Contracts;

public interface ICodeSequenceService
{
    Task<string> GenerateNextAsync(string entityName, string defaultPrefix, int defaultPadding = 6);
}
