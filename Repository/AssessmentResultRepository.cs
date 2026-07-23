using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository.Interfaces;

namespace Career_Guidance_Platform.Repository;

public class AssessmentResultRepository
    : IAssessmentResultRepository
{
    private readonly AppDbContext _context;

    public AssessmentResultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAssessmentAsync(AssessmentResult result)
    {
        await _context.AssessmentResults.AddAsync(result);
    }

    public async Task AddUserAnswerAsync(UserAnswer answer)
    {
        await _context.UserAnswers.AddAsync(answer);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
    
