namespace ZenithHarvest.Domain.Entities;

public class Policy
{
    public int Id { get; set; }
    public int InsurerId { get; set; }
    public string NumeroApolice { get; set; } = null!;
    public DateTime VigenciaInicio { get; set; }
    public DateTime VigenciaFim { get; set; }
    public decimal Premio { get; set; }
    public string Status { get; set; } = "Ativa"; // Ativa, Cancelada, Expirada
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Insurer Insurer { get; set; } = null!;
    public ICollection<Claim> Claims { get; set; } = [];
}
