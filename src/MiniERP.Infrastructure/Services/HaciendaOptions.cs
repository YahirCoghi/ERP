namespace MiniERP.Infrastructure.Services;

public class HaciendaOptions
{
    public bool Enabled { get; set; }
    public string TokenUrl { get; set; } = string.Empty;
    public string ReceptionUrl { get; set; } = string.Empty;
    public string? CallbackUrl { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool ValidateXml { get; set; }
    public string? SchemaDirectory { get; set; }
    public string? FacturaXsdFile { get; set; }
    public string? NotaCreditoXsdFile { get; set; }
    public bool RequireCabys { get; set; }
    public bool RequireReceptorActivity { get; set; }
}
