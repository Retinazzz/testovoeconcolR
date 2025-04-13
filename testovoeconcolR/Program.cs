using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace testovoeconcolR
{
    public enum UserRole
    {
        Manager,
        Employee
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done
    }

    public class User
    {
        public string Username;
        public string Password;
        public UserRole Role;
    }

    public class Task
    {
        public int ProjectId;
        public string Title;
        public string Description;
        public string AssignedTo;
        public TaskStatus Status  = TaskStatus.ToDo;
    }

    public class ProjectManager
    {
        private List<User> users;
        private List<Task> tasks;
        private User currentUser;
        private const string UsersFile = "users.json";
        private const string TasksFile = "tasks.json";
        
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        private bool UserCanLogin(User user, string username, string password)
        {
            return user.Username == username && user.Password == password;
        }

        private bool IsEmployee(User user)
        {
            return user.Role == UserRole.Employee;
        }

        private bool IsAssignedToCurrentUser(Task task)
        {
            return task.AssignedTo == currentUser.Username;
        }
        public ProjectManager()
        {
            LoadData();
        }

        private void SaveData()
        {
            try
            {
                string usersJson = JsonConvert.SerializeObject(users, jsonSettings);
                File.WriteAllText("users.json", usersJson);

                string tasksJson = JsonConvert.SerializeObject(tasks, jsonSettings);
                File.WriteAllText("tasks.json", tasksJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void LoadData()
        {
            try
            {                
                if (File.Exists(UsersFile))
                {
                    string json = File.ReadAllText(UsersFile);
                    users = JsonConvert.DeserializeObject<List<User>>(json);
                }
                else
                {
                    users = new List<User>
                    {
                        new User { Username = "admin", Password = "admin", Role = UserRole.Manager }
                    };
                }
                if (File.Exists(TasksFile))
                {
                    string json = File.ReadAllText(TasksFile);
                    tasks = JsonConvert.DeserializeObject<List<Task>>(json);
                }
                else
                {
                    tasks = new List<Task>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                users = new List<User>();
                tasks = new List<Task>();
            }
        }

        public bool Login(string username, string password)
        {
            foreach (User user in users)
            {
                if (UserCanLogin(user, username, password))
                {
                    currentUser = user;
                    return true;
                }
            }
            return false;
        }

        public void Logout()
        {
            currentUser = null;
        }

        public void Run()
        {
            Console.WriteLine("Система управления проектами");

            while (true)
            {
                if (currentUser == null)
                {
                    Console.Write("Логин: ");
                    var username = Console.ReadLine();
                    Console.Write("Пароль: ");
                    var password = Console.ReadLine();

                    if (!Login(username, password))
                    {
                        Console.WriteLine("Неверный логин или пароль");
                        continue;
                    }
                }

                Console.WriteLine($"\nДобро пожаловать, {currentUser.Username} ({currentUser.Role})");

                if (currentUser.Role == UserRole.Manager)
                {
                    ShowManagerMenu();
                }
                else
                {
                    ShowEmployeeMenu();
                }
            }
        }

        private void ShowManagerMenu()
        {
            while (true)
            {
                Console.WriteLine("\nМеню управляющего:");
                Console.WriteLine("1. Создать задачу");
                Console.WriteLine("2. Зарегистрировать нового сотрудника");
                Console.WriteLine("3. Просмотреть все задачи");
                Console.WriteLine("4. Выйти из системы");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateTask();
                        break;
                    case "2":
                        RegisterUser();
                        break;
                    case "3":
                        ViewAllTasks();
                        break;
                    case "4":
                        Logout();
                        return;
                    default:
                        Console.WriteLine("Неверный выбор");
                        break;
                }
            }
        }

        private void ShowEmployeeMenu()
        {
            while (true)
            {
                Console.WriteLine("\nМеню сотрудника:");
                Console.WriteLine("1. Просмотреть мои задачи");
                Console.WriteLine("2. Изменить статус задачи");
                Console.WriteLine("3. Выйти из системы");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewMyTasks();
                        break;
                    case "2":
                        ChangeTaskStatus();
                        break;
                    case "3":
                        Logout();
                        return;
                    default:
                        Console.WriteLine("Неверный выбор");
                        break;
                }
            }
        }

        private void CreateTask()
        {
            Console.WriteLine("\nСоздание новой задачи:");

            Console.Write("ID проекта: ");
            if (!int.TryParse(Console.ReadLine(), out int projectId))
            {
                Console.WriteLine("Неверный ID проекта");
                return;
            }

            Console.Write("Название задачи: ");
            var title = Console.ReadLine();

            Console.Write("Описание задачи: ");
            var description = Console.ReadLine();

            Console.WriteLine("Доступные сотрудники:");
            foreach (User user in users)
            {
                if (IsEmployee(user))
                {
                    Console.WriteLine($"- {user.Username}");
                }
            }

            Console.Write("Назначить на: ");
            var assignedTo = Console.ReadLine();

            bool employeeFound = false;
            foreach (User user in users)
            {
                if (user.Username == assignedTo && IsEmployee(user))
                {
                    employeeFound = true;
                    break;
                }
            }

            if (!employeeFound)
            {
                Console.WriteLine("Сотрудник не найден");
                return;
            }

            var task = new Task
            {
                ProjectId = projectId,
                Title = title,
                Description = description,
                AssignedTo = assignedTo
            };

            tasks.Add(task);
            SaveData();
            Console.WriteLine("Задача успешно создана");
        }

        private void RegisterUser()
        {
            Console.WriteLine("\nРегистрация нового пользователя:");

            Console.Write("Логин: ");
            var username = Console.ReadLine();

            foreach (User user in users)
            {
                if (user.Username == username)
                {
                    Console.WriteLine("Пользователь с таким логином уже существует");
                    return;
                }
            }

            Console.Write("Пароль: ");
            var password = Console.ReadLine();

            Console.Write("Роль (1 - Управляющий, 2 - Сотрудник): ");
            var roleChoice = Console.ReadLine();

            UserRole role;
            if (roleChoice == "1")
                role = UserRole.Manager;
            else if (roleChoice == "2")
                role = UserRole.Employee;
            else
            {
                Console.WriteLine("Неверный выбор роли");
                return;
            }

            users.Add(new User
            {
                Username = username,
                Password = password,
                Role = role
            });

            SaveData();
            Console.WriteLine("Пользователь успешно зарегистрирован");
        }

        private void ViewAllTasks()
        {
            Console.WriteLine("\nВсе задачи:");
            foreach (var task in tasks)
            {
                Console.WriteLine($"ID проекта: {task.ProjectId}");
                Console.WriteLine($"Название: {task.Title}");
                Console.WriteLine($"Описание: {task.Description}");
                Console.WriteLine($"Назначена: {task.AssignedTo}");
                Console.WriteLine($"Статус: {task.Status}");
                Console.WriteLine();
            }
        }
        private List<Task> GetCurrentUserTasks()
        {
            var result = new List<Task>();
            foreach (Task task in tasks)
            {
                if (IsAssignedToCurrentUser(task))
                {
                    result.Add(task);
                }
            }
            return result;
        }

        private void ViewMyTasks()
        {
            Console.WriteLine("\nМои задачи:");
            List<Task> myTasks = GetCurrentUserTasks();

            if (myTasks.Count == 0)
            {
                Console.WriteLine("У вас нет задач");
                return;
            }

            for (int i = 0; i < myTasks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {myTasks[i].Title} ({myTasks[i].Status})");
                Console.WriteLine($"   Проект: {myTasks[i].ProjectId}");
                Console.WriteLine($"   Описание: {myTasks[i].Description}");
                Console.WriteLine();
            }
        }

        private void ChangeTaskStatus()
        {
            List<Task> myTasks = GetCurrentUserTasks();

            if (myTasks.Count == 0)
            {
                Console.WriteLine("У вас нет задач для изменения статуса");
                return;
            }

            ViewMyTasks();

            Console.Write("Выберите номер задачи для изменения статуса: ");
            if (!int.TryParse(Console.ReadLine(), out int taskNumber) || taskNumber < 1 || taskNumber > myTasks.Count)
            {
                Console.WriteLine("Неверный номер задачи");
                return;
            }

            var task = myTasks[taskNumber - 1];

            Console.WriteLine("Выберите новый статус:");
            Console.WriteLine("1. To Do");
            Console.WriteLine("2. In Progress");
            Console.WriteLine("3. Done");

            var statusChoice = Console.ReadLine();
            TaskStatus newStatus;

            switch (statusChoice)
            {
                case "1":
                    newStatus = TaskStatus.ToDo;
                    break;
                case "2":
                    newStatus = TaskStatus.InProgress;
                    break;
                case "3":
                    newStatus = TaskStatus.Done;
                    break;
                default:
                    Console.WriteLine("Неверный выбор статуса");
                    return;
            }
            task.Status = newStatus;
            SaveData();
            Console.WriteLine("Статус задачи успешно изменен");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var system = new ProjectManager();
            system.Run();
        }
    }
}

   
