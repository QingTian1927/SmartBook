using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartBook.Core.DTOs;
using SmartBook.Core.Models;

namespace SmartBook.Core.Services;

public class GeminiService
{
    private const string EndPoint =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public GeminiService()
    {
        _httpClient = new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }

    public async Task<string?> GetGeminiResponseAsync(string prompt)
    {
        var url = $"{EndPoint}?key={_apiKey}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        string jsonPayload = JsonSerializer.Serialize(payload);

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }

    public async Task<List<GeminiRecommendationDTO>> GetRecommendationsFromGeminiAsync(
        User user,
        List<UserBook> userBooks,
        List<Book> allBooks,
        bool enableLogging = false)
    {
        string prompt = BuildPrompt(user, userBooks, allBooks);
        string? responseJson = await GetGeminiResponseAsync(prompt);

        if (responseJson is null) return new();

        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var contentElement = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(contentElement)) return new();

            int start = contentElement.IndexOf('[');
            int end = contentElement.LastIndexOf(']');
            if (start == -1 || end == -1 || end <= start) return new();

            string jsonArray = contentElement.Substring(start, end - start + 1);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var rawRecs = JsonSerializer.Deserialize<List<GeminiRecommendationDTO>>(jsonArray, options);
            var recommendations = rawRecs?.Select(rec =>
            {
                if (string.IsNullOrWhiteSpace(rec.Description))
                {
                    rec.Description = "No description available. Let the story surprise you!";
                }

                return rec;
            }).ToList();

            if (enableLogging && recommendations is not null && recommendations.Any())
            {
                Console.WriteLine("Gemini Parsed Recommendations:");
                foreach (var rec in recommendations)
                {
                    Console.WriteLine($"BookId: {rec.BookId}, Reason: {rec.Reason}, Desc: {rec.Description}");
                }
            }

            return recommendations ?? new();
        }
        catch (Exception ex)
        {
            if (enableLogging)
            {
                Console.WriteLine("Error during Gemini response parsing:");
                Console.WriteLine(ex.Message);
            }

            return new();
        }
    }

    private string BuildPrompt(User user, List<UserBook> userBooks, List<Book> allBooks)
    {
        var ratedBooks = userBooks
            .Where(ub => ub.Rating >= 3)
            .Select(ub =>
                $"- BookId: {ub.BookId}, Title: {ub.Book.Title}, Author: {ub.Book.Author.Name}, Category: {ub.Book.Category.Name}, Rating: {ub.Rating}")
            .ToList();

        var candidateBooks = allBooks
            .Where(b => !userBooks.Any(ub => ub.BookId == b.Id))
            .Select(b =>
                $"- BookId: {b.Id}, Title: {b.Title}, Author: {b.Author.Name}, Category: {b.Category.Name}")
            .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("You are a recommendation engine.");
        sb.AppendLine();
        sb.AppendLine(
            "Given the user's past highly-rated books (rating ≥ 3) and a list of candidate books, your task is to suggest up to 5 books that the user might enjoy.");
        sb.AppendLine();
        sb.AppendLine(
            "You must return your response strictly as a raw JSON array of objects with this exact structure:");
        sb.AppendLine();
        sb.AppendLine("[");
        sb.AppendLine("  {");
        sb.AppendLine("    \"BookId\": 123,");
        sb.AppendLine("    \"Reason\": \"You liked books by this author and rated them highly.\",");
        sb.AppendLine(
            "    \"Description\": \"A thrilling science fiction story about identity, freedom, and the power of human connection.\"");
        sb.AppendLine("  },");
        sb.AppendLine("  ...");
        sb.AppendLine("]");
        sb.AppendLine();
        sb.AppendLine("Your response must:");
        sb.AppendLine("- Be strictly valid JSON");
        sb.AppendLine("- Contain only the JSON array - no explanations, headers, natural language, or markdown");
        sb.AppendLine("- Not wrap the array in code blocks (like triple backticks)");
        sb.AppendLine("- Ensure that BookId refers only to books in the candidate list");
        sb.AppendLine("- Include a 1–2 sentence Description field for each book summarizing its plot or content");
        sb.AppendLine();
        sb.AppendLine("Example response:");
        sb.AppendLine("[");
        sb.AppendLine("  {");
        sb.AppendLine("    \"BookId\": 101,");
        sb.AppendLine("    \"Reason\": \"You enjoyed books from this category.\",");
        sb.AppendLine(
            "    \"Description\": \"An emotional coming-of-age story about a young girl's journey to find her voice in a small rural town.\"");
        sb.AppendLine("  },");
        sb.AppendLine("  {");
        sb.AppendLine("    \"BookId\": 204,");
        sb.AppendLine("    \"Reason\": \"This book is by an author you’ve rated highly.\",");
        sb.AppendLine(
            "    \"Description\": \"A fast-paced detective mystery filled with twists, secrets, and psychological tension.\"");
        sb.AppendLine("  }");
        sb.AppendLine("]");
        sb.AppendLine();
        sb.AppendLine(
            "You have no memory of previous inputs. All necessary context is provided below. You must rely solely on the provided lists of rated and candidate books.");
        sb.AppendLine();
        sb.AppendLine($"User: {user.Username}");
        sb.AppendLine();
        sb.AppendLine("UserRatings:");
        foreach (var line in ratedBooks)
            sb.AppendLine(line);

        sb.AppendLine();
        sb.AppendLine("CandidateBooks:");
        foreach (var line in candidateBooks)
            sb.AppendLine(line);

        string prompt = sb.ToString();

        Console.WriteLine("Gemini Prompt:");
        Console.WriteLine(prompt);
        Console.WriteLine();

        return prompt;
    }
}