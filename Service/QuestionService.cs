using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository.Interfaces;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Service;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _repo;
    private readonly AppDbContext _context;

    public QuestionService(
        IQuestionRepository repo,
        AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }
    // ================= GET ALL =================
    public async Task<List<QuestionFullDto>> GetAllAsync()
    {
        var data = await _repo.GetAllAsync();

        return data.Select(q => new QuestionFullDto
        {
            Id = q.Id,
            Content = q.Content,

            CareerTestTitle = q.CareerTest?.Title ?? "",
            QuestionTypeTitle = q.QuestionType?.Title ?? "",

            Options = q.QuestionOptions.Select(o => new OptionCreateDto
            {
                Id = o.Id,
                Content = o.Content,

                CareerPaths = o.OptionCareerPaths.Select(cp => new OptionCareerPathDto
                {
                    CareerPathId = cp.CareerPathId,
                    CareerPathTitle = cp.CareerPath.Title,
                    Weight = cp.Weight
                }).ToList()
            }).ToList()
        }).ToList();
    }

    // ================= GET BY ID =================
    public async Task<QuestionFullDto?> GetByIdAsync(int id)
    {
        var q = await _repo.GetByIdAsync(id);
        if (q == null) return null;

        return new QuestionFullDto
        {
            Id = q.Id,
            Content = q.Content,

            CareerTestTitle = q.CareerTest?.Title ?? "",
            QuestionTypeTitle = q.QuestionType?.Title ?? "",

            Options = q.QuestionOptions.Select(o => new OptionCreateDto
            {
                Id = o.Id,
                Content = o.Content,

                CareerPaths = o.OptionCareerPaths.Select(cp => new OptionCareerPathDto
                {
                    CareerPathId = cp.CareerPathId,
                    CareerPathTitle = cp.CareerPath.Title,
                    Weight = cp.Weight
                }).ToList()
            }).ToList()
        };
    }

    // ================= CREATE =================
    public async Task CreateAsync(QuestionFullCreateDto dto)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var question = new Question
            {
                CareerTestId  = dto.CareerTestId ,
                QuestionTypeId = dto.QuestionTypeId,
                Content = dto.Content,
                CreatedAt = DateTime.Now,
                Status = 1
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            foreach (var opt in dto.Options)
            {
                var option = new QuestionOption
                {
                    QuestionId = question.Id,
                    Content = opt.Content,
                    CreatedAt = DateTime.Now,
                    Status = 1
                };

                _context.QuestionOptions.Add(option);
                await _context.SaveChangesAsync();

                foreach (var cp in opt.CareerPaths)
                {
                    _context.OptionCareerPaths.Add(new OptionCareerPath
                    {
                        OptionId = option.Id,
                        CareerPathId = cp.CareerPathId,
                        Weight = cp.Weight,
                        CreatedAt = DateTime.Now,
                        Status = 1
                    });
                }
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ================= UPDATE =================
    public async Task UpdateAsync(QuestionFullDto dto)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var question = await _repo.GetByIdAsync(dto.Id);
            if (question == null) return;

            question.Content = dto.Content;
            question.QuestionTypeId = dto.QuestionTypeId;

            // delete old data
            var oldOptions = question.QuestionOptions.ToList();
            _context.QuestionOptions.RemoveRange(oldOptions);
            await _context.SaveChangesAsync();

            // recreate
            foreach (var opt in dto.Options)
            {
                var option = new QuestionOption
                {
                    QuestionId = question.Id,
                    Content = opt.Content
                };

                _context.QuestionOptions.Add(option);
                await _context.SaveChangesAsync();

                foreach (var cp in opt.CareerPaths)
                {
                    _context.OptionCareerPaths.Add(new OptionCareerPath
                    {
                        OptionId = option.Id,
                        CareerPathId = cp.CareerPathId,
                        Weight = cp.Weight
                    });
                }
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ================= DELETE =================
    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        await _repo.SaveChangesAsync();
    }
}