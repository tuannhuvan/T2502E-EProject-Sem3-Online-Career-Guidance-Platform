using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Repository;

public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;

    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Questions.CountAsync();
    }

    public async Task<QuestionTestDto?> GetQuestionByOrderAsync(int order)
    {
       
        return await _context.Questions
            .Include(q => q.QuestionType)
            .Include(q => q.QuestionOptions)
            .Where(q => q.Status == 1)
            .OrderBy(q => q.Id)
            .Skip(order - 1)
            .Select(q => new QuestionTestDto
            {
                Id = q.Id,
                Content = q.Content,
                QuestionTypeTitle = q.QuestionType.Title,

                Options = q.QuestionOptions.Select(o =>
                    new QuestionOptionDto
                    {
                        Id = o.Id,
                        Content = o.Content
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }
    public async Task<List<Question>> GetAllAsync()
    {
        return await _context.Questions
            .Include(x => x.CareerTest)
            .Include(x => x.QuestionType)
            .Include(x => x.QuestionOptions)
            .ThenInclude(o => o.OptionCareerPaths)
            .ThenInclude(cp => cp.CareerPath)
            .ToListAsync();
    }

    public async Task<Question?> GetByIdAsync(int id)
    {
        return await _context.Questions
            .Include(x => x.CareerTest)
            .Include(x => x.QuestionType)
            .Include(x => x.QuestionOptions)
            .ThenInclude(o => o.OptionCareerPaths)
            .ThenInclude(cp => cp.CareerPath)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task AddAsync(Question question)
        => await _context.Questions.AddAsync(question);

    public async Task DeleteAsync(int id)
    {
        var q = await _context.Questions.FindAsync(id);
        if (q != null) _context.Questions.Remove(q);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}