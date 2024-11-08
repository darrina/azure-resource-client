using Azure.Core;

namespace AzureResourceClient;

public class JwtTokenCredential : TokenCredential
{
    private readonly string _token;

    public JwtTokenCredential(string token)
    {
        _token = token;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken(_token, DateTimeOffset.MaxValue);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.MaxValue));
    }
}
