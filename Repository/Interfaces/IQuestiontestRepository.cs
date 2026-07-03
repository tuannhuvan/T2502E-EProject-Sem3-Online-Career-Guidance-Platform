using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Repository.Interfaces;

public interface IQuestiontestRepository
{
    Task<List<QuestionTest>> GetAllAsync();
    Task<QuestionTest?> GetByIdAsync(int id);

    Task AddAsync(QuestionTest questiontest);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}