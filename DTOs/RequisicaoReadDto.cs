
namespace SistemaOrcamento.Api.DTOs
{
    public class RequisicaoReadDto
    {
        public int RequisicaoID { get; set; }

        // Solicitante
        public int SolicitanteID { get; set; }
        public string SolicitanteNome { get; set; }

        // Centro de Custo
        public int CentroCustoID { get; set; }
        public string CentroCustoNome { get; set; }

        // Plano de Contas
        public int PlanoContaID { get; set; }
        public string PlanoContaNome { get; set; }

        // Dados da Requisição
        public string Descricao { get; set; }
        public decimal ValorSolicitado { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public string Status { get; set; } // 'Pendente', 'Aprovada', 'Rejeitada'

        // Aprovador (pode ser nulo)
        public int? AprovadorID { get; set; }
        public string? AprovadorNome { get; set; }
        public DateTime? DataAprovacao { get; set; }
    }
}