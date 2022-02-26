using Ruler.Engine.Manifest;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ruler.IRule
{
    public class GitClient
    {
        HttpClient _client;

        public GitClient()
        {
            _client = new HttpClient();
        }

        public async Task<MemoryStream> Download(ProgressTask task, string url)
        {
            try
            {
                using (HttpResponseMessage response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    // Set the max value of the progress task to the number of bytes
                    task.MaxValue(response.Content.Headers.ContentLength ?? 0);
                    // Start the progress task
                    task.StartTask();

                    var filename = url.Substring(url.LastIndexOf('/') + 1);
                    AnsiConsole.MarkupLine($"Starting download of [u]{filename}[/] ({task.MaxValue} bytes)");

                    int read;
                    var memoryStream = new MemoryStream();
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[8192];
                        do
                        {
                            read = await contentStream.ReadAsync(buffer, 0, buffer.Length);

                            // Increment the number of read bytes for the progress task
                            task.Increment(read);

                            // Write the read bytes to the output stream
                            await memoryStream.WriteAsync(buffer, 0, read);
                        } while (read != 0);
                    }
                    return memoryStream;
                }
            }
            catch (Exception ex)
            {
                // An error occured
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex}");
                return null!;
            }
        }
        public async Task<IEnumerable<string>> GetVersionsAsync(string url)
        {
            var response = await _client.GetAsync(url);
            var manifest = await response.Content.ReadFromJsonAsync<VersionsManifest>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                ?? throw new ArgumentNullException(nameof(response));
            return manifest.Versions.Select(x => x.Value.Name);
        }
    }
}