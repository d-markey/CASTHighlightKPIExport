using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace HighlightKPIExport.Technical {
    public class CredentialService {

        public CredentialService(Args args) {
            Args = args;
        }

        public Args Args { get; private set; }

        // récupération des credentials (userid/mot de passe)
        public async Task<NetworkCredential> GetCredential() {
            var userId = "";
            var password = "";
            // l'option "--credentials" a priorité
            var credentialFileName = Args.Credentials.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(credentialFileName)) {
                // fichier contenant les credentials sous la forme "userid:password"
                if (File.Exists(credentialFileName)) {
                    Logger.Log(Args, $"Loading credentials from file {credentialFileName}");
                    using (var stream = new StreamReader(password)) {
                        var parts = (await stream.ReadToEndAsync()).Split(':');
                        // récupération de l'identifiant
                        userId = parts.Length > 0 ? parts[0].Trim() : "";
                        // récupération du mot de passe
                        password = parts.Length > 1 ? parts[1].Trim() : "";
                    }
                } else {
                    Logger.Log(Args, $"Credentials file {credentialFileName} not found");
                }
            }
            if (string.IsNullOrWhiteSpace(userId)) {
                // récupération de l'identifiant
                userId = Args.User.Value?.Trim();
            }
            if (string.IsNullOrWhiteSpace(password)) {
                // récupération du mot de passe
                password = Args.Password.Value?.Trim();
                if (File.Exists(password)) {
                    // récupération du mot de passe à partir du fchier
                    Logger.Log(Args, $"Loading password from file {password}");
                    using (var stream = new StreamReader(password)) {
                        password = (await stream.ReadToEndAsync()).Trim();
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password)) {
                Logger.Log(Args, $"Missing credentials");
            }
            return new NetworkCredential(userId, password);
        }
    }
}