namespace PskOnline.Client.Api
{
  using System;
  using System.Security;
  using PskOnline.Client.Api.OpenId;

  /// <summary>
  /// Holds information about OpenID tokens
  /// </summary>
  public class TokenHolder
  {
    /// <summary>
    /// Creates the tokenInfoResponse
    /// </summary>
    /// <param name="tokenInfo"></param>
    /// <param name="scopes"></param>
    public TokenHolder(TokenInfoResponse tokenInfo, string scopes)
    {
      AccessToken = tokenInfo.access_token.ToSecureString();
      IdToken = tokenInfo.id_token;
      NotValidAfterUtc = DateTime.UtcNow + TimeSpan.FromSeconds(tokenInfo.expires_in - 10);
      Scopes = scopes;

      if (!string.IsNullOrEmpty(tokenInfo.refresh_token))
      {
        // only update the stored refresh token, if it was returned
        // if it wasn't, preserve the current refresh token
        RefreshToken = tokenInfo.refresh_token.ToSecureString();
      }
    }

    /// <summary>
    /// A UTC timestamp when the <see cref="AccessToken"/> expires
    /// When set via the constructor, an offset of a few seconds is applied
    /// </summary>
    public DateTime NotValidAfterUtc { get; set; }

    /// <summary>
    /// Gets or sets the id_token returned by the server upon successful authentication
    /// </summary>
    public string IdToken { get; set; }

    /// <summary>
    /// Gets or sets the access_token returned by the server upon successful authentication
    /// </summary>
    public SecureString AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh_token returned by the server upon successful authentication
    /// </summary>
    public SecureString RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the scopes requested by the client
    /// </summary>
    public string Scopes { get; set; }
  }
}
