using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Service.Interfaces;

public interface IQuestionService
{
    Task<List<QuestionFullDto>> GetAllAsync();
    Task<QuestionFullDto?> GetByIdAsync(int id);

    Task CreateAsync(QuestionFullCreateDto dto);
    Task UpdateAsync(QuestionFullDto dto);
    Task DeleteAsync(int id);
  
    
}