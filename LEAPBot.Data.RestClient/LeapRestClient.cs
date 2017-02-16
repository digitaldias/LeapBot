using LEAPBot.Domain.Contracts;
using LEAPBot.Domain.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LEAPBot.Data.RestClient
{
    public class LeapRestClient : ILeapRestClient
    {
        private string _baseUrl;
        private string _apiKey;
        private string _apiSecret;

        private IEnumerable<MasterClass> _masterClasses;

        public LeapRestClient(ISettingsReader settingsReader)
        {
            _apiKey    = settingsReader["LEAP:ApiKey"];
            _apiSecret = settingsReader["LEAP:Secret"];
            _baseUrl   = settingsReader["LEAP:BaseUrl"];
        }


        public async Task<IEnumerable<MasterClass>> GetMasterClasses()
        {
            if (_masterClasses == null)
                _masterClasses = await GetMasterClassesFromApi();

            return _masterClasses;
        }

        private async Task<IEnumerable<MasterClass>> GetMasterClassesFromApi()
        {
            var client = CreateApiClient();
            var uri = new Uri(_baseUrl + "MasterClasses");

            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return new List<MasterClass>();

            var jsonString = await response.Content.ReadAsStringAsync();
            client.Dispose();
            response.Dispose();

            return JsonConvert.DeserializeObject<IEnumerable<MasterClass>>(jsonString);
        }


        public async Task<IEnumerable<Speaker>> GetMasterClassSpeakers(int masterClassNumber)
        {
            var client = CreateApiClient();
            var uri = new Uri(_baseUrl + "Speakers/ByMasterClass/" + masterClassNumber);

            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return new List<Speaker>();

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Speaker>>(jsonString);

        }


        public async Task<IEnumerable<Speaker>> GetAllSpeakers()
        {
            var client = CreateApiClient();
            var uri = new Uri(_baseUrl + "Speakers");

            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return new List<Speaker>();

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Speaker>>(jsonString);
        }

        public async Task<Speaker> GetSpeakerByPartialName(string speakerName)
        {
            if (string.IsNullOrEmpty(speakerName) || speakerName.Length < 3)
                return null;

            var client = CreateApiClient();
            var uri = new Uri(_baseUrl + "Speakers/ByName/" + speakerName);

            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return null;

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Speaker>(jsonString);
        }


        private HttpClient CreateApiClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
            client.DefaultRequestHeaders.Add("leap", _apiSecret);
            return client;
        }


    }
}
