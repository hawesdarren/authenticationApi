using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

using Microsoft.Extensions.Http;


namespace IntegrationTests
{
    public class CustomClientFactory
    {
        public CustomClientFactory() {
            FlurlHttp.Clients.WithDefaults(builder => builder
                    .ConfigureInnerHandler(hc => hc
                        .ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true));
        }
        
    }
}
