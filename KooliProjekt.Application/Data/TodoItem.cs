using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        public bool IsCompleted { get; set; }

        public TodoList TodoList { get; set; }
        public int TodoListId { get; set; }
    }
}
