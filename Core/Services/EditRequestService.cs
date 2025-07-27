using Microsoft.EntityFrameworkCore;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;

namespace SmartBook.Core.Services;

public class EditRequestService : IEditRequestService
{
    private readonly SmartBookDbContext _db;
    
    public EditRequestService(SmartBookDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<AuthorEditRequestDisplayModel>> GetAuthorEditRequestsAsync(string? keyword = null,
        string? status = null)
    {
        var query = _db.AuthorEditRequests
            .Include(r => r.Author)
            .Include(r => r.RequestedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(r =>
                r.Author.Name.Contains(keyword) ||
                r.ProposedName.Contains(keyword) ||
                r.RequestedByUser.Username.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(r => r.Status == status);
        }

        return await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new AuthorEditRequestDisplayModel
            {
                Id = r.Id,
                CurrentAuthorName = r.Author.Name,
                CurrentBio = r.Author.Bio,
                ProposedName = r.ProposedName,
                ProposedBio = r.ProposedBio,
                RequestedByUsername = r.RequestedByUser.Username,
                RequestedAt = r.RequestedAt.ToLocalTime(),
                Status = r.Status,
                ReviewComment = r.ReviewComment
            }).ToListAsync();
    }
    
    public async Task<bool> ReviewAuthorEditRequestAsync(int requestId, bool approve, string? reviewComment)
    {
        var req = await _db.AuthorEditRequests.Include(r => r.Author)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (req == null) return false;
        if (req.Status != "Pending") return false; // Already processed

        req.Status = approve ? "Approved" : "Rejected";
        req.ReviewComment = reviewComment;
        req.ReviewedAt = DateTime.UtcNow;

        if (approve)
        {
            // Apply the change
            if (!string.IsNullOrWhiteSpace(req.ProposedName))
                req.Author.Name = req.ProposedName;
            if (!string.IsNullOrWhiteSpace(req.ProposedBio))
                req.Author.Bio = req.ProposedBio;
        }

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}