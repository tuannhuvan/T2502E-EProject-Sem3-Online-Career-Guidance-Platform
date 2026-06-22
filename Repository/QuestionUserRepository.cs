using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Repository;

public class QuestionUserRepository : IQuestionUserRepository
{
    private readonly AppDbContext _context;

    public QuestionUserRepository(AppDbContext context)
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
}