using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LMStudioClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Asking LM Studio: 'What is the meaning of life?'");
            await AskLMStudioCompletionAsync();
        }

        static async Task AskLMStudioCompletionAsync()
        {
            // Replace with your LM Studio server address if needed.
            string baseUrl = "http://127.0.0.1:1234/";
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Construct the JSON payload.
                var payload = new
                {
                    model = "granite-3.0-2b-instruct",  // Change the model ID as needed.
                    prompt = "What is the meaning of life?",
                    temperature = 0.7,
                    max_tokens = 64,
                    stream = false,
                    stop = "\n"
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                var contentData = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    // Create a POST request to the text completions endpoint.
                    using (var request = new HttpRequestMessage(HttpMethod.Post, "api/v0/completions"))
                    {
                        request.Content = contentData;

                        HttpResponseMessage response = await client.SendAsync(request, CancellationToken.None);
                        response.EnsureSuccessStatusCode();

                        // Read the entire response as a string.
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Parse the JSON response.
                        using (JsonDocument doc = JsonDocument.Parse(responseBody))
                        {
                            if (doc.RootElement.TryGetProperty("choices", out JsonElement choices))
                            {
                                int count = choices.GetArrayLength();
                                Console.WriteLine($"\nNumber of choices: {count}");

                                // Print the text of the first choice if it exists.
                                if (count > 0)
                                {
                                    JsonElement firstChoice = choices[0];
                                    if (firstChoice.TryGetProperty("text", out JsonElement textElement))
                                    {
                                        string text = textElement.GetString();
                                        Console.WriteLine("\nResponse from LM Studio:");
                                        Console.WriteLine(text);
                                    }
                                    else
                                    {
                                        Console.WriteLine("The first choice does not contain a 'text' property.");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("No choices were returned in the response.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error calling LM Studio API: " + ex.Message);
                }
            }
        }
    }
}
