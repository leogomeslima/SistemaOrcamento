using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOrcamento.Api.Models;
using SistemaOrcamento.Api.DTOs;
using System.Linq;

namespace SistemaOrcamento.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequisicoesController : ControllerBase
    {
        private readonly SistemaOrcamentoContext _context;

        public RequisicoesController(SistemaOrcamentoContext context)
        {
            _context = context;
        }

        // --- MÉTODO POST (CRIAR REQUISIÇÃO E VERIFICAR ORÇAMENTO) ---
        [HttpPost]
        public async Task<IActionResult> CreateRequisicao(RequisicaoCreateDto reqDto)
        {
            // --- 1. VALIDAÇÃO DAS ENTIDADES ---

            // Valida Solicitante, Centro de Custo e Plano de Contas
            var solicitante = await _context.Usuarios.FindAsync(reqDto.SolicitanteID);
            var centroCusto = await _context.CentrosCustos.FindAsync(reqDto.CentroCustoID);
            var planoConta = await _context.PlanoContas.FindAsync(reqDto.PlanoContaID);

            if (solicitante == null) return BadRequest("SolicitanteID inválido.");
            if (centroCusto == null) return BadRequest("CentroCustoID inválido.");
            if (planoConta == null) return BadRequest("PlanoContaID inválido.");


            // --- 2. LÓGICA DE VERIFICAÇÃO DE ORÇAMENTO ---

            var dataHoje = DateTime.UtcNow; // Usamos UTC para datas no servidor
            var anoAtual = dataHoje.Year;
            var mesAtual = dataHoje.Month;

            // Busca o orçamento definido para esta combinação
            var orcamento = await _context.Orcamentos.FirstOrDefaultAsync(o =>
                o.CentroCustoID == reqDto.CentroCustoID &&
                o.PlanoContaID == reqDto.PlanoContaID &&
                o.Ano == anoAtual &&
                o.Mes == mesAtual
            );

            if (orcamento == null)
            {
                return BadRequest($"Não existe orçamento alocado para esta categoria ({planoConta.Nome}) neste mês.");
            }

            // Calcula o valor já "comprometido" (gastos Pendentes + Aprovados)
            var valorComprometido = await _context.Requisicoes
                .Where(r =>
                    r.CentroCustoID == reqDto.CentroCustoID &&
                    r.PlanoContaID == reqDto.PlanoContaID &&
                    r.DataSolicitacao.Year == anoAtual &&
                    r.DataSolicitacao.Month == mesAtual &&
                    (r.Status == "Pendente" || r.Status == "Aprovada") // Esta é a regra de negócio
                )
                .SumAsync(r => r.ValorSolicitado);

            // A VERIFICAÇÃO FINAL:
            if ((valorComprometido + reqDto.ValorSolicitado) > orcamento.ValorOrcado)
            {
                return BadRequest($"Orçamento excedido. Limite: {orcamento.ValorOrcado:C}. Comprometido: {valorComprometido:C}. Solicitado: {reqDto.ValorSolicitado:C}.");
            }


            // --- 3. CRIAÇÃO DA REQUISIÇÃO ---

            // Se passou em todas as verificações, cria a requisição
            var novaRequisicao = new Requisicao
            {
                SolicitanteID = reqDto.SolicitanteID,
                CentroCustoID = reqDto.CentroCustoID,
                PlanoContaID = reqDto.PlanoContaID,
                Descricao = reqDto.Descricao,
                ValorSolicitado = reqDto.ValorSolicitado,
                DataSolicitacao = dataHoje,
                Status = "Pendente" // O status inicial é sempre "Pendente"
            };

            _context.Requisicoes.Add(novaRequisicao);
            await _context.SaveChangesAsync();

            // --- 4. RETORNO DO DTO DE LEITURA ---
            // (Construímos manualmente o DTO de retorno)
            var reqParaRetorno = new RequisicaoReadDto
            {
                RequisicaoID = novaRequisicao.RequisicaoID,
                SolicitanteID = solicitante.UsuarioID,
                SolicitanteNome = solicitante.Nome,
                CentroCustoID = centroCusto.CentroCustoID,
                CentroCustoNome = centroCusto.Nome,
                PlanoContaID = planoConta.PlanoContaID,
                PlanoContaNome = planoConta.Nome,
                Descricao = novaRequisicao.Descricao,
                ValorSolicitado = novaRequisicao.ValorSolicitado,
                DataSolicitacao = novaRequisicao.DataSolicitacao,
                Status = novaRequisicao.Status
                // Aprovador é nulo, pois acabou de ser criada
            };

            return CreatedAtAction(nameof(GetRequisicaoById), new { id = novaRequisicao.RequisicaoID }, reqParaRetorno);
        }

        // --- MÉTODO GET (LISTAR TODOS COM FILTROS) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequisicaoReadDto>>> GetRequisicoes(
            [FromQuery] int? ano,
            [FromQuery] int? mes,
            [FromQuery] int? centroCustoId,
            [FromQuery] string status)
        {
            // Query base com todos os JOINs necessários
            var query = _context.Requisicoes
                .Include(r => r.Solicitante)
                .Include(r => r.CentroCusto)
                .Include(r => r.PlanoConta)
                .Include(r => r.Aprovador) // Pode ser nulo
                .AsQueryable();

            // Aplica filtros
            if (ano.HasValue)
                query = query.Where(r => r.DataSolicitacao.Year == ano.Value);
            if (mes.HasValue)
                query = query.Where(r => r.DataSolicitacao.Month == mes.Value);
            if (centroCustoId.HasValue)
                query = query.Where(r => r.CentroCustoID == centroCustoId.Value);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            // Projeta para o DTO de Leitura
            var requisicoes = await query
                .Select(r => new RequisicaoReadDto
                {
                    RequisicaoID = r.RequisicaoID,
                    SolicitanteID = r.Solicitante.UsuarioID,
                    SolicitanteNome = r.Solicitante.Nome,
                    CentroCustoID = r.CentroCusto.CentroCustoID,
                    CentroCustoNome = r.CentroCusto.Nome,
                    PlanoContaID = r.PlanoConta.PlanoContaID,
                    PlanoContaNome = r.PlanoConta.Nome,
                    Descricao = r.Descricao,
                    ValorSolicitado = r.ValorSolicitado,
                    DataSolicitacao = r.DataSolicitacao,
                    Status = r.Status,
                    AprovadorID = r.AprovadorID,
                    AprovadorNome = r.Aprovador.Nome, // EF Core lida com o nulo
                    DataAprovacao = r.DataAprovacao
                })
                .OrderByDescending(r => r.DataSolicitacao) // Mais recentes primeiro
                .ToListAsync();

            return Ok(requisicoes);
        }

        // --- MÉTODO GET (POR ID) ---
        [HttpGet("{id}")]
        public async Task<ActionResult<RequisicaoReadDto>> GetRequisicaoById(int id)
        {
            var r = await _context.Requisicoes
                .Include(r => r.Solicitante)
                .Include(r => r.CentroCusto)
                .Include(r => r.PlanoConta)
                .Include(r => r.Aprovador)
                .FirstOrDefaultAsync(r => r.RequisicaoID == id);

            if (r == null) return NotFound();

            // Mapeia para DTO
            var reqDto = new RequisicaoReadDto
            {
                RequisicaoID = r.RequisicaoID,
                SolicitanteID = r.Solicitante.UsuarioID,
                SolicitanteNome = r.Solicitante.Nome,
                CentroCustoID = r.CentroCusto.CentroCustoID,
                CentroCustoNome = r.CentroCusto.Nome,
                PlanoContaID = r.PlanoConta.PlanoContaID,
                PlanoContaNome = r.PlanoConta.Nome,
                Descricao = r.Descricao,
                ValorSolicitado = r.ValorSolicitado,
                DataSolicitacao = r.DataSolicitacao,
                Status = r.Status,
                AprovadorID = r.AprovadorID,
                AprovadorNome = r.Aprovador.Nome,
                DataAprovacao = r.DataAprovacao
            };

            return Ok(reqDto);
        }
       
        // --- MÉTODO POST (APROVAR REQUISIÇÃO) ---
        [HttpPost("{id}/aprovar")]
        public async Task<IActionResult> AprovarRequisicao(int id, RequisicaoAcaoDto acaoDto)
        {
            // 1. Busca a requisição E o seu Centro de Custo (para validar o gestor)
            var requisicao = await _context.Requisicoes
                .Include(r => r.CentroCusto)
                .FirstOrDefaultAsync(r => r.RequisicaoID == id);

            if (requisicao == null)
            {
                return NotFound("Requisição não encontrada.");
            }

            // 2. Valida o Status
            if (requisicao.Status != "Pendente")
            {
                return BadRequest($"Esta requisição já está com o status '{requisicao.Status}' e não pode ser aprovada.");
            }

            // 3. Valida o Aprovador (Gestor)
            var gestor = await _context.Usuarios.FindAsync(acaoDto.GestorID);
            if (gestor == null || gestor.Role != "Gestor")
            {
                return BadRequest("AprovadorID inválido ou o usuário não é um Gestor.");
            }

            // 4. LÓGICA DE NEGÓCIO CRUCIAL:
            // O gestor que está aprovando é o gestor DO CENTRO DE CUSTO desta requisição?
            if (requisicao.CentroCusto.GestorID != acaoDto.GestorID)
            {
                return StatusCode(403, "Ação não autorizada. Você não é o gestor deste Centro de Custo.");
            }

            // 5. Se tudo estiver OK, atualiza a requisição
            requisicao.Status = "Aprovada";
            requisicao.AprovadorID = acaoDto.GestorID;
            requisicao.DataAprovacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Requisição aprovada com sucesso.");
        }

        // --- MÉTODO POST (REJEITAR REQUISIÇÃO) ---
        [HttpPost("{id}/rejeitar")]
        public async Task<IActionResult> RejeitarRequisicao(int id, RequisicaoAcaoDto acaoDto)
        {
            // 1. Busca a requisição E o seu Centro de Custo
            var requisicao = await _context.Requisicoes
                .Include(r => r.CentroCusto)
                .FirstOrDefaultAsync(r => r.RequisicaoID == id);

            if (requisicao == null)
            {
                return NotFound("Requisição não encontrada.");
            }

            // 2. Valida o Status
            if (requisicao.Status != "Pendente")
            {
                return BadRequest($"Esta requisição já está com o status '{requisicao.Status}' e não pode ser rejeitada.");
            }

            // 3. Valida o Aprovador (Gestor)
            var gestor = await _context.Usuarios.FindAsync(acaoDto.GestorID);
            if (gestor == null || gestor.Role != "Gestor")
            {
                return BadRequest("AprovadorID inválido ou o usuário não é um Gestor.");
            }

            // 4. LÓGICA DE NEGÓCIO CRUCIAL:
            if (requisicao.CentroCusto.GestorID != acaoDto.GestorID)
            {
                return StatusCode(403, "Ação não autorizada. Você não é o gestor deste Centro de Custo.");
            }

            // 5. Se tudo estiver OK, atualiza a requisição
            requisicao.Status = "Rejeitada";
            requisicao.AprovadorID = acaoDto.GestorID;
            requisicao.DataAprovacao = DateTime.UtcNow; // DataAprovacao serve como DataAcao

            await _context.SaveChangesAsync();

            return Ok("Requisição rejeitada com sucesso.");
        }

        // ... (Restante da classe RequisicoesController)
    }
}