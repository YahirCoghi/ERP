using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.Common;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Services;

public class CodeSequenceService : ICodeSequenceService
{
    private readonly ApplicationDbContext _context;

    public CodeSequenceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAsync(string entityName, string defaultPrefix, int defaultPadding = 6)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var normalizedName = entityName.Trim().ToLowerInvariant();
        var sequence = await _context.EntitySequences
            .FirstOrDefaultAsync(s => s.EntityName == normalizedName);

        if (sequence == null)
        {
            sequence = new EntitySequence
            {
                EntityName = normalizedName,
                Prefix = defaultPrefix,
                LastNumber = 0,
                Padding = defaultPadding
            };
            _context.EntitySequences.Add(sequence);
        }

        sequence.LastNumber += 1;
        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        var formattedNumber = sequence.LastNumber.ToString($"D{sequence.Padding}", CultureInfo.InvariantCulture);
        return $"{sequence.Prefix}{formattedNumber}";
    }
}
