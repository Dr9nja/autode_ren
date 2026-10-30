using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;

namespace KooliProjekt.Application.Data
{
    public static class SeedData
    {

        public static void Generate(ApplicationDbContext context)
        {
            if(context.Users.Any())
            {
                return;
            }

            SeedUsers(context);
            SeedTodoLists(context);
            SeedTodoItems(context);
        }

        private static void SeedUsers(ApplicationDbContext context)
        {
            var users = new List<User>
            {
                new User { UserName = "user1", PasswordHash = "password1" },
                new User { UserName = "user2", PasswordHash = "password2" }
            };
            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static void SeedTodoLists(ApplicationDbContext context)
        {
            for(var i = 1; i <= 32; i++)
            {
                var list = new TodoList { Title = "Todo List " + i, UserId = 1 };
                context.Add(list);
            }

            context.SaveChanges();
        }

        private static void SeedTodoItems(ApplicationDbContext context)
        {
            var todoItems = new List<TodoItem>
            {
                new TodoItem { Title = "Todo Item 1", IsCompleted = false, TodoListId = 1 },
                new TodoItem { Title = "Todo Item 2", IsCompleted = true, TodoListId = 1 },
                new TodoItem { Title = "Todo Item 3", IsCompleted = false, TodoListId = 2 }
            };
            context.TodoItems.AddRange(todoItems);
            context.SaveChanges();
        }
    }
}
