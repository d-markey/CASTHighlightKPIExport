using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HighlightKPIExport.Technical {
    public class CredentialService {

        public CredentialService(ILogger logger) {
            _logger = logger;
        }

        private ILogger _logger;

        // récupération des credentials (userid/mot de passe)
        public async Task<NetworkCredential> GetCredential(ICredentialConfig credConfig) {
            var userId = string.Empty;
            var password = string.Empty;
            // les infos du fichier ont priorité, s'il est spécifié
            var credentialFileName = credConfig.CredentialFileName?.Trim();
            if (!string.IsNullOrWhiteSpace(credentialFileName)) {
                // fichier contenant les credentials sous la forme "userid:password"
                if (File.Exists(credentialFileName)) {
                    _logger.Log($"Loading credentials from file {credentialFileName}");
                    using (var stream = new StreamReader(password)) {
                        var parts = (await stream.ReadToEndAsync()).Split(':');
                        // récupération de l'identifiant
                        var mail = new MailAddress(parts.Length > 0 ? parts[0].Trim() : "");
                        userId = mail.Address;
                        // récupération du mot de passe
                        password = parts.Length > 1 ? parts[1].Trim() : "";
                    }
                } else {
                    _logger.Log($"Credentials file {credentialFileName} not found");
                }
            }
            if (string.IsNullOrWhiteSpace(userId)) {
                // récupération de l'identifiant
                userId = credConfig.UserId;
            }
            if (string.IsNullOrWhiteSpace(password)) {
                // récupération du mot de passe
                password = credConfig.Password;
                if (File.Exists(password)) {
                    // récupération du mot de passe à partir du fichier
                    _logger.Log($"Loading password from file {password}");
                    using (var stream = new StreamReader(password)) {
                        password = (await stream.ReadToEndAsync()).Trim();
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password)) {
                throw new UnauthorizedAccessException($"Missing credentials");
            }
            return new NetworkCredential(userId, password);
        }
    }
}