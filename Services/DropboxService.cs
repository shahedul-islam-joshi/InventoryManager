using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InventoryManager.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace InventoryManager.Services
{
    public class DropboxService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _accessToken;
        private readonly string _uploadFolder;

        public DropboxService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _accessToken = configuration["Dropbox:AccessToken"];
            _uploadFolder = configuration["Dropbox:UploadFolder"] ?? "/SupportTickets";
        }

        public async Task<bool> UploadTicketAsync(SupportTicketDto ticket)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new Exception("Dropbox AccessToken is missing from configuration.");
            }

            var fileName = $"ticket_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = $"{_uploadFolder}/{fileName}";

            var jsonContent = JsonSerializer.Serialize(ticket);
            var bytes = Encoding.UTF8.GetBytes(jsonContent);
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var apiArg = new
            {
                path = filePath,
                mode = "add",
                autorename = true,
                mute = false,
                strict_conflict = false
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://content.dropboxapi.com/2/files/upload");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(apiArg));
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Dropbox error {response.StatusCode}: {errorBody}");
            }

            return true;
        }
    }
}
