using SmartBook.Core.DTOs;
using SmartBook.Core.Models;

namespace SmartBook.Core.Interfaces;

public interface IGeminiService
{
    Task<string?> GetGeminiResponseAsync(string prompt);

    Task<List<GeminiRecommendationDTO>> GetRecommendationsFromGeminiAsync(
        User user,
        List<UserBook> userBooks,
        List<Book> allBooks,
        bool enableLogging = false);

    Task<string> GenerateBookDescriptionAsync(Book book);
}