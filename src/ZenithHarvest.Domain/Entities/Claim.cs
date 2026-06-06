namespace ZenithHarvest.Domain.Entities;

public class Claim
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public decimal NDVIAntes { get; set; }
    public decimal NDVIDepois { get; set; }
    public decimal ValorSinistro { get; set; }
    public string Evento { get; set; } = null!; // Seca, Chuva Excessiva, Geada
    public string Status { get; set; } = "Pendente"; // Pendente, Aprovado, Recusado, Pago
    public DateTime DataOcorrencia { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Policy Policy { get; set; } = null!;
}
