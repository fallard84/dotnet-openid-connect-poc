using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace oauth2_client {
    public class OAuthValidator {
        public static async Task<JwtSecurityToken> ValidateToken(
            string token,
            CancellationToken ct = default(CancellationToken))
        {
            OAuthConfig oAuthConfig = OAuthConfig.getInstance();
            String issuer = oAuthConfig.getIssuer();
            ConfigurationManager<OpenIdConnectConfiguration> configurationManager = oAuthConfig.getConfig();
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrEmpty(issuer)) throw new ArgumentNullException(nameof(issuer));

            var discoveryDocument = await configurationManager.GetConfigurationAsync(ct);
            var signingKeys = discoveryDocument.SigningKeys;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                // Allow for some drift in server time
                // (a lower value is better; we recommend five minutes or less)
                ClockSkew = TimeSpan.FromMinutes(5),
                // See additional validation for aud below
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out var rawValidatedToken);
                JwtSecurityToken validatedToken = (JwtSecurityToken)rawValidatedToken;
                ValidateClientId(validatedToken);
                return validatedToken;
            }
            catch (SecurityTokenValidationException e)
            {
                // Logging, etc.
                Console.Error.WriteLine(e.ToString());
                return null;
            }
        }
        private static void ValidateClientId(JwtSecurityToken token){
            // Validate client ID
            var expectedClientId = "test"; // This Application's Client ID
            var clientIdMatches = token.Payload.TryGetValue("cid", out var rawCid)
                && rawCid.ToString() == expectedClientId;

            if (!clientIdMatches)
            {
                throw new SecurityTokenValidationException("The cid claim was invalid.");
            }
        }
    }
}