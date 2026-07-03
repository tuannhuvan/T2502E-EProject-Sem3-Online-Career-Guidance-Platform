using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Repository;

public class QuestiontestRepository : IQuestiontestRepository
{
    private readonly AppDbContext _context;

    public QuestiontestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.QuestionTests.CountAsync();
    }

    public async Task<QuestionTestDto?> GetQuestionByOrderAsync(int order)
    {
       
        return await _context.QuestionTests
            .Include(q => q.QuestionType)
            .Include(q => q.QuestionOptions)
            .Where(q => q.Status == 1)
            .OrderBy(q => q.Id)
            .Skip(order - 1)
            .Select(q => new QuestionTestDto
            {
                Id = q.Id,
                Content = q.Content,
                QuestionTypeTitle = q.QuestionType.Name,

                Options = q.QuestionOptions.Select(o =>
                    new QuestionOptionDto
                    {
                        Id = o.Id,
                        Content = o.Content
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }
    public async Task<List<QuestionTest>> GetAllAsync()
    {
        return await _context.QuestionTests
            .Include(x => x.Test)
            .Include(x => x.QuestionType)
            .Include(x => x.QuestionOptions)
            .ThenInclude(o => o.OptionCareerPaths)
            .ThenInclude(cp => cp.CareerPath)
            .ToListAsync();
    }

    public async Task<QuestionTest?> GetByIdAsync(int id)
    {
        return await _context.QuestionTests
            .Include(x => x.Test)
            .Include(x => x.QuestionType)
            .Include(x => x.QuestionOptions)
            .ThenInclude(o => o.OptionCareerPaths)
            .ThenInclude(cp => cp.CareerPath)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task AddAsync(QuestionTest questiontest)
        => await _context.QuestionTests.AddAsync(questiontest);

    public async Task DeleteAsync(int id)
    {
        var q = await _context.QuestionTests.FindAsync(id);
        if (q != null) _context.QuestionTests.Remove(q);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}