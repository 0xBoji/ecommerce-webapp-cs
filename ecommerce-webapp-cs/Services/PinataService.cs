using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class PinataService
{
    private readonly HttpClient _httpClient;
    private readonly string _jwtToken;

    public PinataService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _jwtToken = configuration["Pinata:Jwt"]; // Get JWT from configuration

        // Set up HttpClient instance for Pinata API
        _httpClient.BaseAddress = new Uri("https://cyan-eldest-earwig-943.mypinata.cloud/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    public async Task<string> UploadImageToPinataAsync(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return null;
        }

        using (var content = new MultipartFormDataContent())
        using (var imageStream = imageFile.OpenReadStream())
        {
            content.Add(new StreamContent(imageStream), "file", imageFile.FileName);

            // Send a POST request to Pinata's /pinning/pinFileToIPFS endpoint
            var response = await _httpClient.PostAsync("pinning/pinFileToIPFS", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var ipfsHash = ExtractIpfsHashFromResponse(responseContent);
                return ipfsHash;
            }
            else
            {
                // Handle failure
                throw new Exception("Failed to upload image to Pinata");
            }
        }
    }

    private string ExtractIpfsHashFromResponse(string responseContent)
    {
        return "ExtractedIpfsHash";
    }
}
