using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KooliProjekt.Application.Data
{
    public class TodoList
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public User User { get; set; }
        public int UserId { get; set; }

        public IList<TodoItem> Items { get; set; }

        public TodoList()
        {
            Items = new List<TodoItem>();
        }
    }
}
