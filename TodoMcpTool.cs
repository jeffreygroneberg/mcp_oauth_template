using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace MyMcpServer.McpTools
{
    [McpServerToolType]
    public class TodoMcpTool
    {
        private static readonly List<Todo> _todos = new();
        private static int _nextId = 1;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TodoMcpTool(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUsername()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Debug: Log all claims to console
                Console.WriteLine("=== JWT TOKEN DEBUG ===");
                Console.WriteLine(
                    $"User.Identity.IsAuthenticated: {user.Identity.IsAuthenticated}"
                );
                Console.WriteLine($"User.Identity.Name: {user.Identity.Name}");
                Console.WriteLine(
                    $"User.Identity.AuthenticationType: {user.Identity.AuthenticationType}"
                );
                Console.WriteLine("All Claims:");
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
                Console.WriteLine("=== END JWT DEBUG ===");

                // Try to get the name claim (preferred username or name)
                var username =
                    user.FindFirst("preferred_username")?.Value
                    ?? user.FindFirst("upn")?.Value
                    ?? user.FindFirst("name")?.Value
                    ?? user.FindFirst(ClaimTypes.Name)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? "Unknown User";

                Console.WriteLine($"Selected username: {username}");
                return username;
            }
            Console.WriteLine("User is not authenticated");
            return "Anonymous";
        }

        [McpServerTool]
        [Description("Creates a new todo item")]
        public Task<string> CreateTodoAsync(
            [Description("Description of the todo")] string description,
            [Description("Priority level")] string priority = "medium"
        )
        {
            var currentUser = GetCurrentUsername();
            var todo = new Todo
            {
                Id = _nextId++,
                Description = description,
                Priority = priority,
                CreatedDate = DateTime.UtcNow,
                IsCompleted = false,
                CreatedBy = currentUser,
            };

            _todos.Add(todo);
            return Task.FromResult(
                $"Todo created: {todo.Description} (Id: {todo.Id}) by {currentUser}"
            );
        }

        [McpServerTool]
        [Description("Gets all todos or a specific todo by ID")]
        public Task<object> GetTodosAsync(
            [Description("Optional todo ID to get specific todo")] int? id = null
        )
        {
            var currentUser = GetCurrentUsername();

            if (id.HasValue)
            {
                var todo = _todos.FirstOrDefault(t => t.Id == id.Value);
                if (todo != null)
                {
                    return Task.FromResult<object>(
                        new
                        {
                            todo.Id,
                            todo.Description,
                            todo.Priority,
                            todo.CreatedDate,
                            todo.IsCompleted,
                            todo.CreatedBy,
                            RequestedBy = currentUser,
                        }
                    );
                }
                return Task.FromResult<object>(
                    new { Message = $"Todo with ID {id} not found", RequestedBy = currentUser }
                );
            }

            var todos = _todos
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Description,
                    t.Priority,
                    t.CreatedDate,
                    t.IsCompleted,
                    t.CreatedBy,
                })
                .ToList();

            return Task.FromResult<object>(
                new
                {
                    Todos = todos,
                    RequestedBy = currentUser,
                    TotalCount = todos.Count,
                }
            );
        }

        [McpServerTool]
        [Description("Updates a todo item")]
        public Task<string> UpdateTodoAsync(
            [Description("ID of the todo to update")] int id,
            [Description("New description (optional)")] string? description = null,
            [Description("New priority (optional)")] string? priority = null,
            [Description("Mark as completed (optional)")] bool? isCompleted = null
        )
        {
            var currentUser = GetCurrentUsername();
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
                return Task.FromResult($"Todo with ID {id} not found (requested by {currentUser})");

            if (!string.IsNullOrEmpty(description))
                todo.Description = description;
            if (!string.IsNullOrEmpty(priority))
                todo.Priority = priority;
            if (isCompleted.HasValue)
                todo.IsCompleted = isCompleted.Value;

            return Task.FromResult(
                $"Todo {id} updated successfully by {currentUser} (originally created by {todo.CreatedBy})"
            );
        }

        [McpServerTool]
        [Description("Deletes a todo item")]
        public Task<string> DeleteTodoAsync([Description("ID of the todo to delete")] int id)
        {
            var currentUser = GetCurrentUsername();
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
                return Task.FromResult($"Todo with ID {id} not found (requested by {currentUser})");

            _todos.Remove(todo);
            return Task.FromResult(
                $"Todo {id} deleted successfully by {currentUser} (originally created by {todo.CreatedBy})"
            );
        }
    }

    public class Todo
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "medium";
        public DateTime CreatedDate { get; set; }
        public bool IsCompleted { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
