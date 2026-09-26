using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ulgtArchipelagoPull;

/// <summary>
/// Manages Auth0 machine-to-machine token retrieval and automatic renewal
/// </summary>
public class Auth0TokenManager(
    string domain, 
    string clientId, 
    string clientSecret, 
    string audience) : IDisposable
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _domain = domain ?? throw new ArgumentNullException(nameof(domain));
    private readonly string _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
    private readonly string _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
    private readonly string _audience = audience ?? throw new ArgumentNullException(nameof(audience));
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

    private string _currentToken = string.Empty;
    private DateTime _tokenExpiry;
    private bool _disposed;

    /// <summary>
    /// Gets a valid access token, refreshing if necessary
    /// </summary>
    /// <returns>Valid access token</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        await _tokenSemaphore.WaitAsync();
        try
        {
            // Check if we have a valid token that hasn't expired (with 30 second buffer)
            if (!string.IsNullOrEmpty(_currentToken) && DateTime.UtcNow < _tokenExpiry.AddSeconds(-30))
            {
                return _currentToken;
            }

            // Token is expired or doesn't exist, get a new one
            await RefreshTokenAsync();
            return _currentToken;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    /// <summary>
    /// Forces a token refresh
    /// </summary>
    public async Task RefreshTokenAsync()
    {
        var tokenRequest = new
        {
            client_id = _clientId,
            client_secret = _clientSecret,
            audience = _audience,
            grant_type = "client_credentials"
        };

        var json = JsonSerializer.Serialize(tokenRequest);           

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var tokenUrl = $"https://{_domain}/oauth/token";
        
        try
        {
            var response = await _httpClient.PostAsync(tokenUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            _currentToken = tokenResponse?.AccessToken ?? "";
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse?.ExpiresIn ?? 0);
        }
        catch (HttpRequestException ex)
        {
            throw new Auth0TokenException($"Failed to retrieve Auth0 token: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Auth0TokenException($"Failed to parse Auth0 token response: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if the current token is valid (not expired)
    /// </summary>
    public bool IsTokenValid()
    {
        return !string.IsNullOrEmpty(_currentToken) && DateTime.UtcNow < _tokenExpiry;
    }

    /// <summary>
    /// Gets the current token expiry time (UTC)
    /// </summary>
    public DateTime? GetTokenExpiry()
    {
        return string.IsNullOrEmpty(_currentToken) ? null : _tokenExpiry;
    }

    public void Dispose()
    {
        Dispose(_disposed);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _tokenSemaphore?.Dispose();
            _disposed = true;
        }
    }
   
    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; } = 0;
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}

/// <summary>
/// Exception thrown when Auth0 token operations fail
/// </summary>
public class Auth0TokenException : Exception
{
    public Auth0TokenException(string message) : base(message) { }
    public Auth0TokenException(string message, Exception innerException) : base(message, innerException) { }
}
