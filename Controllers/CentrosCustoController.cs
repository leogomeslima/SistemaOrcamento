using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOrcamento.Api.DTOs;
using SistemaOrcamento.Api.Models;

namespace SistemaOrcamento.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CentrosCustoController : ControllerBase
    {
        private readonly SistemaOrcamentoContext _context;

        // Injeta o DbContext
        public CentrosCustoController(SistemaOrcamentoContext context)
        {
            _context = context;
        }

        // --- MÉTODO POST (CRIAR) ---
        [HttpPost]
        public async Task<IActionResult> CreateCentroCusto(CentroCustoCreateDto ccDto)
        {
            // Validação 1: O Gestor existe e é um "Gestor"?
            var gestor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioID == ccDto.GestorID && u.Role == "Gestor");

            if (gestor == null)
            {
                return BadRequest("O GestorID fornecido não existe ou o usuário não tem o Role 'Gestor'.");
            }

            // Validação 2: Se um Pai foi informado, ele existe?
            if (ccDto.CentroCustoPaiID.HasValue)
            {
                var paiExiste = await _context.CentrosCustos
                    .AnyAsync(c => c.CentroCustoID == ccDto.CentroCustoPaiID.Value);
                if (!paiExiste)
                {
                    return BadRequest("O CentroCustoPaiID fornecido não existe.");
                }
            }

            // Validação 3: Código ou Nome já existem?
            if (await _context.CentrosCustos.AnyAsync(c => c.Nome == ccDto.Nome || c.Codigo == ccDto.Codigo))
            {
                return BadRequest("Nome ou Código do Centro de Custo já existem.");
            }

            // Mapeia o DTO para o Modelo
            var novoCC = new CentrosCusto
            {
                Nome = ccDto.Nome,
                Codigo = ccDto.Codigo,
                GestorID = ccDto.GestorID,
                CentroCustoPaiID = ccDto.CentroCustoPaiID
            };

            _context.CentrosCustos.Add(novoCC);
            await _context.SaveChangesAsync();

            // --- Mapeia para o DTO de Leitura para o retorno ---
            // (Note: O 'novoCC' agora tem o 'CentroCustoID' gerado)
            var ccParaRetorno = new CentroCustoReadDto
            {
                CentroCustoID = novoCC.CentroCustoID,
                Nome = novoCC.Nome,
                Codigo = novoCC.Codigo,
                GestorID = gestor.UsuarioID,
                GestorNome = gestor.Nome, // Já temos o objeto 'gestor'
                CentroCustoPaiID = novoCC.CentroCustoPaiID
                // Poderíamos buscar o nome do Pai, mas vamos deixar simples por enquanto
            };

            return CreatedAtAction(nameof(GetCentroCustoById), new { id = novoCC.CentroCustoID }, ccParaRetorno);
        }

        // --- MÉTODO GET (LISTAR TODOS) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CentroCustoReadDto>>> GetCentrosCusto()
        {
            // Aqui usamos Include() para carregar os dados relacionados (JOINs)
            var centrosCusto = await _context.CentrosCustos
                .Include(c => c.Gestor) // JOIN com Usuarios (Gestor)
                .Include(c => c.CentroCustoPai) // JOIN com si mesmo (Pai)
                .Select(c => new CentroCustoReadDto
                {
                    // Mapeia os dados para o DTO de Leitura
                    CentroCustoID = c.CentroCustoID,
                    Nome = c.Nome,
                    Codigo = c.Codigo,
                    GestorID = c.GestorID,
                    GestorNome = c.Gestor.Nome,
                    CentroCustoPaiID = c.CentroCustoPaiID,
                    CentroCustoPaiNome = c.CentroCustoPai.Nome // Pode ser nulo
                })
                .ToListAsync();

            return Ok(centrosCusto);
        }

        // --- MÉTODO GET (POR ID) ---
        [HttpGet("{id}")]
        public async Task<ActionResult<CentroCustoReadDto>> GetCentroCustoById(int id)
        {
            var cc = await _context.CentrosCustos
                .Include(c => c.Gestor)
                .Include(c => c.CentroCustoPai)
                .FirstOrDefaultAsync(c => c.CentroCustoID == id);

            if (cc == null)
            {
                return NotFound();
            }

            // Mapeia para o DTO
            var ccDto = new CentroCustoReadDto
            {
                CentroCustoID = cc.CentroCustoID,
                Nome = cc.Nome,
                Codigo = cc.Codigo,
                GestorID = cc.GestorID,
                GestorNome = cc.Gestor.Nome,
                CentroCustoPaiID = cc.CentroCustoPaiID,
                CentroCustoPaiNome = cc.CentroCustoPai.Nome
            };

            return Ok(ccDto);
        }
    }
}
