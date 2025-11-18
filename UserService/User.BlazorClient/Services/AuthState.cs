namespace User.BlazorClient.Services;

public class AuthState
{
    public string? UserName { get; private set; }
    public string? Token { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public void SetAuth(string userName, string token)
    {
        // պահում ենք առանց "Bearer " prefix-ի
        if (!string.IsNullOrWhiteSpace(token) &&
            token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring("Bearer ".Length).Trim();
        }

        UserName = userName;
        Token = token;
    }

    public void Logout()
    {
        UserName = null;
        Token = null;
    }

}
