using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmericanAirlinesApi.Data;
using AmericanAirlinesApi.Models;

namespace AmericanAirlinesApi.Controllers
{
    // DTO de entrada — apenas os campos que o usuário deve preencher
    public class AeronaveInput
    {
        public string Modelo { get; set; } = string.Empty;
        public string CodigoCauda { get; set; } = string.Empty;
        public int CapacidadePassageiros { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AeronavesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AeronavesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/aeronaves
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aeronave>>> GetAeronaves()
        {
            return await _context.Aeronaves.ToListAsync();
        }

        // GET: api/aeronaves/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Aeronave>> GetAeronave(int id)
        {
            var aeronave = await _context.Aeronaves.FindAsync(id);

            if (aeronave == null)
                return NotFound($"Aeronave com Id {id} não encontrada.");

            return aeronave;
        }

        // POST: api/aeronaves
        [HttpPost]
        public async Task<ActionResult<Aeronave>> PostAeronave(AeronaveInput input)
        {
            var aeronave = new Aeronave
            {
                Modelo = input.Modelo,
                CodigoCauda = input.CodigoCauda,
                CapacidadePassageiros = input.CapacidadePassageiros
            };

            _context.Aeronaves.Add(aeronave);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAeronave), new { id = aeronave.Id }, aeronave);
        }

        // PUT: api/aeronaves/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAeronave(int id, AeronaveInput input)
        {
            var aeronave = await _context.Aeronaves.FindAsync(id);

            if (aeronave == null)
                return NotFound($"Aeronave com Id {id} não encontrada.");

            aeronave.Modelo = input.Modelo;
            aeronave.CodigoCauda = input.CodigoCauda;
            aeronave.CapacidadePassageiros = input.CapacidadePassageiros;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/aeronaves/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAeronave(int id)
        {
            var aeronave = await _context.Aeronaves.FindAsync(id);

            if (aeronave == null)
                return NotFound($"Aeronave com Id {id} não encontrada.");

            _context.Aeronaves.Remove(aeronave);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
