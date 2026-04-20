using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmericanAirlinesApi.Data;
using AmericanAirlinesApi.Models;

namespace AmericanAirlinesApi.Controllers
{
    public class TripulanteInput
    {
        public string Nome { get; set; } = string.Empty;
        public string Funcao { get; set; } = string.Empty;
        public string NumeroLicenca { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TripulantesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripulantesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/tripulantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tripulante>>> GetTripulantes()
        {
            return await _context.Tripulantes.ToListAsync();
        }

        // GET: api/tripulantes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Tripulante>> GetTripulante(int id)
        {
            var tripulante = await _context.Tripulantes.FindAsync(id);

            if (tripulante == null)
                return NotFound($"Tripulante com Id {id} não encontrado.");

            return tripulante;
        }

        // POST: api/tripulantes
        [HttpPost]
        public async Task<ActionResult<Tripulante>> PostTripulante(TripulanteInput input)
        {
            var funcoesValidas = new[] { "Piloto", "Copiloto", "Comissário" };

            if (!funcoesValidas.Contains(input.Funcao))
                return BadRequest($"Função inválida. As funções aceitas são: {string.Join(", ", funcoesValidas)}.");

            var tripulante = new Tripulante
            {
                Nome = input.Nome,
                Funcao = input.Funcao,
                NumeroLicenca = input.NumeroLicenca
            };

            _context.Tripulantes.Add(tripulante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTripulante), new { id = tripulante.Id }, tripulante);
        }

        // PUT: api/tripulantes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTripulante(int id, TripulanteInput input)
        {
            var tripulante = await _context.Tripulantes.FindAsync(id);

            if (tripulante == null)
                return NotFound($"Tripulante com Id {id} não encontrado.");

            var funcoesValidas = new[] { "Piloto", "Copiloto", "Comissário" };

            if (!funcoesValidas.Contains(input.Funcao))
                return BadRequest($"Função inválida. As funções aceitas são: {string.Join(", ", funcoesValidas)}.");

            tripulante.Nome = input.Nome;
            tripulante.Funcao = input.Funcao;
            tripulante.NumeroLicenca = input.NumeroLicenca;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/tripulantes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTripulante(int id)
        {
            var tripulante = await _context.Tripulantes.FindAsync(id);

            if (tripulante == null)
                return NotFound($"Tripulante com Id {id} não encontrado.");

            _context.Tripulantes.Remove(tripulante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
