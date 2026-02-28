using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Extensions.Options;
using MiniERP.Application.Contracts;

namespace MiniERP.Infrastructure.Services;

public class ElectronicDocumentValidator : IElectronicDocumentValidator
{
    private readonly HaciendaOptions _options;

    public ElectronicDocumentValidator(IOptions<HaciendaOptions> options)
    {
        _options = options.Value;
    }

    public XmlValidationResult Validate(string documentType, string xmlContent)
    {
        if (!_options.ValidateXml)
        {
            return new XmlValidationResult(true, Array.Empty<string>());
        }

        var schemaPath = ResolveSchemaPath(documentType);
        if (string.IsNullOrWhiteSpace(schemaPath) || !File.Exists(schemaPath))
        {
            return new XmlValidationResult(false, new[] { $"XSD not found for document type {documentType}." });
        }

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema
        };

        var schemas = new XmlSchemaSet();
        schemas.Add(null, schemaPath);
        settings.Schemas = schemas;

        var errors = new List<string>();
        settings.ValidationEventHandler += (_, args) =>
        {
            if (args.Exception != null)
            {
                errors.Add($"{args.Severity}: {args.Exception.Message}");
            }
            else
            {
                errors.Add($"{args.Severity}: {args.Message}");
            }
        };

        using var stringReader = new StringReader(xmlContent);
        using var reader = XmlReader.Create(stringReader, settings);
        try
        {
            while (reader.Read())
            {
                // Read through to validate.
            }
        }
        catch (XmlException ex)
        {
            errors.Add($"XML error: {ex.Message}");
        }

        return new XmlValidationResult(errors.Count == 0, errors);
    }

    private string? ResolveSchemaPath(string documentType)
    {
        var directory = _options.SchemaDirectory;
        var fileName = documentType switch
        {
            "01" => string.IsNullOrWhiteSpace(_options.FacturaXsdFile) ? "facturaElectronica.xsd" : _options.FacturaXsdFile,
            "03" => string.IsNullOrWhiteSpace(_options.NotaCreditoXsdFile) ? "notaCreditoElectronica.xsd" : _options.NotaCreditoXsdFile,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return Path.Combine(directory, fileName);
    }
}
