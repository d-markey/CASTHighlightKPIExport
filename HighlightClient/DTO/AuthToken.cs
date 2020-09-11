namespace HighlightKPIExport.Client.DTO {
    public class AuthToken {
        public int ExpiresInMin { get; protected set; }
        public string Token { get; protected set; }
    }
}