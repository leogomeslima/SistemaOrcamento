using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOrcamento.Api.DTOs;
using SistemaOrcamento.Api.Models;
using System.Linq;

namespace SistemaOrcamento.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanoContasController : ControllerBase
    {
        private readonly SistemaOrcamentoContext _context;

        public PlanoContasController(SistemaOrcamentoContext context)
        {
            _context = context;
        }

        // --- MÉTODO POST (CRIAR) ---
        [HttpPost]
        public async Task<IActionResult> CreatePlanoConta(PlanoContasCreateDto pcDto)
        {
            // Validação: Código ou Nome já existem?
            if (await _context.PlanoContas.AnyAsync(p => p.Nome == pcDto.Nome || p.CodigoConta == pcDto.CodigoConta))
            {
                return BadRequest("Nome ou Código da Conta já existem.");
            }

            // Mapeia DTO para Modelo
            var novoPlanoConta = new PlanoConta
            {
                Nome = pcDto.Nome,
                CodigoConta = pcDto.CodigoConta
            };

            _context.PlanoContas.Add(novoPlanoConta);
            await _context.SaveChangesAsync();

            // Mapeia Modelo para DTO de Leitura
            var pcParaRetorno = new PlanoContasReadDto
            {
                PlanoContaID = novoPlanoConta.PlanoContaID,
                Nome = novoPlanoConta.Nome,
                CodigoConta = novoPlanoConta.CodigoConta
            };

            return CreatedAtAction(nameof(GetPlanoContaById), new { id = novoPlanoConta.PlanoContaID }, pcParaRetorno);
        }

        // --- MÉTODO GET (LISTAR TODOS) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanoContasReadDto>>> GetPlanoContas()
        {
            // Esta entidade é simples, não precisa de Include()
            var planosDeContas = await _context.PlanoContas
                .Select(p => new PlanoContasReadDto
                {
                    PlanoContaID = p.PlanoContaID,
                    Nome = p.Nome,
                    CodigoConta = p.CodigoConta
                })
                .ToListAsync();

            return Ok(planosDeContas);
        }

        // --- MÉTODO GET (POR ID) ---
        [HttpGet("{id}")]
        public async Task<ActionResult<PlanoContasReadDto>> GetPlanoContaById(int id)
        {
            var pc = await _context.PlanoContas.FindAsync(id);

            if (pc == null)
            {
                return NotFound();
            }

            // Mapeia para o DTO
            var pcDto = new PlanoContasReadDto
            {
                PlanoContaID = pc.PlanoContaID,
                Nome = pc.Nome,
                CodigoConta = pc.CodigoConta
            };

            return Ok(pcDto);
        }
    }
}
