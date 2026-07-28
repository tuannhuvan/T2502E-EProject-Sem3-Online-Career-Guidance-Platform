using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Dtos.Career;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Service;

public class AssessmentService : IAssessmentService
{
    private readonly AppDbContext _context;

    public AssessmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SubmitTestAsync(AssessmentResultDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            Console.WriteLine(dto.UserId);
            // ======================
            // 1. CREATE RESULT
            // ======================
            var result = new AssessmentResult
            {
                UserId = dto.UserId,
                CareerTestId= dto.CareerTestId,
                CreatedAt = DateTime.Now,
                Status = 1
            };

            _context.AssessmentResults.Add(result);
            await _context.SaveChangesAsync();

            // ======================
            // 2. SAVE ANSWERS
            // ======================
            foreach (var ans in dto.Answers)
            {
                _context.UserAnswers.Add(new UserAnswer
                {
                    AssessmentResultId = result.Id,
                    QuestionId = ans.QuestionId,
                    OptionId = ans.OptionId,
                    CreatedAt = DateTime.Now,
                    Status = 1
                });
            }

            await _context.SaveChangesAsync();

            // ======================
            // 3. CALCULATE TOP 3
            // ======================
            var scores = await CalculateScores(dto.Answers);

            var top3 = scores
                .OrderByDescending(x => x.Value)
                .Take(3)
                .ToList();

            // ======================
            // 4. UPDATE RESULT (SAVE BEST CAREER)
            // ======================
            var best = top3.FirstOrDefault();

            result.RecommendedCareerPathId = best.Key;
            result.CompatibilityScore = best.Value;

            _context.AssessmentResults.Update(result);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return result.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ======================
    // SCORE LOGIC
    // ======================
    public async Task<Dictionary<int, int>> CalculateScores(List<UserAnswerDto> answers)
    {
        if (answers == null || !answers.Any())
            return new Dictionary<int, int>();

        var optionIds = answers.Select(x => x.OptionId).ToList();

        var mappings = await _context.OptionCareerPaths
            .Where(x => optionIds.Contains(x.OptionId))
            .ToListAsync();

        var result = new Dictionary<int, int>();

        foreach (var map in mappings)
        {
            if (!result.ContainsKey(map.CareerPathId))
                result[map.CareerPathId] = 0;

            result[map.CareerPathId] += map.Weight;
        }

        return result;
    }
}