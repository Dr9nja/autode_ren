using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.Application.Features.TodoLists
{
    internal class ListTodoListsQueryHandler : IRequestHandler<ListToDoListsQuery, OperationResult<PagedResult<TodoListItemDto>>>
    {
        private readonly ApplicationDbContext _dbContext;

        public ListTodoListsQueryHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<PagedResult<TodoListItemDto>>> Handle(ListToDoListsQuery request, CancellationToken cancellationToken)
        {
            var result = new OperationResult<PagedResult<TodoListItemDto>>();
            result.Value = await _dbContext.TodoLists
                .AsNoTracking()
                .OrderByDescending(list => list.Id)
                .Select(list => new TodoListItemDto
                {
                    Id = list.Id,
                    Title = list.Title,
                    UserName = list.User.UserName
                })
                .GetPagedAsync(request.Page, request.PageSize);

            return result;
        }
    }
}
