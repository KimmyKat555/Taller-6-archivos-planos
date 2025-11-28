namespace PlainFiles.Core;

public class Person
{
    public int Id { get; set; } // Requerido: ID
    public string Name { get; set; } = string.Empty; // Nombres 
    public string LastName { get; set; } = string.Empty; // Apellidos 
    public string Phone { get; set; } = string.Empty; // Teléfono 
    public string City { get; set; } = string.Empty; // Ciudad 
    public decimal Balance { get; set; } // Saldo/Balance (decimal para valores monetarios) 
}