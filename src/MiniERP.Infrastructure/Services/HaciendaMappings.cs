using System;
using System.Globalization;
using System.Linq;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Infrastructure.Services;

public static class HaciendaMappings
{
    public static string ToIdentificationCode(IdentificationType? type)
    {
        return type switch
        {
            IdentificationType.NationalId => "01",
            IdentificationType.LegalEntity => "02",
            IdentificationType.Dimex => "03",
            IdentificationType.Nite => "04",
            IdentificationType.Foreign => "05",
            IdentificationType.NonTaxpayer => "06",
            _ => "01"
        };
    }

    public static string NormalizeIdentification(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return digits;
    }

    public static string FormatIssuerIdForKey(string? raw)
    {
        var digits = NormalizeIdentification(raw);
        if (digits.Length == 0)
        {
            throw new InvalidOperationException("Issuer identification is required for key generation.");
        }
        if (digits.Length > 12)
        {
            throw new InvalidOperationException("Issuer identification exceeds 12 digits required for key generation.");
        }

        return digits.PadLeft(12, '0');
    }

    public static string ToCurrencyCode(Currency currency)
    {
        return currency switch
        {
            Currency.CRC => "CRC",
            Currency.USD => "USD",
            Currency.EUR => "EUR",
            Currency.MXN => "MXN",
            Currency.COP => "COP",
            Currency.ARS => "ARS",
            _ => "CRC"
        };
    }

    public static string FormatMoney(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
