
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received: {input}");

        var json = JsonConvert.DeserializeObject<JObject>(input.ToString()!);
        string issueUrl = json?["issue"]?["html_url"]?.ToString() ?? "";

        if (string.IsNullOrEmpty(issueUrl))
            return "No issue URL found";

        string payload = JsonConvert.SerializeObject(
            new { text = $"Issue Created: {issueUrl}" }
        );

        using var client = new HttpClient();

        using var webRequest = new HttpRequestMessage(
            HttpMethod.Post,
            Environment.GetEnvironmentVariable("SLACK_URL")
        )
        {
            Content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json"
            )
        };

        using var response = client.Send(webRequest);

        return response.Content.ReadAsStringAsync()
            .GetAwaiter().GetResult();
    }
}
