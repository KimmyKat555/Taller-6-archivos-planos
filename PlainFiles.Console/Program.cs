using PlainFiles.Core;
using System.IO; // Necesario para trabajar con archivos como Users.txt

public class Program
{
    // ----------------------------------------------------
    // 1. VARIABLES ESTÁTICAS GLOBALES (Ahora son miembros estáticos de la clase Program)
    // ----------------------------------------------------
    static List<Person> people = new List<Person>();
    static ManualCsvHelper manualCsv = new ManualCsvHelper();
    static string? listName = null;
    static string? LoggedInUser = null;

    // ----------------------------------------------------
    // 2. PUNTO DE ENTRADA PRINCIPAL (Main)
    // ----------------------------------------------------
    public static void Main(string[] args)
    {
        // Cargar usuarios y ejecutar autenticación
        var userList = LoadUsers();

        if (!Authenticate(userList))
        {
            Console.WriteLine("Saliendo del programa por fallas de autenticación.");
            return; // Detiene el programa si falla la autenticación
        }

        Console.Write("Digite el nombre de la lista (por defecto 'people'): ");
        listName = Console.ReadLine();
        if (string.IsNullOrEmpty(listName))
        {
            listName = "people";
        }

        var helper = new NugetCsvHelper();
        try
        {
            // Cargar los datos como List<Person>
            people = helper.Read($"{listName}.csv").ToList();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Advertencia: El archivo '{listName}.csv' no se encontró. Se creará una lista vacía.");
            people = new List<Person>();
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Error al cargar el archivo: {ex.Message}");
             people = new List<Person>();
        }

        foreach (var person in people)
        {
            Console.WriteLine($"ID: {person.Id}, Nombre: {person.Name}, Balance: {person.Balance:C}");
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
                    Console.WriteLine("Archivo guardado.");
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
                    Console.WriteLine("Saliendo...");
                    break;

                default:
                    Console.WriteLine("Opción no válida.");
                    break;
            }
        } while (option != "0");
    } // Fin de Main

    // ----------------------------------------------------
    // 3. FUNCIONES AUXILIARES DE PERSONAS
    // ----------------------------------------------------

    static void SortData()
    {
        int order;
        do
        {
            Console.Write("Por cual campo desea ordenar 0. Nombre, 1. Apellido, 2. Saldo? ");
            var orderString = Console.ReadLine();
            int.TryParse(orderString, out order);
            if (order < 0 || order > 2)
            {
                Console.WriteLine("Orden no válido. Intente de nuevo.");
            }
        } while (order < 0 || order > 2);

        int type;
        do
        {
            Console.Write("Desea ordenar 0. Ascendente, 1. Descendente?");
            var typeString = Console.ReadLine();
            int.TryParse(typeString, out type);
            if (type < 0 || type > 1)
            {
                Console.WriteLine("Orden no válido. Intente de nuevo.");
            }
        } while (type < 0 || type > 1);

        people.Sort((a, b) =>
        {
            int cmp;
            if (order == 2) // Saldo (Balance)
            {
                cmp = a.Balance.CompareTo(b.Balance);
            }
            else if (order == 1) // Apellido
            {
                cmp = string.Compare(a.LastName, b.LastName, StringComparison.OrdinalIgnoreCase);
            }
            else // order == 0: Nombre
            {
                cmp = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            }

            return type == 0 ? cmp : -cmp; // 0 = ascendente, 1 = descendente
        });

        Console.WriteLine("Datos ordenados.");
    }

    static void ListPeople()
    {
        Console.WriteLine("\n--- Lista de Personas ---");
        Console.WriteLine($"{"ID",-5}|{"Nombres",-15}|{"Apellidos",-15}|{"Teléfono",-15}|{"Ciudad",-15}|{"Balance",-10}");
        Console.WriteLine(new string('-', 80));
        foreach (var person in people)
        {
            Console.WriteLine($"{person.Id,-5}|{person.Name,-15}|{person.LastName,-15}|{person.Phone,-15}|{person.City,-15}|{person.Balance,-10:C}");
        }
    }

    static void AddPerson()
    {
        Console.WriteLine("\n--- Adicionar Persona ---");

        // 1. Validar ID (Único y Numérico)
        int newId = 0;
        bool idIsValid = false;
        do
        {
            Console.Write("Digite el ID: ");
            if (int.TryParse(Console.ReadLine(), out newId))
            {
                if (people.Any(p => p.Id == newId))
                {
                    Console.WriteLine("Error: El ID ya existe. Debe ser único.");
                }
                else
                {
                    idIsValid = true;
                }
            }
            else
            {
                Console.WriteLine("Error: El ID debe ser un número.");
            }
        } while (!idIsValid);

        // 2. Validar Nombres (No vacío)
        string name = string.Empty;
        do
        {
            Console.Write("Digite el Nombre: ");
            name = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Error: Debe ingresar el nombre.");
            }
        } while (string.IsNullOrWhiteSpace(name));

        // 3. Validar Apellidos (No vacío)
        string lastName = string.Empty;
        do
        {
            Console.Write("Digite el Apellido: ");
            lastName = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(lastName))
            {
                Console.WriteLine("Error: Debe ingresar el apellido.");
            }
        } while (string.IsNullOrWhiteSpace(lastName));

        // 4. Validar Teléfono (Válido/Numérico)
        string phone = string.Empty;
        do
        {
            Console.Write("Digite el Teléfono: ");
            phone = Console.ReadLine() ?? string.Empty;
            // Validación básica: solo contiene dígitos y tiene una longitud razonable (7-15)
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{7}$"))
            {
                Console.WriteLine("Error: El teléfono debe contener solo números y tener una longitud válida de 7 dígitos. Try again");
            }
        } while (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{7}$"));

        // 5. Ciudad 
        Console.Write("Digite la Ciudad: ");
        string city = Console.ReadLine() ?? string.Empty;

        // 6. Validar Saldo/Balance (Numérico Positivo)
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
                Console.WriteLine("Error: El saldo debe ser un número positivo o cero.");
            }
        } while (!balanceIsValid);

        // Crear la nueva persona
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
        Console.WriteLine("Persona añadida exitosamente.");

        // Registrar en el Log
        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Añadir Persona: ID={newId}, Nombre={name}");
        }
    }

    static void DeletePerson()
    {
        Console.Write("Digite el ID de la persona a borrar: ");
        if (!int.TryParse(Console.ReadLine(), out var idToDelete))
        {
            Console.WriteLine("Error: Debe ingresar un ID numérico.");
            return;
        }

        var personToRemove = people.FirstOrDefault(p => p.Id == idToDelete);

        if (personToRemove == null)
        {
            Console.WriteLine($"No se encontró la persona con ID: {idToDelete}.");
            return;
        }

        Console.WriteLine("\nDatos de la persona a eliminar:");
        Console.WriteLine($"ID: {personToRemove.Id}, Nombre: {personToRemove.Name} {personToRemove.LastName}, Ciudad: {personToRemove.City}, Saldo: {personToRemove.Balance:C}");
        Console.Write("¿Desea borrar esta persona (Y/N)?");

        if (Console.ReadLine()?.ToUpper() == "Y")
        {
            people.Remove(personToRemove);
            Console.WriteLine("Persona eliminada.");

            // Registrar en el Log
            using (var logger = new LogWriter("log.txt"))
            {
                logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Eliminar Persona: ID={idToDelete}, Nombre={personToRemove.Name}");
            }
        }
        else
        {
            Console.WriteLine("Operación cancelada.");
        }
    }
    
    // Implementación de la Edición (Opción 6)
    static void EditPerson()
    {
        Console.WriteLine("\n--- Editar Persona ---");
        Console.Write("Digite el ID de la persona a editar: ");

        if (!int.TryParse(Console.ReadLine(), out var idToEdit))
        {
            Console.WriteLine("Error: Debe ingresar un ID numérico.");
            return;
        }

        var personToEdit = people.FirstOrDefault(p => p.Id == idToEdit);

        if (personToEdit == null)
        {
            Console.WriteLine($"No se encontró la persona con ID: {idToEdit}.");
            return;
        }

        Console.WriteLine($"\nEditando a {personToEdit.Name} {personToEdit.LastName}. Presione ENTER para mantener el valor actual.");

        // --- 1. Nombre ---
        Console.Write($"Nombre actual ({personToEdit.Name}): ");
        var newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName))
        {
            personToEdit.Name = newName;
        }

        // --- 2. Apellido ---
        Console.Write($"Apellido actual ({personToEdit.LastName}): ");
        var newLastName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newLastName))
        {
            personToEdit.LastName = newLastName;
        }

        // --- 3. Teléfono --- (Validación con opción de dejar vacío)
        string newPhone = string.Empty;
        bool phoneIsValid = false;
        do
        {
            Console.Write($"Teléfono actual ({personToEdit.Phone}): ");
            newPhone = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(newPhone))
            {
                phoneIsValid = true; // Permite dejar vacío (mantener valor anterior)
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(newPhone, @"^\d{7}$"))
            {
                personToEdit.Phone = newPhone;
                phoneIsValid = true;
            }
            else
            {
                Console.WriteLine("Error: El teléfono debe contener solo números y tener una longitud válida de 7 digitos. Intente de nuevo.");
            }
        } while (!phoneIsValid);

        // --- 4. Ciudad ---
        Console.Write($"Ciudad actual ({personToEdit.City}): ");
        var newCity = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newCity))
        {
            personToEdit.City = newCity;
        }

        // --- 5. Saldo/Balance --- (Validación con opción de dejar vacío)
        decimal newBalance = 0;
        bool balanceIsValid = false;
        do
        {
            Console.Write($"Saldo actual ({personToEdit.Balance:C}): ");
            var balanceString = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(balanceString))
            {
                balanceIsValid = true; // Permite dejar vacío (mantener valor anterior)
            }
            else if (decimal.TryParse(balanceString, out newBalance) && newBalance >= 0)
            {
                personToEdit.Balance = newBalance;
                balanceIsValid = true;
            }
            else
            {
                Console.WriteLine("Error: El saldo debe ser un número positivo o cero. Intente de nuevo.");
            }
        } while (!balanceIsValid);

        Console.WriteLine("Persona actualizada exitosamente.");

        // Registrar en el Log
        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Editar Persona: ID={idToEdit}, Nombre={personToEdit.Name}");
        }
    }

    // Implementación del Informe por Ciudad (Opción 7)
    static void ShowReportByCity()
    {
        Console.WriteLine("\n--- Informe de Saldo por Ciudad ---");

        // Agrupamos y ordenamos por ciudad
        var report = people
            .GroupBy(p => p.City)
            .OrderBy(g => g.Key) 
            .ToList();

        decimal totalGeneral = 0;

        foreach (var cityGroup in report)
        {
            // Calculamos el subtotal por la ciudad
            decimal subtotal = cityGroup.Sum(p => p.Balance);
            totalGeneral += subtotal;

            Console.WriteLine($"\nCiudad: {cityGroup.Key}");
            Console.WriteLine($"{"ID",-5}|{"Nombres",-15}|{"Apellidos",-15}|{"Saldo",-10}");
            Console.WriteLine(new string('-', 45));

            // Listar personas de esta ciudad
            foreach (var person in cityGroup.OrderBy(p => p.Name))
            {
                Console.WriteLine($"{person.Id,-5}|{person.Name,-15}|{person.LastName,-15}|{person.Balance,-10:C}");
            }

            // Mostrar Subtotal
            Console.WriteLine(new string('=', 45));
            Console.WriteLine($"Total: {cityGroup.Key,-20}{subtotal,-20:C}");
        }

        // Mostrar Total General
        Console.WriteLine($"\n{new string('=', 45)}");
        Console.WriteLine($"Total General: {"",-19}{totalGeneral,-20:C}");
        Console.WriteLine(new string('=', 45));

        // Registrar en el Log
        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Generó el informe de subtotales por ciudad.");
        }
    }

    static string MyMenu()
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("1. Adicionar.");
        Console.WriteLine("2. Mostrar.");
        Console.WriteLine("3. Grabar.");
        Console.WriteLine("4. Eliminar.");
        Console.WriteLine("5. Ordenar.");
        Console.WriteLine("6. Editar.");
        Console.WriteLine("7. Informe por Ciudad.");
        Console.WriteLine("0. Salir.");
        Console.Write("Seleccione una opción: ");
        return Console.ReadLine() ?? string.Empty;
    }

    // Acepta List<Person> y convierte a líneas de texto para grabar, incluyendo el encabezado.
    static void SaveFile(List<Person> peopleToSave, string? listName)
    {
        var lines = new List<string>();

        // 1. Agregar el Encabezado (HEADER)
        // Esto asegura que la primera línea siempre sea el formato que NugetCsvHelper espera.
        lines.Add("Id,Name,LastName,Phone,City,Balance");

        // 2. Mapear List<Person> a líneas de texto CSV
        foreach (var person in peopleToSave)
        {
            // Nota: Aquí no usamos el ManualCsvHelper. Escribimos directamente en el formato CSV.
            // Asegúrate de que los datos no tengan comas internas.
            lines.Add($"{person.Id},{person.Name},{person.LastName},{person.Phone},{person.City},{person.Balance}");
        }

        // 3. Escribir todas las líneas (Encabezado + Datos) al archivo, sobrescribiendo el contenido anterior.
        try
        {
            File.WriteAllLines($"{listName}.csv", lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al escribir en el archivo: {ex.Message}");
            // No registramos en el log porque falló la escritura.
            return;
        }

        Console.WriteLine("Archivo guardado.");

        // Registrar en el Log
        using (var logger = new LogWriter("log.txt"))
        {
            logger.WriteLog(LoggedInUser ?? "SYSTEM", "INFO", $"Archivo '{listName}.csv' guardado.");
        }
    }

    // ----------------------------------------------------
    // 4. FUNCIONES DE AUTENTICACIÓN
    // ----------------------------------------------------

    static List<User> LoadUsers(string path = "Users.txt")
    {
        var users = new List<User>();
        if (!File.Exists(path))
        {
            // Creamos un usuario por defecto si no existe el archivo
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
            Console.WriteLine($"Error al leer Users.txt: {ex.Message}");
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
        Console.WriteLine("     MÓDULO DE AUTENTICACIÓN     ");
        Console.WriteLine("===============================");

        while (attempts < maxAttempts)
        {
            Console.Write("Usuario: ");
            string username = Console.ReadLine() ?? string.Empty;

            // Versión Segura (Usa tu función ReadPassword() para ocultar la clave)
            Console.Write("Contraseña: ");
            string password = ReadPassword() ?? string.Empty; // <-- Usar ReadPassword()

            User? user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                if (!user.IsActive)
                {
                    Console.WriteLine("Error: Este usuario está bloqueado. Contacte al administrador.");
                    return false;
                }

                if (user.Password == password)
                {
                    // AUTENTICACIÓN EXITOSA
                    Console.WriteLine("\n¡Bienvenido!");
                    LoggedInUser = user.Username; 
                    return true;
                }
            }

            // Credenciales incorrectas o usuario no encontrado/inactivo
            attempts++;
            if (attempts < maxAttempts)
            {
                Console.WriteLine($"Error de autenticación. Intento {attempts} de {maxAttempts}.");
            }
            else
            {
                Console.WriteLine($"\nHa excedido los {maxAttempts} intentos.");

                if (user != null)
                {
                    Console.WriteLine($"El usuario '{username}' ha sido bloqueado en el sistema.");
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

                // Oculta el carácter pero permite backspace
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
                        Console.Write("\b \b"); // Mueve el cursor hacia atrás y borra el *
                    }
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine(); // Salto de línea después de presionar Enter
            return password;
        }
    }
}