using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace oauth2_client
{
    public class OAuthConfig
    {   
        private static OAuthConfig instance = null;
        private static ConfigurationManager<OpenIdConnectConfiguration> configurationManager;
        private static string issuer;
        private OAuthConfig(){
            // Replace with your authorization server URL:
            issuer = "http://cassandra:8080/auth/realms/demo";
                
            HttpDocumentRetriever httpDocumentRetriever = new HttpDocumentRetriever();
            httpDocumentRetriever.RequireHttps = false;
            configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            issuer + "/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            httpDocumentRetriever);
        }
        public static OAuthConfig getInstance(){
            if(instance != null){
                return instance;
            }
            return new OAuthConfig();
        }

        public ConfigurationManager<OpenIdConnectConfiguration> getConfig(){
            return configurationManager;
        }

        public string getIssuer(){
            return issuer;
        }
    }
}