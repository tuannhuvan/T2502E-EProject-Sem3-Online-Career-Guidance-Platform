using Career_Guidance_Platform.Dtos.Question;

namespace Career_Guidance_Platform.Service.Interfaces;

public interface IQuestiontestService
{
    Task<List<QuestionFullDto>> GetAllAsync();
    Task<QuestionFullDto?> GetByIdAsync(int id);

    Task CreateAsync(QuestionFullCreateDto dto);
    Task UpdateAsync(QuestionFullDto dto);
    Task DeleteAsync(int id);
    
}