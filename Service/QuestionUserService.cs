using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Career;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Repository.Interfaces;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Service;

public class QuestionUserService : IQuestionUserService
{
    private readonly IQuestionUserRepository _repo;
    private readonly AppDbContext _context;

    public QuestionUserService(
        IQuestionUserRepository repo,
        AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<int> GetCountAsync()
    {
        return await _repo.GetCountAsync();
    }

    public async Task<QuestionTestDto?> GetQuestionByOrderAsync(int order)
    {
        return await _repo.GetQuestionByOrderAsync(order);
    }

    public async Task<Dictionary<int, int>> CalculateScores(List<UserAnswerDto> answers)
    {
        if (answers == null || !answers.Any())
            return new Dictionary<int, int>();

        var optionIds = answers
            .Select(x => x.OptionId)
            .Distinct()
            .ToList();

        var mappings = await _context.OptionCareerPaths
            .Where(x => optionIds.Contains(x.OptionId))
            .ToListAsync();

        return mappings
            .GroupBy(x => x.CareerPathId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Weight)
            );
    }

    public async Task<List<TopCareerDto>> GetTop3CareerAsync(List<UserAnswerDto> answers)
    {
        var scores = await CalculateScores(answers);

        if (!scores.Any())
            return new List<TopCareerDto>();

        var top3 = scores
            .OrderByDescending(x => x.Value)
            .Take(3)
            .ToList();

        var careerIds = top3
            .Select(x => x.Key)
            .ToList();

        var careers = await _context.CareerPaths
            .Where(x => careerIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        return top3.Select(x => new TopCareerDto
        {
            CareerPathId = x.Key,
            Title = careers.ContainsKey(x.Key)
                ? careers[x.Key].Title
                : "Unknown Career",

            Score = x.Value
        }).ToList();
    }
}