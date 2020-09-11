using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json; 

using HighlightKPIExport.Client.DTO;

namespace HighlightKPIExport.Client {
    // proxy pour les API Highlight
    public class HighlightClient : IDisposable {
        public HighlightClient(Uri baseUrl) {
            BaseUrl = baseUrl;
        }

        public Uri BaseUrl { get; private set; }

        public void Dispose() {
            _auth = "";
            _token = null;
            _cred = null;
        }

        private string _auth = "";
        private AuthToken _token = null;        
        private NetworkCredential _cred = null;

        public async Task Authenticate(NetworkCredential credential) {
            _cred = credential;
            if (_cred == null) {
                _auth = "";
            } else {
                var userAndPwd = Encoding.UTF8.GetBytes(_cred.UserName + ":" + _cred.Password);
                _auth = "Basic " + Convert.ToBase64String(userAndPwd);
            }
            _token = await GetAuthToken();
        }

        // appel d'une API Highlight
        private async Task<string> LoadResourceAsync(string resourceUri) {
            if (!Uri.TryCreate(BaseUrl, resourceUri, out Uri uri)) throw new InvalidOperationException();
            var req = WebRequest.Create(uri);
            req.Headers.Add("Accept", "application/json");
            req.Headers.Add("Authorization", _auth);
            var resp = await req.GetResponseAsync();
            using (var stream = new StreamReader(resp.GetResponseStream())) {
                return await stream.ReadToEndAsync();
            }
        }

        // appel de l'API /WS2/domains/{domainId}/applications/{appId}
        public async Task<AppInfo> GetAppInfoForApp(string domainId, string appId) {
            var json = await LoadResourceAsync($"/WS2/domains/{domainId}/applications/{appId}");
            return JsonConvert.DeserializeObject<AppInfo>(json);
        }

        // appel de l'API /WS2/domains/{domainId}/applications
        public async Task<IList<AppId>> GetAppIdsForDomain(string domainId) {
            var json = await LoadResourceAsync($"/WS2/domains/{domainId}/applications");
            return JsonConvert.DeserializeObject<IList<AppId>>(json);
        }
 
        // appel de l'API /WS2/authtoken
        public async Task<AuthToken> GetAuthToken() {
            var json = await LoadResourceAsync($"/WS2/authtoken");
            return JsonConvert.DeserializeObject<AuthToken>(json);
        }
 
        // appel de l'API /WS/company/{companyId}/audit
        public async Task<AuditLog> GetAuditForCompany(string companyId) {
            var json = await LoadResourceAsync($"/WS/company/{companyId}/audit");
            return JsonConvert.DeserializeObject<AuditLog>(json);
        }
    }
}