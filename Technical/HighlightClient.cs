using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; 

namespace HighlightKPIExport.Technical {
    // proxy pour les API Highlight
    public class HighlightClient {
        public static HighlightClient Build(Args args) {
            var client = new HighlightClient();
            var url = args.Url.Value.Trim();
            if (url.EndsWith("/")) {
                url = url.Substring(0, url.Length - 1);
            }
            if (string.IsNullOrWhiteSpace(url)) {
                Logger.Log(args, "URL is empty");
                return null;
            }
            client.BaseUrl = url;
            return client;
        }

        private HighlightClient() {
        }

        public string BaseUrl { get; private set; }

        private string _auth = "";
        
        private NetworkCredential _cred;
        public NetworkCredential Credential {
            set {
                _cred = value;
                if (_cred == null) {
                    _auth = "";
                } else {
                    var userAndPwd = Encoding.UTF8.GetBytes(_cred.UserName + ":" + _cred.Password);
                    _auth = "Basic " + Convert.ToBase64String(userAndPwd);
                }
            } 
            get {
                return _cred;
            } 
        }

        // appel d'une API Highlight
        private async Task<string> LoadResourceAsync(string resourceUri) {
            var req = HttpWebRequest.Create($"{BaseUrl}{resourceUri}");
            req.Headers.Add("Accept", "application/json");
            req.Headers.Add("Authorization", _auth);
            var resp = await req.GetResponseAsync();
            string json;
            using (var stream = new StreamReader(resp.GetResponseStream())) {
                json = await stream.ReadToEndAsync();
            }
            return json;
        }

        // appel de l'API /domains/{domainId}/applications/{appId}
        public async Task<T> GetAppInfoForApp<T>(string domainId, string appId) {
            var json = await LoadResourceAsync($"/WS2/domains/{domainId}/applications/{appId}");
            return JsonConvert.DeserializeObject<T>(json);
        }

        // appel de l'API /domains/{domainId}/applications
        public async Task<IList<T>> GetAppIdsForDomain<T>(string domainId) {
            var json = await LoadResourceAsync($"/WS2/domains/{domainId}/applications");
            return JsonConvert.DeserializeObject<IList<T>>(json);
        }
 
        // appel de l'API /WS/company/{companyId}/audit
        public async Task<T> GetAuditForCompany<T>(string companyId) {
            var json = await LoadResourceAsync($"/WS/company/{companyId}/audit");
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}