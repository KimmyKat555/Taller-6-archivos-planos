using PlainFiles.Core;
using System.IO;

public class Program
{

    static List<Person> people = new List<Person>();
    static ManualCsvHelper manualCsv = new ManualCsvHelper();
    static string? listName = null;
    static string? LoggedInUser = null;

    public static void Main(string[] args)
    {
        var userList = LoadUsers();

        if (!Authenticate(userList))
        {
            Console.WriteLine("Exiting the program. Authentication failed.");
            return;
        }

        Console.Write("Write the list name or please ENTER for 'people' by default: ");
        listName = Console.ReadLine();
        if (string.IsNullOrEmpty(listName))
        {
            listName = "people";
        }

        var helper = new NugetCsvHelper();
        try
        {
            people = helper.Read($"{listName}.csv").ToList();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Warning: The file '{listName}.csv' was not found. An empty list will be created.");
            people = new List<Person>();
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Error loading the file: {ex.Message}");
             people = new List<Person>();
        }

        foreach (var person in people)
        {
            Console.WriteLine($"ID: {person.Id}, Name: {person.Name}, Balance: {person.Balance:C}");
        }

        var option = string.Empty;

        do
        {
            option = MyMenu();
            Console.WriteLine();
            Console.WriteLine();
            switch (option)
            {
                case "1":
                    AddPerson();
                    break;

                case "2":
                    ListPeople();
                    break;

                case "3":
                    SaveFile(people, listName);
                    break;

                case "4":
                    DeletePerson();
                    break;

                case "5":
                    SortData();
                    break;

                case "6":
                    EditPerson();
                    break;
                
                case "7":
                    ShowReportByCity();
                    break;

                case "0":
                    Console.WriteLine("Exiting...");
                    break;

                default:
                    Console.WriteLine("Option not valid.");
                    break;
            }
        } while (option != "0");
    }
    static void SortData()
    {
        int order;
        do
        {
            Console.Write("\n0. Name");
            Console.Write("\n1. Last name");
            Console.Write("\n2. Balance");
            Console.Write("\nSelect what you want to order: ");

            var orderString = Console.ReadLine();
            int.TryParse(orderString, out order);
            if (order < 0 || order > 2)
            {
                Console.WriteLine("Invalid order. Try again.");
            }
        } while (order < 0 || order > 2);

        int type;
        do
        {
            Console.Write("\n0. Ascending");
            Console.Write("\n1. Descending");
            Console.Write("\nSelect the way you want to order: ");
            var typeString = Console.ReadLine();
            int.TryParse(typeString, out type);
            if (type < 0 || type > 1)
            {
                Console.WriteLine("Invalid order. Try again.");
            }
        } while (type < 0 || type > 1);

        people.Sort((a, b) =>
        {
            int cmp;
            if (order == 2)
            {
                cmp = a.Balance.CompareTo(b.Balance);
            }
            else if (order == 1)
            {
                cmp = string.Compare(a.LastName, b.LastName, StringComparison.OrdinalIgnoreCase);
            }
            else 
            {
                cmp = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            }

            return type == 0 ? cmp : -cmp; 
        });

        Console.WriteLine("Data sorted.");
    }

    static void ListPeople()
    {
        Console.WriteLine("\n======= List of People =======");

        foreach (var person in people)
        {
            Console.WriteLine($"{person.Id}");

            Console.WriteLine($"  {person.Name} {person.LastName}");

            Console.WriteLine($"  Phone: {person.Phone}");

            Console.WriteLine($"  City: {person.City}");

            Console.WriteLine($"  Balance: \t{person.Balance:C}");

            Console.WriteLine(new string('-', 30));
        }

        Console.WriteLine("================================");
    }

    static void AddPerson()
    {
        Console.WriteLine("\n======= Add Person =======");

        int newId = 0;
        bool idIsValid = false;
        do
        {
            Console.Write("Enter ID: ");
            if (int.TryParse(Console.ReadLine(), out newId))
            {
                if (people.Any(p => p.Id == newId))
                {
                    Console.WriteLine("Error: ID already exists. Must be unique.");
                }
                else
                {
                    idIsValid = true;
                }
            }
            else
            {
                Console.WriteLine("Error: El ID must be a number.");
            }
        } while (!idIsValid);

        string name = string.Empty;
        do
        {
            Console.Write("Enter name: ");
            name = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Error: Name is required.");
            }
        } while (string.IsNullOrWhiteSpace(name));

        string lastName = string.Empty;
        do
        {
            Console.Write("Enter LastName: ");
            lastName = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(lastName))
            {
                Console.WriteLine("Error: LastName is required.");
            }
        } while (string.IsNullOrWhiteSpace(lastName));

        string phone = string.Empty;
        do
        {
            Console.Write("Enter Phone: ");
            phone = Console.ReadLine() ?? string.Empty;

            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{7}$"))
            {
                Console.WriteLine("Error: Phone must contain only numbers and have a valid length of 7 digits. Try again");
            }
        } while (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{7}$"));
 
        Console.Write("Enter the city: ");
        string city = Console.ReadLine() ?? string.Empty;

        decimal balance = 0;
        bool balanceIsValid = false;
        do
        {
            Console.Write("Digite el Saldo (Balance): ");
            if (decimal.TryParse(Console.ReadLine(), out balance) && balance >= 0)
            {
                balanceIsValid = true;
            }
            else
            {
                Console.WriteLine("Error: Balance must be a positive number or zero.");
            }
        } while (!balanceIsValid);

        var newPerson = new Person
        {
            Id = newId,
            Name = name,
            LastName = lastName,
            Phone = phone,
            City = city,
            Balance = balance
        };

        people.Add(newPerson);
        Console.WriteLine("Person added successfully.");

        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Added Person: ID={newId}, Name={name}");
        }
    }

    static void DeletePerson()
    {
        Console.Write("Enter the ID of the person to delete: ");
        if (!int.TryParse(Console.ReadLine(), out var idToDelete))
        {
            Console.WriteLine("Error: ID must be a number.");
            return;
        }

        var personToRemove = people.FirstOrDefault(p => p.Id == idToDelete);

        if (personToRemove == null)
        {
            Console.WriteLine($"Person with ID: {idToDelete}. Was not found.");
            return;
        }

        Console.WriteLine("\nData of the person to delete:");
        Console.WriteLine($"ID: {personToRemove.Id}, Full Name: {personToRemove.Name} {personToRemove.LastName}, City {personToRemove.City}, Balance: {personToRemove.Balance:C}");
        Console.Write("¿Do you want to delete this person (Y/N)? ");

        if (Console.ReadLine()?.ToUpper() == "Y")
        {
            people.Remove(personToRemove);
            Console.WriteLine("Person deleted.");

            using (var logger = new LogWriter("log.txt"))
            {
                logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Deleted Person: ID={idToDelete}, Name={personToRemove.Name}");
            }
        }
        else
        {
            Console.WriteLine("Operation cancelled.");
        }
    }
    static void EditPerson()
    {
        Console.WriteLine("\n======= Edit Person =======");
        Console.Write("Enter the ID of the person to edit: ");

        if (!int.TryParse(Console.ReadLine(), out var idToEdit))
        {
            Console.WriteLine("Error: ID must be a number.");
            return;
        }

        var personToEdit = people.FirstOrDefault(p => p.Id == idToEdit);

        if (personToEdit == null)
        {
            Console.WriteLine($"Person with ID: {idToEdit} Was not found.");
            return;
        }

        Console.WriteLine($"\nEditing {personToEdit.Name} {personToEdit.LastName}. Press ENTER to keep the current value.");

        Console.Write($"Current name ({personToEdit.Name}): ");
        var newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName))
        {
            personToEdit.Name = newName;
        }

        Console.Write($"Current Last Name ({personToEdit.LastName}): ");
        var newLastName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newLastName))
        {
            personToEdit.LastName = newLastName;
        }
        string newPhone = string.Empty;
        bool phoneIsValid = false;
        do
        {
            Console.Write($"Current Phone ({personToEdit.Phone}): ");
            newPhone = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(newPhone))
            {
                phoneIsValid = true;
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(newPhone, @"^\d{7}$"))
            {
                personToEdit.Phone = newPhone;
                phoneIsValid = true;
            }
            else
            {
                Console.WriteLine("Error: Phone must contain only numbers and have a valid length of 7 digits. Try again.");
            }
        } while (!phoneIsValid);

        Console.Write($"Current City ({personToEdit.City}): ");
        var newCity = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newCity))
        {
            personToEdit.City = newCity;
        }

        decimal newBalance = 0;
        bool balanceIsValid = false;
        do
        {
            Console.Write($"Current Balance ({personToEdit.Balance:C}): ");
            var balanceString = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(balanceString))
            {
                balanceIsValid = true;
            }
            else if (decimal.TryParse(balanceString, out newBalance) && newBalance >= 0)
            {
                personToEdit.Balance = newBalance;
                balanceIsValid = true;
            }
            else
            {
                Console.WriteLine("Error: Balance must be a positive number or zero. Try again.");
            }
        } while (!balanceIsValid);

        Console.WriteLine("Person updated successfully.");

        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Edited Person: ID={idToEdit}, Name={personToEdit.Name}");
        }
    }
    static void ShowReportByCity()
    {

        Console.WriteLine("\n======= Report by City =======");

        var report = people
            .GroupBy(p => p.City)
            .OrderBy(g => g.Key)
            .ToList();

        decimal totalGeneral = 0;

        foreach (var cityGroup in report)
        {
            decimal subtotal = cityGroup.Sum(p => p.Balance);
            totalGeneral += subtotal;

            Console.WriteLine($"\nCity: {cityGroup.Key}");

            Console.WriteLine($"{"ID",-6}{"Name",-14}{"LastName",-18}{"Balance",-12}");

            Console.WriteLine($"{"=====",-6}{"===========",-14}{"==============",-18}{"==========",-12}");

            foreach (var person in cityGroup.OrderBy(p => p.Name))
            {
                Console.WriteLine($"{person.Id,-6}{person.Name,-14}{person.LastName,-18}{person.Balance,-12:C}");
            }

            Console.WriteLine($"{"",-38}=========="); 

            Console.WriteLine($"{"Total: " + cityGroup.Key,-38}{subtotal,-12:C}");
        }

        Console.WriteLine($"\n{"",-38}============"); 

        Console.WriteLine($"{"Grand Total:",-38}{totalGeneral,-12:C}");

        Console.WriteLine($"{"",-38}============");

        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Generated balance report by city.");
        }
    }
    static string MyMenu()
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("1. Add.");
        Console.WriteLine("2. Show data.");
        Console.WriteLine("3. Save data.");
        Console.WriteLine("4. Delete");
        Console.WriteLine("5. Order.");
        Console.WriteLine("6. Edit.");
        Console.WriteLine("7. Report by city.");
        Console.WriteLine("0. Exit.");
        Console.Write("Select an option: ");
        return Console.ReadLine() ?? string.Empty;
    }

    static void SaveFile(List<Person> peopleToSave, string? listName)
    {
        var lines = new List<string>();

        lines.Add("Id,Name,LastName,Phone,City,Balance");

        foreach (var person in peopleToSave)
        {

            lines.Add($"{person.Id},{person.Name},{person.LastName},{person.Phone},{person.City},{person.Balance}");
        }
        try
        {
            File.WriteAllLines($"{listName}.csv", lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
            return;
        }

        Console.WriteLine("File saved.");

        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"File '{listName}.csv' saved.");
        }
    }
    static List<User> LoadUsers(string path = "Users.txt")
    {
        var users = new List<User>();
        if (!File.Exists(path))
        {
            users.Add(new User { Username = "admin", Password = "admin", IsActive = true });
            SaveUsers(users); 
            return users;
        }

        try
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = line.Split(',');

                if (fields.Length == 3)
                {
                    users.Add(new User
                    {
                        Username = fields[0].Trim(),
                        Password = fields[1].Trim(),
                        IsActive = bool.TryParse(fields[2].Trim(), out bool active) && active
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading Users.txt: {ex.Message}");
        }
        return users;
    }

    static void SaveUsers(List<User> users, string path = "Users.txt")
    {
        var lines = new List<string>();
        foreach (var user in users)
        {
            lines.Add($"{user.Username},{user.Password},{user.IsActive.ToString().ToLower()}");
        }
        File.WriteAllLines(path, lines);
    }

    static bool Authenticate(List<User> users)
    {
        int maxAttempts = 3;
        int attempts = 0;

        Console.WriteLine("===============================");
        Console.WriteLine("         AUTHENTICATION    ");
        Console.WriteLine("===============================");

        while (attempts < maxAttempts)
        {
            Console.Write("User: ");
            string username = Console.ReadLine() ?? string.Empty;

            Console.Write("Password: ");
            string password = ReadPassword() ?? string.Empty; 

            User? user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                if (!user.IsActive)
                {
                    Console.WriteLine("Error: This user is blocked. Contact administrator.");
                    return false;
                }

                if (user.Password == password)
                {
                    Console.WriteLine("\n¡Welcome");
                    LoggedInUser = user.Username; 
                    return true;
                }
            }

            attempts++;
            if (attempts < maxAttempts)
            {
                Console.WriteLine($"Authentication error. Attempt {attempts} of {maxAttempts}.");
            }
            else
            {
                Console.WriteLine($"\nYou have exceeded the {maxAttempts} attempts.");

                if (user != null)
                {
                    Console.WriteLine($"User '{username}' Has been blocked in the system .");
                    user.IsActive = false; 
                    SaveUsers(users); 
                }

                return false;
            }
        }
        return false;

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }
    }
}