# SalahMCP

A simple [Model Context Protocol](https://modelcontextprotocol.io/) (MCP) server that tells you which prayer (Salah) is coming up next and how much time is left until it starts.

It fetches prayer times from the [Al Adhan API](https://aladhan.com/prayer-times-api) based on a city and country, then calculates the next upcoming prayer using the correct local timezone.

## Features

- Calculates the next prayer (Fajr, Dhuhr, Asr, Maghrib, or Isha) and the time remaining until it starts
- Uses the real local timezone of the requested city (not the server's local time)
- Falls back to tomorrow's Fajr if all of today's prayers have already passed
- City/country can be passed per-request, or configured once via environment variables

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- Internet access (calls `api.aladhan.com`)

## Installation

```bash
git clone <your-repo-url>
cd SalahMCP
dotnet restore
dotnet build
```

## Configuration

By default, the server uses **Cairo, Egypt**. You can override this with environment variables or by passing arguments directly to the tool.

| Variable        | Description                  | Default |
|-----------------|-------------------------------|---------|
| `PRAYER_CITY`    | Default city for prayer times | `Cairo` |
| `PRAYER_COUNTRY` | Default country               | `Egypt` |

Example:

```bash
export PRAYER_CITY="Istanbul"
export PRAYER_COUNTRY="Turkey"
```

## Running the server

The server communicates over stdio, as expected by MCP clients:

```bash
dotnet run
```

## Using it with an MCP client (e.g. Claude Desktop)

Add an entry to your MCP client's config, pointing at the built executable or `dotnet run`:

```json
{
  "mcpServers": {
    "salah": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/SalahMCP"],
      "env": {
        "PRAYER_CITY": "Cairo",
        "PRAYER_COUNTRY": "Egypt"
      }
    }
  }
}
```

## Tool reference

### `Salah`

Calculates which prayer is next and how much time remains until it starts.

**Parameters** (both optional — override the environment variable defaults):

| Parameter | Type   | Description                                  |
|-----------|--------|-----------------------------------------------|
| `city`    | string | City name, e.g. `"Cairo"`                     |
| `country` | string | Country name, e.g. `"Egypt"`                  |

**Example response:**

```
Current time in Cairo: 19:23. The next prayer is Isha at 20:03 (in 0h 39m).
```

If all prayers for the day have passed:

```
All prayers for today have passed in Cairo (current time: 23:50). The next prayer is Fajr tomorrow at 04:15 (in 4h 25m).
```

## How it works

1. Fetches the day's prayer timings for the given city/country from the Al Adhan API.
2. Reads the IANA timezone returned in the API response and converts the current UTC time into that local time — so it's accurate regardless of where the server itself is hosted.
3. Compares the local time against Fajr, Dhuhr, Asr, Maghrib, and Isha to find the next one.
4. If the current time is past Isha, it rolls over to Fajr the next day.

## License

MIT (or your license of choice).