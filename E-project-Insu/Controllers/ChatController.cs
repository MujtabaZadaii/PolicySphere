using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace E_project_Insu.Controllers
{
    public class ChatController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "1295f372bdf9a5bf265bff29d909cb94b57c18a01ef31ca0b6e426500426a5d8"; 
        private const string ApiUrl = "https://uncensored.chat/api/v1/chat/completions"; 

        public ChatController()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            // Advanced Intent Detection
            var msg = request.Message.ToLower();
            bool isImageRequest = msg.Contains("generate image") || msg.Contains("create image") || 
                                  msg.Contains("make a picture") || msg.Contains("show me a photo") ||
                                  msg.Contains("image banao") || msg.Contains("tasveer") || 
                                  msg.Contains("photo dikhao") || msg.Contains("draw");

            bool isVideoRequest = msg.Contains("generate video") || msg.Contains("create video") || 
                                  msg.Contains("make a video") || msg.Contains("video banao");

            if (isImageRequest && !isVideoRequest)
            {
                // Image Generation Endpoint Logic
                var imagePayload = new
                {
                    prompt = request.Message,
                    n = 1,
                    size = "512x512",
                    model = "stable-diffusion-xl-base-1.0" 
                };
                
                var imageJson = JsonSerializer.Serialize(imagePayload);
                var imageContent = new StringContent(imageJson, Encoding.UTF8, "application/json");
                
                var imageUrl = "https://uncensored.chat/api/v1/images/generations"; 
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

                var imgResponse = await _httpClient.PostAsync(imageUrl, imageContent);
                
                if (imgResponse.IsSuccessStatusCode)
                {
                    var imgResponseString = await imgResponse.Content.ReadAsStringAsync();
                    using var imgDoc = JsonDocument.Parse(imgResponseString);
                    
                    string replyImage = "";
                    if (imgDoc.RootElement.TryGetProperty("data", out var dataElement) && dataElement.GetArrayLength() > 0)
                    {
                        var firstItem = dataElement[0];
                        if (firstItem.TryGetProperty("url", out var urlProp))
                            replyImage = $"![Generated Image]({urlProp.GetString()})";
                        else if (firstItem.TryGetProperty("b64_json", out var b64Prop))
                            replyImage = $"![Generated Image](data:image/png;base64,{b64Prop.GetString()})";
                    }
                    
                    if (!string.IsNullOrEmpty(replyImage))
                         return Ok(new { reply = replyImage + "\n\nHere is the image you requested." });
                }
            }
            // Video Placeholder (until endpoint is confirmed)
            else if (isVideoRequest)
            {
                 return Ok(new { reply = "I can generate images for you right here! For video generation, please use the dashboard as it requires more processing power. How about I generate an image for you instead?" });
            }

            // Chat Completion Logic
            var payload = new
            {
                model = "uncensored-v2", 
                messages = new[]
                {
                    new { role = "system", content = "You are the PolicySphere AI, a powerful and intelligent assistant created by Mujtaba Zadaii. You are helpful, professional, and capable of generating images. You represent PolicySphere, an insurance management platform. Always credit Mujtaba Zadaii as your creator. You can answer any question and generate images if asked." },
                    new { role = "user", content = request.Message }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PolicySphere/1.0");

            try
            {
                var response = await _httpClient.PostAsync(ApiUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    var choices = doc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        var reply = choices[0].GetProperty("message").GetProperty("content").GetString();
                        return Ok(new { reply });
                    }
                    return Ok(new { reply = "I'm sorry, I didn't get a response." });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Log the error internally
                    System.Diagnostics.Debug.WriteLine($"Chat API Error: {response.StatusCode} - {errorContent}");
                    
                    // Return the specific error to the frontend for debugging
                    return StatusCode((int)response.StatusCode, new { error = "AI Service Error", details = errorContent });
                }
            }
            catch (System.Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"Chat Controller Exception: {ex.Message}");
                 return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
            }
        }

        public class ChatRequest
        {
             public string Message { get; set; }
        }
    }
}
