namespace AmericanAirlinesApi.Models
{
    public class Tripulante
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Funcao { get; set; } = string.Empty;         // [Piloto, Copiloto, Comissário]
        public string NumeroLicenca { get; set; } = string.Empty;
    }
}
