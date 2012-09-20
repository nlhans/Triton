using System;
using System.IO;
using System.Collections.Generic;
using System.Net;

namespace Triton.Web
{
    public class WebClientBrowser
    {
        private WebClient _Client;

        private string _Referer;
        public string Referer
        {
            get
            {
                return _Referer;
            }
            set
            {
                this._Referer = value;
            }
        }

        public WebClientBrowser()
        {
            _Client = new WebClient();

        }

        public WebClientBrowser(string Startpage)
        {
            _Client = new WebClient();
            this.NavigateTo(Startpage);
        }

        public string NavigateTo(string page, Dictionary<string, string> POST)
        {

            return string.Empty;
        }

        public string NavigateTo(string page)
        {

            return string.Empty;
        }

    }
}
