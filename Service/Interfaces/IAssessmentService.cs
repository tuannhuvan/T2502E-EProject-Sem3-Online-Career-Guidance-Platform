using Career_Guidance_Platform.Dtos;

namespace Career_Guidance_Platform.Service.Interfaces;

public interface IAssessmentService
{
    Task<int> SubmitTestAsync(AssessmentResultDto dto);
    Task<Dictionary<int, int>> CalculateScores(List<UserAnswerDto> answers);
}