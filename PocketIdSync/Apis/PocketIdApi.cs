using System;
using System.Net.Http;
using System.Text.Json;
using PocketIdSync.Models;
using RestSharp;
using RestSharp.Serializers.Json;

namespace PocketIdSync.Apis;

internal class PocketIdClient
{
    public IRestClient Api { get; private set; }
    public string BaseUrl { get; init; }
    public string ApiKey { private get; init; }

    public PocketIdClient(string baseUrl, string apiKey)
    {
        BaseUrl = baseUrl;
        ApiKey = apiKey;
        Api = Login(BaseUrl, ApiKey);
    }

    public void Refresh()
    {
        Api = Login(BaseUrl, ApiKey);
    }

    public IRestClient Login(string BaseUrl, string ApiKey)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
        };
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
        var options = new RestClientOptions(BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(10),
            FailOnDeserializationError = true,
        };
        var client = new RestClient(
            httpClient,
            options,
            disposeHttpClient: true,
            configureSerialization: s => s.UseSystemTextJson(new JsonSerializerOptions
            {
                TypeInfoResolver = SourceGenerationContext.Default,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            })
        );

        return client;
    }
}

internal static class PocketIdApi
{
    extension(PocketIdClient client)
    {
        public VersionApi Version { get { return new VersionApi(client); } }
        public OidcClientsApi OidcClients { get { return new OidcClientsApi(client); } }
        public UserGroupsApi UserGroups { get { return new UserGroupsApi(client); } }
    }
}
