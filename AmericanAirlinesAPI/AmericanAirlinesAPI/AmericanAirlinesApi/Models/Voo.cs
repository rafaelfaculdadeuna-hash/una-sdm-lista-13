namespace AmericanAirlinesApi.Models
{
    public class Voo
    {
        public int Id { get; set; }
        public string CodigoVoo { get; set; } = string.Empty;      // Ex: "AA123"
        public string Origem { get; set; } = string.Empty;         // Ex: "GRU" (Guarulhos)
        public string Destino { get; set; } = string.Empty;        // Ex: "DFW" (Dallas)
        public int AeronavId { get; set; }                         // FK para Aeronave
        public string Status { get; set; } = "Agendado";           // [Agendado, Em Voo, Finalizado, Cancelado]

        // Navegação
        public Aeronave? Aeronave { get; set; }
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
