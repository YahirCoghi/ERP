using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using MiniERP.Domain.Entities.Common;
using MiniERP.Domain.Entities.Sales;

namespace MiniERP.Infrastructure.Services;

public class ElectronicInvoiceXmlBuilder
{
    private static readonly XNamespace FacturaNs = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica";
    private static readonly XNamespace NotaCreditoNs = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica";
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

    public string BuildFacturaXml(Invoice invoice, CompanyProfile company, DateTime issueDate, string key, string consecutive, HaciendaOptions options)
    {
        var root = BuildCommonRoot(FacturaNs, "FacturaElectronica", invoice, company, issueDate, key, consecutive, includeCondicionVenta: true, options);

        var detalle = BuildDetalleServicio(FacturaNs, invoice, options, out var summary);
        root.Add(detalle);
        root.Add(BuildResumen(FacturaNs, invoice, summary));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        return doc.ToString(SaveOptions.DisableFormatting);
    }

    public string BuildNotaCreditoXml(Invoice invoice, CompanyProfile company, DateTime issueDate, string key, string consecutive, HaciendaOptions options)
    {
        if (string.IsNullOrWhiteSpace(invoice.ReferenceDocumentType) ||
            string.IsNullOrWhiteSpace(invoice.ReferenceNumber) ||
            string.IsNullOrWhiteSpace(invoice.ReferenceCode) ||
            string.IsNullOrWhiteSpace(invoice.ReferenceReason) ||
            !invoice.ReferenceDate.HasValue)
        {
            throw new InvalidOperationException("Credit note requires reference document data.");
        }

        var root = BuildCommonRoot(NotaCreditoNs, "NotaCreditoElectronica", invoice, company, issueDate, key, consecutive, includeCondicionVenta: false, options);
        root.Add(BuildInformacionReferencia(NotaCreditoNs, invoice));

        var detalle = BuildDetalleServicio(NotaCreditoNs, invoice, options, out var summary);
        root.Add(detalle);
        root.Add(BuildResumen(NotaCreditoNs, invoice, summary));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static XElement BuildCommonRoot(
        XNamespace ns,
        string rootName,
        Invoice invoice,
        CompanyProfile company,
        DateTime issueDate,
        string key,
        string consecutive,
        bool includeCondicionVenta,
        HaciendaOptions options)
    {
        if (invoice.Customer == null)
        {
            throw new InvalidOperationException("Invoice must include customer data.");
        }

        if (string.IsNullOrWhiteSpace(company.EconomicActivityCode))
        {
            throw new InvalidOperationException("Company economic activity code is required.");
        }

        var root = new XElement(ns + rootName,
            new XAttribute("xmlns", ns),
            new XAttribute(XNamespace.Xmlns + "xsi", Xsi));

        root.Add(new XElement(ns + "Clave", key));
        root.Add(new XElement(ns + "CodigoActividad", company.EconomicActivityCode));
        root.Add(new XElement(ns + "NumeroConsecutivo", consecutive));
        root.Add(new XElement(ns + "FechaEmision", FormatDate(issueDate)));
        root.Add(BuildEmisor(ns, company));
        root.Add(BuildReceptor(ns, invoice.Customer, options));

        if (includeCondicionVenta)
        {
            root.Add(new XElement(ns + "CondicionVenta", invoice.SaleCondition));

            if (invoice.SaleCondition == "02" && invoice.DueDate.HasValue)
            {
                var days = Math.Max(0, (invoice.DueDate.Value.Date - invoice.InvoiceDate.Date).Days);
                root.Add(new XElement(ns + "PlazoCredito", days.ToString(CultureInfo.InvariantCulture)));
            }
        }

        return root;
    }

    private static XElement BuildEmisor(XNamespace ns, CompanyProfile company)
    {
        var emisor = new XElement(ns + "Emisor",
            new XElement(ns + "Nombre", company.LegalName),
            new XElement(ns + "Identificacion",
                new XElement(ns + "Tipo", HaciendaMappings.ToIdentificationCode(company.IdentificationType)),
                new XElement(ns + "Numero", HaciendaMappings.NormalizeIdentification(company.IdentificationNumber))));

        if (!string.IsNullOrWhiteSpace(company.TradeName))
        {
            emisor.Add(new XElement(ns + "NombreComercial", company.TradeName));
        }

        var ubicacion = BuildUbicacion(company.Province, company.Canton, company.District, company.Address);
        if (ubicacion != null)
        {
            emisor.Add(ReplaceNamespace(ubicacion, ns));
        }

        var telefono = BuildTelefono(company.Phone);
        if (telefono != null)
        {
            emisor.Add(ReplaceNamespace(telefono, ns));
        }

        if (!string.IsNullOrWhiteSpace(company.Email))
        {
            emisor.Add(new XElement(ns + "CorreoElectronico", company.Email));
        }

        return emisor;
    }

    private static XElement BuildReceptor(XNamespace ns, Customer customer, HaciendaOptions options)
    {
        var receptorId = HaciendaMappings.NormalizeIdentification(customer.IdentificationNumber);
        if (string.IsNullOrWhiteSpace(receptorId))
        {
            throw new InvalidOperationException("Customer identification is required for electronic invoice.");
        }

        var receptor = new XElement(ns + "Receptor",
            new XElement(ns + "Nombre", customer.Name),
            new XElement(ns + "Identificacion",
                new XElement(ns + "Tipo", HaciendaMappings.ToIdentificationCode(customer.IdentificationType)),
                new XElement(ns + "Numero", receptorId)));

        if (!string.IsNullOrWhiteSpace(customer.EconomicActivityCode))
        {
            receptor.Add(new XElement(ns + "CodigoActividad", customer.EconomicActivityCode));
        }
        else if (options.RequireReceptorActivity)
        {
            throw new InvalidOperationException("Customer economic activity code is required.");
        }

        var ubicacion = BuildUbicacion(null, null, null, customer.Address);
        if (ubicacion != null)
        {
            receptor.Add(ReplaceNamespace(ubicacion, ns));
        }

        var telefono = BuildTelefono(customer.Phone);
        if (telefono != null)
        {
            receptor.Add(ReplaceNamespace(telefono, ns));
        }

        if (!string.IsNullOrWhiteSpace(customer.Email))
        {
            receptor.Add(new XElement(ns + "CorreoElectronico", customer.Email));
        }

        return receptor;
    }

    private static XElement BuildResumen(XNamespace ns, Invoice invoice, SummaryTotals summary)
    {
        var resumen = new XElement(ns + "ResumenFactura");

        resumen.Add(new XElement(ns + "TotalServGravados", HaciendaMappings.FormatMoney(summary.TotalServGravados)));
        resumen.Add(new XElement(ns + "TotalServExentos", HaciendaMappings.FormatMoney(summary.TotalServExentos)));
        resumen.Add(new XElement(ns + "TotalMercanciasGravadas", HaciendaMappings.FormatMoney(summary.TotalMercanciasGravadas)));
        resumen.Add(new XElement(ns + "TotalMercanciasExentas", HaciendaMappings.FormatMoney(summary.TotalMercanciasExentas)));
        resumen.Add(new XElement(ns + "TotalGravado", HaciendaMappings.FormatMoney(summary.TotalGravado)));
        resumen.Add(new XElement(ns + "TotalExento", HaciendaMappings.FormatMoney(summary.TotalExento)));
        resumen.Add(new XElement(ns + "TotalVenta", HaciendaMappings.FormatMoney(summary.TotalVenta)));
        resumen.Add(new XElement(ns + "TotalDescuentos", HaciendaMappings.FormatMoney(summary.TotalDescuentos)));
        resumen.Add(new XElement(ns + "TotalVentaNeta", HaciendaMappings.FormatMoney(summary.TotalVentaNeta)));
        resumen.Add(new XElement(ns + "TotalImpuesto", HaciendaMappings.FormatMoney(summary.TotalImpuesto)));
        resumen.Add(new XElement(ns + "TotalComprobante", HaciendaMappings.FormatMoney(summary.TotalComprobante)));

        if (!string.IsNullOrWhiteSpace(invoice.PaymentMethod))
        {
            resumen.Add(new XElement(ns + "MedioPago", invoice.PaymentMethod));
        }

        var currencyCode = HaciendaMappings.ToCurrencyCode(invoice.Currency);
        if (currencyCode != "CRC" || invoice.ExchangeRate != 1m)
        {
            resumen.Add(new XElement(ns + "CodigoTipoMoneda",
                new XElement(ns + "CodigoMoneda", currencyCode),
                new XElement(ns + "TipoCambio", HaciendaMappings.FormatMoney(invoice.ExchangeRate))));
        }

        return resumen;
    }

    private static string FormatDate(DateTime date)
    {
        var utc = date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
        return utc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }

    private static XElement? BuildUbicacion(string? provincia, string? canton, string? distrito, string? otrasSenas)
    {
        if (string.IsNullOrWhiteSpace(provincia) &&
            string.IsNullOrWhiteSpace(canton) &&
            string.IsNullOrWhiteSpace(distrito) &&
            string.IsNullOrWhiteSpace(otrasSenas))
        {
            return null;
        }

        var ubicacion = new XElement(FacturaNs + "Ubicacion");

        if (!string.IsNullOrWhiteSpace(provincia))
        {
            ubicacion.Add(new XElement(FacturaNs + "Provincia", provincia));
        }

        if (!string.IsNullOrWhiteSpace(canton))
        {
            ubicacion.Add(new XElement(FacturaNs + "Canton", canton));
        }

        if (!string.IsNullOrWhiteSpace(distrito))
        {
            ubicacion.Add(new XElement(FacturaNs + "Distrito", distrito));
        }

        if (!string.IsNullOrWhiteSpace(otrasSenas))
        {
            ubicacion.Add(new XElement(FacturaNs + "OtrasSenas", otrasSenas));
        }

        return ubicacion;
    }

    private static XElement? BuildTelefono(string? phone)
    {
        var digits = HaciendaMappings.NormalizeIdentification(phone);
        if (string.IsNullOrWhiteSpace(digits) || digits.Length < 8)
        {
            return null;
        }

        return new XElement(FacturaNs + "Telefono",
            new XElement(FacturaNs + "CodigoPais", "506"),
            new XElement(FacturaNs + "NumTelefono", digits));
    }

    private static XElement BuildDetalleServicio(XNamespace ns, Invoice invoice, HaciendaOptions options, out SummaryTotals summary)
    {
        var detalle = new XElement(ns + "DetalleServicio");
        summary = new SummaryTotals();
        var lineNumber = 1;

        foreach (var line in invoice.Lines)
        {
            var product = line.Product;
            if (options.RequireCabys && string.IsNullOrWhiteSpace(product?.CabysCode))
            {
                throw new InvalidOperationException("CABYS code is required for each line.");
            }
            var quantity = line.Quantity;
            var unitPrice = line.UnitPrice;
            var lineTotal = quantity * unitPrice;
            var discount = line.Discount;
            var subtotal = lineTotal - discount;
            var taxRate = line.TaxRate > 0 ? line.TaxRate : (product?.TaxRate ?? 0m);
            if (product?.IsTaxExempt == true)
            {
                taxRate = 0m;
            }
            var taxAmount = line.TaxAmount > 0 ? line.TaxAmount : subtotal * taxRate / 100m;
            var isTaxed = taxRate > 0m;
            var totalLine = subtotal + taxAmount;
            var isService = IsService(product);

            summary.AddLine(isService, isTaxed, lineTotal, discount, taxAmount);

            var lineElement = new XElement(ns + "LineaDetalle",
                new XElement(ns + "NumeroLinea", lineNumber.ToString(CultureInfo.InvariantCulture)));

            if (!string.IsNullOrWhiteSpace(product?.CabysCode))
            {
                lineElement.Add(new XElement(ns + "CodigoCabys", product.CabysCode));
            }

            lineElement.Add(new XElement(ns + "Cantidad", quantity.ToString("0.####", CultureInfo.InvariantCulture)));
            lineElement.Add(new XElement(ns + "UnidadMedida", string.IsNullOrWhiteSpace(product?.UnitMeasureCode) ? "Unid" : product.UnitMeasureCode));
            lineElement.Add(new XElement(ns + "Detalle", product?.Name ?? "Producto"));
            lineElement.Add(new XElement(ns + "PrecioUnitario", HaciendaMappings.FormatMoney(unitPrice)));
            lineElement.Add(new XElement(ns + "MontoTotal", HaciendaMappings.FormatMoney(lineTotal)));
            lineElement.Add(new XElement(ns + "MontoDescuento", HaciendaMappings.FormatMoney(discount)));

            if (discount > 0)
            {
                lineElement.Add(new XElement(ns + "NaturalezaDescuento", "Descuento"));
            }

            lineElement.Add(new XElement(ns + "SubTotal", HaciendaMappings.FormatMoney(subtotal)));

            if (isTaxed)
            {
                var impuesto = new XElement(ns + "Impuesto",
                    new XElement(ns + "Codigo", "01"),
                    new XElement(ns + "Tarifa", taxRate.ToString("0.##", CultureInfo.InvariantCulture)),
                    new XElement(ns + "Monto", HaciendaMappings.FormatMoney(taxAmount)));
                lineElement.Add(impuesto);
            }

            lineElement.Add(new XElement(ns + "MontoTotalLinea", HaciendaMappings.FormatMoney(totalLine)));
            detalle.Add(lineElement);
            lineNumber++;
        }

        return detalle;
    }

    private static XElement BuildInformacionReferencia(XNamespace ns, Invoice invoice)
    {
        return new XElement(ns + "InformacionReferencia",
            new XElement(ns + "TipoDoc", invoice.ReferenceDocumentType),
            new XElement(ns + "Numero", invoice.ReferenceNumber),
            new XElement(ns + "FechaEmision", FormatDate(invoice.ReferenceDate!.Value)),
            new XElement(ns + "Codigo", invoice.ReferenceCode),
            new XElement(ns + "Razon", invoice.ReferenceReason));
    }

    private static XElement ReplaceNamespace(XElement element, XNamespace ns)
    {
        var replaced = new XElement(ns + element.Name.LocalName, element.Attributes(), element.Nodes().Select(node =>
        {
            if (node is XElement child)
            {
                return ReplaceNamespace(child, ns);
            }
            return node;
        }));

        return replaced;
    }

    private static bool IsService(MiniERP.Domain.Entities.Inventory.Product? product)
    {
        if (product == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(product.Unit) && product.Unit.StartsWith("SERV", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(product.Category) && product.Category.StartsWith("SERV", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private sealed class SummaryTotals
    {
        public decimal TotalServGravados { get; private set; }
        public decimal TotalServExentos { get; private set; }
        public decimal TotalMercanciasGravadas { get; private set; }
        public decimal TotalMercanciasExentas { get; private set; }
        public decimal TotalVenta { get; private set; }
        public decimal TotalDescuentos { get; private set; }
        public decimal TotalImpuesto { get; private set; }
        public decimal TotalGravado => TotalServGravados + TotalMercanciasGravadas;
        public decimal TotalExento => TotalServExentos + TotalMercanciasExentas;
        public decimal TotalVentaNeta => TotalVenta - TotalDescuentos;
        public decimal TotalComprobante => TotalVentaNeta + TotalImpuesto;

        public void AddLine(bool isService, bool isTaxed, decimal lineTotal, decimal discount, decimal taxAmount)
        {
            TotalVenta += lineTotal;
            TotalDescuentos += discount;
            TotalImpuesto += taxAmount;

            if (isService)
            {
                if (isTaxed)
                {
                    TotalServGravados += lineTotal;
                }
                else
                {
                    TotalServExentos += lineTotal;
                }
            }
            else
            {
                if (isTaxed)
                {
                    TotalMercanciasGravadas += lineTotal;
                }
                else
                {
                    TotalMercanciasExentas += lineTotal;
                }
            }
        }
    }
}
