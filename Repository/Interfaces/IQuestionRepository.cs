using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Repository.Interfaces;

public interface IQuestionRepository
{
    Task<List<Question>> GetAllAsync();
    Task<Question?> GetByIdAsync(int id);

    Task AddAsync(Question question);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
 
}