using System;
using System.Net.Http;
using System.Text.Json;
using PocketIdSync.Apis.ApiKeys;
using PocketIdSync.Apis.Apis;
using PocketIdSync.Apis.ApplicationConfiguration;
using PocketIdSync.Apis.ApplicationImages;
using PocketIdSync.Apis.OidcClients;
using PocketIdSync.Apis.UserGroups;
using PocketIdSync.Models;
using RestSharp;
using RestSharp.Serializers.Json;

namespace PocketIdSync.Apis;

sealed class PocketIdClient
{
    public IRestClient Api { get; init; }

    public PocketIdClient(IRestClient restClient)
    {
        Api = restClient;
    }
}

static class PocketIdApi
{
    extension(PocketIdClient client)
    {
        public VersionApi Version { get { return new VersionApi(client); } }
        public OidcClientsApi OidcClients { get { return new OidcClientsApi(client); } }
        public ApisApi Apis { get { return new ApisApi(client); } }
        public UserGroupsApi UserGroups { get { return new UserGroupsApi(client); } }
        public ApplicationImagesApi ApplicationImages { get { return new ApplicationImagesApi(client); } }
        public ApplicationConfigurationApi ApplicationConfiguration { get { return new ApplicationConfigurationApi(client); } }
        public ApiKeysApi ApiKeys { get { return new ApiKeysApi(client); } }
    }

    extension(IHttpClientFactory httpClientFactory)
    {
        public PocketIdClient Connect(string baseUrl, string apiKey)
        {
            var httpClient = httpClientFactory.CreateClient(nameof(PocketIdClient));
            var restClient = Login(httpClient, baseUrl, apiKey);
            return new PocketIdClient(restClient);
        }
    }

    private static RestClient Login(HttpClient httpClient, string baseUrl, string apiKey)
    {
        httpClient.BaseAddress = new Uri(baseUrl);
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        var options = new RestClientOptions(baseUrl)
        {
            Timeout = TimeSpan.FromSeconds(10),
            FailOnDeserializationError = true,
        };
        var client = new RestClient(
            httpClient,
            options,
            disposeHttpClient: false,
            configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions
            {
                TypeInfoResolver = SourceGenerationContext.Default,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            })
        );
        return client;
    }
}
