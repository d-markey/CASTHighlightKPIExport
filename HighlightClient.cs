using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; 

namespace HighlightKPIExport {
    // proxy pour les API Highlight
    public class HighlightClient {
        public HighlightClient(string baseUrl) {
            BaseUrl = baseUrl?.Trim() ?? "";
            if (BaseUrl.Length == 0) {
                BaseUrl = DefaultBaseUrl;
            } else if (baseUrl.EndsWith("/")) {
                BaseUrl = BaseUrl.Substring(0, BaseUrl.Length - 1);
            }
        }

        public const string DefaultBaseUrl = "https://rpa.casthighlight.com";

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
            var req = HttpWebRequest.Create($"{BaseUrl}/WS2/{resourceUri}");
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
        public async Task<HighlightAppInfo> GetAppInfoForApp(string domainId, string appId) {
            var json = await LoadResourceAsync($"/domains/{domainId}/applications/{appId}");
            var app = JsonConvert.DeserializeObject<HighlightAppInfo>(json);
            app.Url = $"{BaseUrl}/#Explore/Applications/{app.Id}/Detail";
            return app;
        }

        // appel de l'API /domains/{domainId}/applications
        public async Task<IList<HighlightAppId>> GetAppIdsForDomain(string domainId) {
            var json = await LoadResourceAsync($"/domains/{domainId}/applications");
            var apps = JsonConvert.DeserializeObject<IList<HighlightAppId>>(json);
            foreach (var app in apps) {
                app.DomainId = domainId;
            }
            return apps;
        }
    }
}