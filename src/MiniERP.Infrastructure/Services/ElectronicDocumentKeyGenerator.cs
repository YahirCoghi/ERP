using System;
using System.Security.Cryptography;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Infrastructure.Services;

public static class ElectronicDocumentKeyGenerator
{
    public static string GenerateKey(DateTime issueDate, string issuerIdentification, string consecutive, int situation = 1)
    {
        var datePart = issueDate.ToString("ddMMyy");
        var issuerPart = HaciendaMappings.FormatIssuerIdForKey(issuerIdentification);
        var situationPart = situation.ToString("0");
        var securityCode = GenerateSecurityCode();

        return $"506{datePart}{issuerPart}{consecutive}{situationPart}{securityCode}";
    }

    private static string GenerateSecurityCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        var value = BitConverter.ToUInt32(bytes, 0) % 100000000;
        return value.ToString("00000000");
    }
}
