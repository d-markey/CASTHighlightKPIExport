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
using System.Net;
using System.Text;

namespace HighlightKPIExport.Technical {
    public class Credential {

        public Credential(NetworkCredential credential) {
            _credential = credential;
        }

        public Credential(String token) {
            _token = token;
        }

        private NetworkCredential _credential;
        private String _token;

        private String _auth;

        public String Authorization {
            get {
                if (_auth == null) {
                    if (_credential != null) {
                        var userAndPwd = Encoding.UTF8.GetBytes(_credential.UserName + ":" + _credential.Password);
                        _auth = "Basic " + Convert.ToBase64String(userAndPwd);
                    } else if (!string.IsNullOrWhiteSpace(_token)) {
                        _auth = "Bearer " + _token;
                    } else {
                        throw new UnauthorizedAccessException("Missing authentication information");
                    }
                }
                return _auth;
            }
        }
    }
}