using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmericanAirlinesApi.Data;

namespace AmericanAirlinesApi.Controllers
{
    [ApiController]
    [Route("api/radar")]
    public class FlightRadarController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FlightRadarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/radar/proximosdestinos
        // Desafio de Sistemas Distribuídos:
        // Agrupa todos os voos ativos e retorna quantos voos estão indo para cada destino.
        // Thread.Sleep(2000) simula a latência de consulta a um serviço externo de satélite.
        [HttpGet("proximosdestinos")]
        public async Task<ActionResult<object>> GetProximosDestinos()
        {
            // Simulação de latência: busca em serviço externo de satélite (2 segundos)
            Thread.Sleep(2000);

            var voosAtivos = await _context.Voos
                .Where(v => v.Status == "Agendado" || v.Status == "Em Voo")
                .GroupBy(v => v.Destino)
                .Select(g => new
                {
                    Destino = g.Key,
                    TotalVoos = g.Count()
                })
                .OrderByDescending(x => x.TotalVoos)
                .ToListAsync();

            return Ok(new
            {
                mensagem = "Dados obtidos via serviço de satélite (simulação).",
                totalDestinos = voosAtivos.Count,
                destinos = voosAtivos
            });
        }
    }
}
