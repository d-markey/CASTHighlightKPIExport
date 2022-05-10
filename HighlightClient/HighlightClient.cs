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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json; 

using HighlightKPIExport.Client.DTO;
using HighlightKPIExport.Technical;

namespace HighlightKPIExport.Client {
    // proxy pour les API Highlight
    public class HighlightClient : IDisposable {
        public HighlightClient(Uri baseUrl) {
            BaseUrl = baseUrl;
        }

        public Uri BaseUrl { get; private set; }

        public void Dispose() {
            _token = null;
            _cred = null;
        }

        private Credential _cred = null;
        private AuthToken _token = null;        

        public async Task Authenticate(Credential credential) {
            _cred = credential;
            _token = await GetAuthToken();
        }

        // appel d'une API Highlight
        private async Task<string> LoadResourceAsync(string resourceUri) {
            if (!Uri.TryCreate(BaseUrl, resourceUri, out Uri uri)) throw new InvalidOperationException();
            var req = WebRequest.Create(uri);
            req.Headers.Add("Accept", "application/json");
            req.Headers.Add("Authorization", _cred.Authorization);
            var resp = await req.GetResponseAsync();
            using (var stream = new StreamReader(resp.GetResponseStream())) {
                return await stream.ReadToEndAsync();
            }
        }

        // appel de l'API /WS2/domains/{domainId}/applications/{appId}
        public async Task<AppInfo> GetAppInfoForApp(string domainId, string appId) {
            var json = await LoadResourceAsync($"/WS2/domains/{domainId}/applications/{appId}/?maxEntryPerPage=30&pageOffset=0");
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