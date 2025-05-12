using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ExpertMed.Services
{
    public class ChatGPTService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ChatGPTService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }


    }
}