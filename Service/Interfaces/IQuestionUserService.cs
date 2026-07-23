using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Career;
using Career_Guidance_Platform.Dtos.Question;

namespace Career_Guidance_Platform.Service.Interfaces;

public interface IQuestionUserService
{
    
    Task<int> GetCountAsync();
    Task<QuestionTestDto?> GetQuestionByOrderAsync(int order);
    Task<Dictionary<int, int>> CalculateScores(List<UserAnswerDto> answers);
    
    Task<List<TopCareerDto>> GetTop3CareerAsync(List<UserAnswerDto> answers);
}