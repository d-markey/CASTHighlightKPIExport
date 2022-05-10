// HighlightKPIExport
// Copyright (C) 2020-2022 David MARKEY

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.


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

        // récupération des credentials
        public async Task<Credential> GetCredential(ICredentialConfig credConfig) {
            // token en priorité
            var token = credConfig.Token;
            // si le token correspond à un nom de fichier, lire le token à partir du fichier
            if (string.IsNullOrWhiteSpace(token)) {
                if (File.Exists(token)) {
                    // récupération du token à partir du fichier
                    _logger.Log($"Loading token from file {token}");
                    using (var stream = new StreamReader(token)) {
                        token = (await stream.ReadToEndAsync()).Trim();
                    }
                }
                if (!String.IsNullOrWhiteSpace(token)) {
                    return new Credential(token);
                }
            }

            // sinon, userid/mot de passe
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
            return new Credential(new NetworkCredential(userId, password));
        }
    }
}