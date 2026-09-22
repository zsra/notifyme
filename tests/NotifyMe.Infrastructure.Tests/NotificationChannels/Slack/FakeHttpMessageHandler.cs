using System.Net;
using System.Text;
using System.Text.Json;

namespace NotifyMe.Infrastructure.Tests.NotificationChannels.Slack;

/// <summary>
/// Hand-rolled fake <see cref="HttpMessageHandler"/> that captures the last request it received
/// and returns a canned response, so <see cref="Infrastructure.NotificationChannels.Slack.SlackNotificationChannel"/>
/// can be unit tested without a real HTTP endpoint.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;

    public FakeHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK, string responseBody = "ok")
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
    }

    public HttpRequestMessage? LastRequest { get; private set; }

    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, "text/plain"),
        };
    }
}
