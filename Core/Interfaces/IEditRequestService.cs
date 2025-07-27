using SmartBook.Core.DTOs;

namespace SmartBook.Core.Interfaces;

public interface IEditRequestService
{
    Task<List<AuthorEditRequestDisplayModel>> GetAuthorEditRequestsAsync(string? keyword = null, string? status = null);

    Task<bool> ReviewAuthorEditRequestAsync(int requestId, bool approve, string? reviewComment);
}