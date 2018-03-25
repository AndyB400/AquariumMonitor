namespace AquariumMonitor.BusinessLogic
{
    public class Constants
    {
        // Tokens
        public const string ValidIssuer = "Tokens:Issuer";
        public const string ValidAudience = "Tokens:Audience";
        public const string TokenDurationMinutes = "Tokens:ValidForDurationMins";
        public const string TokenSecurityKey = "AquariumAPITokenSecurityKey";

        // Cors
        public const string CorsPolicyName = "Cors:PolicyName";
        public const string CorsOrigins = "Cors:Origins";

        // API 
        public const string APIVersionHeaderName = "x-api-version";

        // DB Connection
        public const string DBConnectionName = "AquariumDB";

        // Logging
        public const string SerilogTableName = "Logs";
    }
}
