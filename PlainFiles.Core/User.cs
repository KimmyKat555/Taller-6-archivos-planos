namespace PlainFiles.Core;

public class User
{
    // Campo 1: Usuario (jzuluaga)
    public string Username { get; set; } = string.Empty;

    // Campo 2: Contraseña (P@ssw0rd123!)
    public string Password { get; set; } = string.Empty;

    // Campo 3: Estado Activo (true/false)
    public bool IsActive { get; set; }
}