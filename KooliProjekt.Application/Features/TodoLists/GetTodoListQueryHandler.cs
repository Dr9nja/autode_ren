using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.Application.Features.TodoLists
{
    public class GetTodoListQueryHandler : IRequestHandler<GetTodoListQuery, OperationResult<TodoListDto>>
    {
        private readonly ApplicationDbContext _dbContext;

        public GetTodoListQueryHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<TodoListDto>> Handle(GetTodoListQuery request, CancellationToken cancellationToken)
        {
            var response = new OperationResult<TodoListDto>();
            response.Value = await _dbContext.TodoLists
                .AsNoTracking()
                .Include(list => list.Items)
                .Include(list => list.User)
                .Where(list => list.Id == request.Id)                
                .Select(list => new TodoListDto
                {
                    Id = list.Id,
                    Title = list.Title,
                    UserName = list.User.UserName,
                    // Selle võib teha ka eraldi päringuga
                    Items = list.Items.Select(item => new TodoItemDto
                    {
                        Id = item.Id,
                        Title = item.Title,
                        IsCompleted = item.IsCompleted
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            return response;
        }
    }
}
