using System.Net.Http.Headers;
using Azure.Core;

namespace OpenAISearchScenarios;

public class TokenCredentialsHttpHandler : DelegatingHandler
{
    private readonly TokenCredential _credential;
    private readonly string[] _scopes;
    private AccessToken? _token;

    public TokenCredentialsHttpHandler(TokenCredential credential, params string[] scopes) 
        : this(credential, scopes, null)
    {
    }

    public TokenCredentialsHttpHandler(TokenCredential credential, string[] scopes, HttpMessageHandler? innerHandler = null)
        : base(innerHandler ?? new HttpClientHandler())
    {
        _credential = credential;
        _scopes = scopes;
        _token = null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // If having an extra active component is not an issue, a nice addition to testing for expiration here
        // is to have a timer that refreshes the token a few minutes before its expiration, so no caller has to wait
        if (_token == null || _token.Value.ExpiresOn < DateTimeOffset.UtcNow)
        {
            _token = await _credential.GetTokenAsync(new TokenRequestContext(_scopes), cancellationToken);
        }
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.Value.Token);
        return await base.SendAsync(request, cancellationToken);
    }
}
