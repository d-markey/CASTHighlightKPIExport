namespace HighlightKPIExport.Technical {
    public interface ICredentialConfig {
        string CredentialFileName { get; }
        string UserId { get; }
        string Password { get; }
    }
}