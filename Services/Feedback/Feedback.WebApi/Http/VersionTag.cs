using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Feedback.WebApi.Http;

public static class VersionTag
{
    public static void Write(HttpResponse response, int version)
    {
        response.Headers["ETag"] = $"\"{version}\"";
    }

    public static bool TryRead(
        HttpRequest request,
        out int version)
    {
        version = 0;

        var value = request.Headers["If-Match"]
            .ToString()
            .Trim();

        if (value.Length < 3 ||
            value[0] != '"' ||
            value[^1] != '"')
        {
            return false;
        }

        return int.TryParse(
            value[1..^1],
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out version);
    }
}