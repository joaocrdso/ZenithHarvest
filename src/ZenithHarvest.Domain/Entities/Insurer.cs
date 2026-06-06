namespace ZenithHarvest.Domain.Entities;

public class Insurer
{
    public int Id { get; set; }
    public string CNPJ { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string CodigoSUSEP { get; set; } = null!;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Policy> Policies { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];
}
