using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmericanAirlinesApi.Data;
using AmericanAirlinesApi.Models;

namespace AmericanAirlinesApi.Controllers
{
    // DTO de entrada
    public class VooInput
    {
        public string CodigoVoo { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public int AeronavId { get; set; }
        public string Status { get; set; } = "Agendado";
    }

    // DTOs de saída — resposta limpa sem referências circulares
    public class AeronaveResumo
    {
        public int Id { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public string CodigoCauda { get; set; } = string.Empty;
        public int CapacidadePassageiros { get; set; }
    }

    public class VooResponse
    {
        public int Id { get; set; }
        public string CodigoVoo { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public AeronaveResumo? Aeronave { get; set; }
        public int TotalReservas { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class VoosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VoosController(AppDbContext context)
        {
            _context = context;
        }

        private static VooResponse ToResponse(Voo v) => new VooResponse
        {
            Id = v.Id,
            CodigoVoo = v.CodigoVoo,
            Origem = v.Origem,
            Destino = v.Destino,
            Status = v.Status,
            TotalReservas = v.Reservas?.Count ?? 0,
            Aeronave = v.Aeronave == null ? null : new AeronaveResumo
            {
                Id = v.Aeronave.Id,
                Modelo = v.Aeronave.Modelo,
                CodigoCauda = v.Aeronave.CodigoCauda,
                CapacidadePassageiros = v.Aeronave.CapacidadePassageiros
            }
        };

        // GET: api/voos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VooResponse>>> GetVoos()
        {
            var voos = await _context.Voos
                .Include(v => v.Aeronave)
                .Include(v => v.Reservas)
                .ToListAsync();

            return Ok(voos.Select(ToResponse));
        }

        // GET: api/voos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<VooResponse>> GetVoo(int id)
        {
            var voo = await _context.Voos
                .Include(v => v.Aeronave)
                .Include(v => v.Reservas)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (voo == null)
                return NotFound($"Voo com Id {id} não encontrado.");

            return Ok(ToResponse(voo));
        }

        // POST: api/voos
        // Regra A: Agendamento de Voo Inteligente
        [HttpPost]
        public async Task<ActionResult<VooResponse>> PostVoo(VooInput input)
        {
            var aeronave = await _context.Aeronaves.FindAsync(input.AeronavId);

            if (aeronave == null)
                return NotFound($"Aeronave com Id {input.AeronavId} não encontrada.");

            var aeronaveEmTransito = await _context.Voos
                .AnyAsync(v => v.AeronavId == input.AeronavId && v.Status == "Em Voo");

            if (aeronaveEmTransito)
                return Conflict("Aeronave indisponível, encontra-se em trânsito.");

            var statusValidos = new[] { "Agendado", "Em Voo", "Finalizado", "Cancelado" };

            if (!statusValidos.Contains(input.Status))
                return BadRequest($"Status inválido. Os status aceitos são: {string.Join(", ", statusValidos)}.");

            var voo = new Voo
            {
                CodigoVoo = input.CodigoVoo,
                Origem = input.Origem,
                Destino = input.Destino,
                AeronavId = input.AeronavId,
                Status = input.Status
            };

            _context.Voos.Add(voo);
            await _context.SaveChangesAsync();

            // Recarrega com navegação para montar o response
            await _context.Entry(voo).Reference(v => v.Aeronave).LoadAsync();

            return CreatedAtAction(nameof(GetVoo), new { id = voo.Id }, ToResponse(voo));
        }

        // PUT: api/voos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoo(int id, VooInput input)
        {
            var voo = await _context.Voos.FindAsync(id);

            if (voo == null)
                return NotFound($"Voo com Id {id} não encontrado.");

            var aeronave = await _context.Aeronaves.FindAsync(input.AeronavId);

            if (aeronave == null)
                return NotFound($"Aeronave com Id {input.AeronavId} não encontrada.");

            voo.CodigoVoo = input.CodigoVoo;
            voo.Origem = input.Origem;
            voo.Destino = input.Destino;
            voo.AeronavId = input.AeronavId;
            voo.Status = input.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/voos/{id}/status
        // Regra C: Atualização de Status de Voo
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> PatchStatusVoo(int id, [FromBody] string novoStatus)
        {
            var voo = await _context.Voos.FindAsync(id);

            if (voo == null)
                return NotFound($"Voo com Id {id} não encontrado.");

            var statusValidos = new[] { "Agendado", "Em Voo", "Finalizado", "Cancelado" };

            if (!statusValidos.Contains(novoStatus))
                return BadRequest($"Status inválido. Os status aceitos são: {string.Join(", ", statusValidos)}.");

            // Regra de Ouro: Voo Finalizado ou Cancelado não pode voltar para Em Voo
            if ((voo.Status == "Finalizado" || voo.Status == "Cancelado") && novoStatus == "Em Voo")
                return UnprocessableEntity(
                    $"Regra de negócio violada: Um voo com status '{voo.Status}' não pode ser revertido para 'Em Voo'."
                );

            voo.Status = novoStatus;
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Status do voo '{voo.CodigoVoo}' atualizado para '{novoStatus}'." });
        }

        // DELETE: api/voos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoo(int id)
        {
            var voo = await _context.Voos.FindAsync(id);

            if (voo == null)
                return NotFound($"Voo com Id {id} não encontrado.");

            _context.Voos.Remove(voo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
