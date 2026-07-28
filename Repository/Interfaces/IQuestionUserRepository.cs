using Career_Guidance_Platform.Dtos.Question;

namespace Career_Guidance_Platform.Repository.Interfaces;

public interface IQuestionUserRepository
{
    Task<int> GetCountAsync();
    Task<QuestionTestDto?> GetQuestionByOrderAsync(int order);
   
}