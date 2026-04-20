namespace AmericanAirlinesApi.Models
{
    public class Aeronave
    {
        public int Id { get; set; }
        public string Modelo { get; set; } = string.Empty;         // Ex: "Boeing 777", "Airbus A320"
        public string CodigoCauda { get; set; } = string.Empty;    // Ex: "N789AA"
        public int CapacidadePassageiros { get; set; }

        // Navegação
        public ICollection<Voo> Voos { get; set; } = new List<Voo>();
    }
}
