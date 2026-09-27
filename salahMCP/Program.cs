using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();

[McpServerToolType]
public static class PrayerTools
{
    private static readonly HttpClient Http = new();

    [McpServerTool]
    [Description("Calculates which prayer (Salah) is next and how much time remains until it starts.")]
    public static async Task<string> Salah(
        [Description("City name (optional). Overrides the PRAYER_CITY environment variable.")]
        string? city = null,
        [Description("Country name (optional). Overrides the PRAYER_COUNTRY environment variable.")]
        string? country = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetCity = !string.IsNullOrWhiteSpace(city) 
                ? city 
                : Environment.GetEnvironmentVariable("PRAYER_CITY") ?? "Cairo";

            var targetCountry = !string.IsNullOrWhiteSpace(country) 
                ? country 
                : Environment.GetEnvironmentVariable("PRAYER_COUNTRY") ?? "Egypt";

            var url = $"https://api.aladhan.com/v1/timingsByCity?city={Uri.EscapeDataString(targetCity)}&country={Uri.EscapeDataString(targetCountry)}";
            var response = await Http.GetFromJsonAsync<JsonObject>(url, cancellationToken);

            var data = response?["data"];
            if (data is null)
                return $"Error: Could not retrieve prayer times for {targetCity}, {targetCountry}.";

            var timings = data["timings"]!.AsObject();
            var meta = data["meta"]!.AsObject();
            var timezoneId = meta["timezone"]!.ToString();

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            var nowInLocation = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            string[] prayerNames = ["Fajr", "Dhuhr", "Asr", "Maghrib", "Isha"];
            var schedule = new List<(string Name, DateTime Time)>();

            foreach (var name in prayerNames)
            {
                var rawTime = timings[name]!.ToString().Split(' ')[0];
                var parts = rawTime.Split(':');
                var prayerTime = new DateTime(
                    nowInLocation.Year, nowInLocation.Month, nowInLocation.Day,
                    int.Parse(parts[0]), int.Parse(parts[1]), 0);

                schedule.Add((name, prayerTime));
            }

            var next = schedule.FirstOrDefault(p => p.Time > nowInLocation);

            if (next.Name is null)
            {
                var fajrParts = timings["Fajr"]!.ToString().Split(' ')[0].Split(':');
                var tomorrowFajr = new DateTime(
                    nowInLocation.Year, nowInLocation.Month, nowInLocation.Day,
                    int.Parse(fajrParts[0]), int.Parse(fajrParts[1]), 0).AddDays(1);

                var diff = tomorrowFajr - nowInLocation;
                return $"All prayers for today have passed in {targetCity} (current time: {nowInLocation:HH:mm}). " +
                       $"The next prayer is Fajr tomorrow at {tomorrowFajr:HH:mm} (in {diff.Hours}h {diff.Minutes}m).";
            }
            else
            {
                var diff = next.Time - nowInLocation;
                return $"Current time in {targetCity}: {nowInLocation:HH:mm}. " +
                       $"The next prayer is {next.Name} at {next.Time:HH:mm} (in {diff.Hours}h {diff.Minutes}m).";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Error in Salah tool]: {ex.Message}");
            return $"Failed to calculate prayer time: {ex.Message}";
        }
    }
}