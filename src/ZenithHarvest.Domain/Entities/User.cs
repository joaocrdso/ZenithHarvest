namespace ZenithHarvest.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public int InsurerId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "Analista"; // Analista, Gerente, Admin
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Insurer Insurer { get; set; } = null!;
}
