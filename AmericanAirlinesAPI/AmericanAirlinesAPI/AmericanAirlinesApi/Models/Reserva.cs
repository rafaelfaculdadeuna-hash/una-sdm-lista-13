namespace AmericanAirlinesApi.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int VooId { get; set; }                              // FK para Voo
        public string NomePassageiro { get; set; } = string.Empty;
        public string Assento { get; set; } = string.Empty;        // Ex: "12A"
        public decimal Taxa { get; set; } = 0.00m;                 // Taxa adicional (janela: +$50)

        // Navegação
        public Voo? Voo { get; set; }
    }
}
