using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmericanAirlinesApi.Data;
using AmericanAirlinesApi.Models;

namespace AmericanAirlinesApi.Controllers
{
    public class ReservaInput
    {
        public int VooId { get; set; }
        public string NomePassageiro { get; set; } = string.Empty;
        public string Assento { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/reservas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
        {
            return await _context.Reservas
                .Include(r => r.Voo)
                    .ThenInclude(v => v!.Aeronave)
                .ToListAsync();
        }

        // GET: api/reservas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Voo)
                    .ThenInclude(v => v!.Aeronave)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
                return NotFound($"Reserva com Id {id} não encontrada.");

            return reserva;
        }

        // POST: api/reservas
        // Regra B: Sistema de Check-in
        [HttpPost]
        public async Task<ActionResult<Reserva>> PostReserva(ReservaInput input)
        {
            var voo = await _context.Voos
                .Include(v => v.Aeronave)
                .Include(v => v.Reservas)
                .FirstOrDefaultAsync(v => v.Id == input.VooId);

            if (voo == null)
                return NotFound($"Voo com Id {input.VooId} não encontrado.");

            if (voo.Aeronave == null)
                return NotFound("Aeronave vinculada ao voo não encontrada.");

            // Regra B.1 - Validação de Overbooking
            if (voo.Reservas.Count >= voo.Aeronave.CapacidadePassageiros)
                return BadRequest("Voo lotado. Não é possível realizar novas reservas.");

            var reserva = new Reserva
            {
                VooId = input.VooId,
                NomePassageiro = input.NomePassageiro,
                Assento = input.Assento,
                Taxa = 0.00m
            };

            // Regra B.2 - Lógica de Assento de Janela (termina com "A" ou "F")
            string ultimaLetra = input.Assento.Length > 0
                ? input.Assento[^1].ToString().ToUpper()
                : string.Empty;

            if (ultimaLetra == "A" || ultimaLetra == "F")
            {
                reserva.Taxa += 50.00m;
                Console.WriteLine("🪟 Assento na janela reservado com sucesso!");
            }

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.Id }, reserva);
        }

        // PUT: api/reservas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, ReservaInput input)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
                return NotFound($"Reserva com Id {id} não encontrada.");

            var vooExiste = await _context.Voos.AnyAsync(v => v.Id == input.VooId);

            if (!vooExiste)
                return NotFound($"Voo com Id {input.VooId} não encontrado.");

            reserva.VooId = input.VooId;
            reserva.NomePassageiro = input.NomePassageiro;
            reserva.Assento = input.Assento;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/reservas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
                return NotFound($"Reserva com Id {id} não encontrada.");

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
