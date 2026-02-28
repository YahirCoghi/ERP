using System.Collections.Generic;

namespace MiniERP.Application.Contracts;

public record XmlValidationResult(bool IsValid, IReadOnlyList<string> Errors);

public interface IElectronicDocumentValidator
{
    XmlValidationResult Validate(string documentType, string xmlContent);
}
