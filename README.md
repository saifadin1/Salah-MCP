# SalahMCP

A simple [Model Context Protocol](https://modelcontextprotocol.io/) (MCP) server that tells you which prayer (Salah) is coming up next and how much time is left until it starts.

It fetches prayer times from the [Al Adhan API](https://aladhan.com/prayer-times-api) based on a city and country, then calculates the next upcoming prayer using the correct local timezone.

## Installation

Pull the image from Docker Hub:

```bash
docker pull saif637/salah-mcp:latest
```

Or build it yourself from source:

```bash
git clone https://github.com/saifadin1/Salah-MCP.git
cd SalahMCP
docker build -t salah-mcp .
```

## Configuration

By default, the server uses **Cairo, Egypt**. You can override this with environment variables passed to Docker, or by passing arguments directly to the tool.

| Variable         | Description                    | Default |
|------------------|---------------------------------|---------|
| `PRAYER_CITY`    | Default city for prayer times   | `Cairo` |
| `PRAYER_COUNTRY` | Default country                 | `Egypt` |

## Using it with an MCP client (e.g. Claude Desktop)

Add an entry to your MCP client's config, pointing Docker at the image:

```json
{
  "mcpServers": {
    "salah": {
      "command": "docker",
      "args": [
        "run",
        "-i",
        "--rm",
        "-e",
        "PRAYER_CITY=Cairo",
        "-e",
        "PRAYER_COUNTRY=Egypt",
        "saif637/salah-mcp:latest"
      ]
    }
  }
}
```

The server communicates over stdio, as expected by MCP clients — the `-i` flag keeps stdin open and `--rm` cleans up the container after each run.

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

## License

MIT (or your license of choice).
