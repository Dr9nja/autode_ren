using KooliProjekt.Application.Data;
using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Paging;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;

namespace KooliProjekt.Application.Features.TodoLists
{
    public class ListToDoListsQuery : IRequest<OperationResult<PagedResult<TodoListItemDto>>>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}