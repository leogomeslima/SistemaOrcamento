using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOrcamento.Api.Models;
using SistemaOrcamento.Api.DTOs;
using System.Linq;

namespace SistemaOrcamento.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrcamentoController : ControllerBase
    {
        private readonly SistemaOrcamentoContext _context;

        public OrcamentoController(SistemaOrcamentoContext context)
        {
            _context = context;
        }

        // --- MÉTODO POST (CRIAR) ---
        [HttpPost]
        public async Task<IActionResult> CreateOrcamento(OrcamentoCreateDto orcamentoDto)
        {
            // Validação 1: As entidades relacionadas existem?
            var centroCusto = await _context.CentrosCustos
                .FirstOrDefaultAsync(c => c.CentroCustoID == orcamentoDto.CentroCustoID);

            var planoConta = await _context.PlanoContas
                .FirstOrDefaultAsync(p => p.PlanoContaID == orcamentoDto.PlanoContaID);

            if (centroCusto == null) return BadRequest("CentroCustoID inválido.");
            if (planoConta == null) return BadRequest("PlanoContaID inválido.");

            // Validação 2: Já existe um orçamento para essa combinação?
            // (Esta é a nossa chave única do banco de dados)
            bool jaExiste = await _context.Orcamentos.AnyAsync(o =>
                o.CentroCustoID == orcamentoDto.CentroCustoID &&
                o.PlanoContaID == orcamentoDto.PlanoContaID &&
                o.Ano == orcamentoDto.Ano &&
                o.Mes == orcamentoDto.Mes
            );

            if (jaExiste)
            {
                return Conflict("Já existe um orçamento cadastrado para este Centro de Custo, Plano de Contas, Mês e Ano.");
            }

            // Mapeia DTO para Modelo
            var novoOrcamento = new Orcamento
            {
                CentroCustoID = orcamentoDto.CentroCustoID,
                PlanoContaID = orcamentoDto.PlanoContaID,
                Ano = orcamentoDto.Ano,
                Mes = orcamentoDto.Mes,
                ValorOrcado = orcamentoDto.ValorOrcado
            };

            _context.Orcamentos.Add(novoOrcamento);
            await _context.SaveChangesAsync();

            // Mapeia para o DTO de Leitura para o retorno
            var orcamentoParaRetorno = new OrcamentoReadDto
            {
                OrcamentoID = novoOrcamento.OrcamentoID,
                CentroCustoID = centroCusto.CentroCustoID,
                CentroCustoNome = centroCusto.Nome,
                PlanoContaID = planoConta.PlanoContaID,
                PlanoContaNome = planoConta.Nome,
                Ano = novoOrcamento.Ano,
                Mes = novoOrcamento.Mes,
                ValorOrcado = novoOrcamento.ValorOrcado
            };

            return CreatedAtAction(nameof(GetOrcamentoById), new { id = novoOrcamento.OrcamentoID }, orcamentoParaRetorno);
        }

        // --- MÉTODO GET (LISTAR TODOS COM FILTROS) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrcamentoReadDto>>> GetOrcamentos(
            [FromQuery] int? ano,
            [FromQuery] int? mes,
            [FromQuery] int? centroCustoId)
        {
            // Começa com a query base
            var query = _context.Orcamentos
                .Include(o => o.CentroCusto) // JOIN com CentrosCusto
                .Include(o => o.PlanoConta)  // JOIN com PlanoContas
                .AsQueryable();

            // Aplica filtros se eles foram fornecidos
            if (ano.HasValue)
            {
                query = query.Where(o => o.Ano == ano.Value);
            }
            if (mes.HasValue)
            {
                query = query.Where(o => o.Mes == mes.Value);
            }
            if (centroCustoId.HasValue)
            {
                query = query.Where(o => o.CentroCustoID == centroCustoId.Value);
            }

            // Projeta para o DTO de Leitura
            var orcamentos = await query
                .Select(o => new OrcamentoReadDto
                {
                    OrcamentoID = o.OrcamentoID,
                    CentroCustoID = o.CentroCustoID,
                    CentroCustoNome = o.CentroCusto.Nome,
                    PlanoContaID = o.PlanoContaID,
                    PlanoContaNome = o.PlanoConta.Nome,
                    Ano = o.Ano,
                    Mes = o.Mes,
                    ValorOrcado = o.ValorOrcado
                })
                .ToListAsync();

            return Ok(orcamentos);
        }

        // --- MÉTODO GET (POR ID) ---
        [HttpGet("{id}")]
        public async Task<ActionResult<OrcamentoReadDto>> GetOrcamentoById(int id)
        {
            var orcamento = await _context.Orcamentos
                .Include(o => o.CentroCusto)
                .Include(o => o.PlanoConta)
                .FirstOrDefaultAsync(o => o.OrcamentoID == id);

            if (orcamento == null)
            {
                return NotFound();
            }

            var orcamentoDto = new OrcamentoReadDto
            {
                OrcamentoID = orcamento.OrcamentoID,
                CentroCustoID = orcamento.CentroCustoID,
                CentroCustoNome = orcamento.CentroCusto.Nome,
                PlanoContaID = orcamento.PlanoContaID,
                PlanoContaNome = orcamento.PlanoConta.Nome,
                Ano = orcamento.Ano,
                Mes = orcamento.Mes,
                ValorOrcado = orcamento.ValorOrcado
            };

            return Ok(orcamentoDto);
        }
    }
}
