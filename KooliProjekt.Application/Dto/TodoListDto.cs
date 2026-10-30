using System.Collections.Generic;

namespace KooliProjekt.Application.Dto
{
    public class TodoListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public IList<TodoItemDto> Items { get; set; }

        public TodoListDto()
        {
            Items = new List<TodoItemDto>();
        }
    }
}
