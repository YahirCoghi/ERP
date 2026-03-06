using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MiniERP.Application.Contracts;

namespace MiniERP.Infrastructure.Services;

public class HaciendaClient : IHaciendaClient
{
    private readonly HaciendaOptions _options;
    private readonly HttpClient _http;

    public HaciendaClient(HttpClient httpClient, IOptions<HaciendaOptions> options)
    {
        _http = httpClient;
        _options = options.Value;
    }

    public async Task<HaciendaSubmissionResult> SubmitAsync(HaciendaSubmissionRequest request)
    {
        if (!_options.Enabled)
        {
            return new HaciendaSubmissionResult(
                "NotConfigured",
                "Hacienda integration is disabled. Enable Hacienda:Enabled and configure credentials.",
                null,
                null);
        }

        if (string.IsNullOrWhiteSpace(_options.ReceptionUrl) || string.IsNullOrWhiteSpace(_options.TokenUrl))
        {
            return new HaciendaSubmissionResult(
                "MissingConfiguration",
                "Hacienda configuration is incomplete.",
                null,
                null);
        }

        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new HaciendaSubmissionResult(
                "AuthError",
                "Unable to obtain Hacienda access token.",
                null,
                null);
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new Dictionary<string, object?>
        {
            ["clave"] = request.Key,
            ["fecha"] = request.IssueDate.ToString("yyyy-MM-ddTHH:mm:ssK"),
            ["emisor"] = new Dictionary<string, string>
            {
                ["tipoIdentificacion"] = request.EmisorTipo,
                ["numeroIdentificacion"] = request.EmisorNumero
            },
            ["comprobanteXml"] = request.SignedXmlBase64
        };

        if (!string.IsNullOrWhiteSpace(request.ReceptorTipo) && !string.IsNullOrWhiteSpace(request.ReceptorNumero))
        {
            payload["receptor"] = new Dictionary<string, string>
            {
                ["tipoIdentificacion"] = request.ReceptorTipo,
                ["numeroIdentificacion"] = request.ReceptorNumero
            };
        }

        if (!string.IsNullOrWhiteSpace(request.CallbackUrl))
        {
            payload["callbackUrl"] = request.CallbackUrl;
        }
        else if (!string.IsNullOrWhiteSpace(_options.CallbackUrl))
        {
            payload["callbackUrl"] = _options.CallbackUrl;
        }

        var json = JsonSerializer.Serialize(payload);
        var response = await _http.PostAsync(_options.ReceptionUrl, new StringContent(json, Encoding.UTF8, "application/json"));
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new HaciendaSubmissionResult(
                "Error",
                $"Hacienda responded with {response.StatusCode}",
                null,
                body);
        }

        var haciendaStatus = TryReadStatus(body);
        var location = response.Headers.Location?.ToString();
        var message = string.IsNullOrWhiteSpace(location) ? "Submitted" : $"Submitted. Location: {location}";

        return new HaciendaSubmissionResult("Submitted", message, haciendaStatus, body);
    }

    public async Task<HaciendaSubmissionResult> QueryStatusAsync(string key)
    {
        if (!_options.Enabled)
        {
            return new HaciendaSubmissionResult(
                "NotConfigured",
                "Hacienda integration is disabled.",
                null,
                null);
        }

        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new HaciendaSubmissionResult(
                "AuthError",
                "Unable to obtain Hacienda access token.",
                null,
                null);
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var statusUrl = $"{_options.ReceptionUrl.TrimEnd('/')}/{key}";
        var response = await _http.GetAsync(statusUrl);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new HaciendaSubmissionResult(
                "Error",
                $"Hacienda responded with {response.StatusCode}",
                null,
                body);
        }

        var haciendaStatus = TryReadStatus(body);
        return new HaciendaSubmissionResult("Status", "Status retrieved", haciendaStatus, body);
    }

    private async Task<string?> GetTokenAsync()
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _options.ClientId,
            ["username"] = _options.Username,
            ["password"] = _options.Password
        };

        if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            form["client_secret"] = _options.ClientSecret;
        }

        var response = await _http.PostAsync(_options.TokenUrl, new FormUrlEncodedContent(form));
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("access_token", out var token))
            {
                return token.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string? TryReadStatus(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("ind-estado", out var status))
            {
                return status.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
