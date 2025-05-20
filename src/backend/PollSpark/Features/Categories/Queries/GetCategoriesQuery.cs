using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using PollSpark.Data;
using PollSpark.DTOs;
using PollSpark.Features.Polls.Commands;
using PollSpark.Models;

namespace PollSpark.Features.Categories.Queries;

public record GetCategoriesQuery : IRequest<OneOf<List<CategoryDto>, ValidationError>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, OneOf<List<CategoryDto>, ValidationError>>
{
    private readonly PollSparkContext _context;

    public GetCategoriesQueryHandler(PollSparkContext context)
    {
        _context = context;
    }

    public async Task<OneOf<List<CategoryDto>, ValidationError>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToList();
    }
} 