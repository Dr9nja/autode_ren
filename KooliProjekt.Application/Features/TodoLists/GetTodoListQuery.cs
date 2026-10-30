using KooliProjekt.Application.Data;
using KooliProjekt.Application.Dto;
using KooliProjekt.Application.Infrastructure.Results;
using MediatR;

namespace KooliProjekt.Application.Features.TodoLists
{
    public class GetTodoListQuery : IRequest<OperationResult<TodoListDto>>
    {
        public int Id { get; set; }
    }
}
