using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;

namespace PollSpark.Features.Categories.Commands;

public record CreateCategoryCommand(
    string Name,
    string Description
) : IRequest<OneOf<CategoryDto, ValidationError>>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, OneOf<CategoryDto, ValidationError>>
{
    private readonly PollSparkContext _context;

    public CreateCategoryCommandHandler(PollSparkContext context)
    {
        _context = context;
    }

    public async Task<OneOf<CategoryDto, ValidationError>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        // Check if category with same name already exists
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingCategory != null)
        {
            return new ValidationError("A category with this name already exists");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }
} 