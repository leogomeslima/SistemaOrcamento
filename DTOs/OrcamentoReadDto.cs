namespace SistemaOrcamento.Api.DTOs
{
    public class OrcamentoReadDto
    {
        public int OrcamentoID { get; set; }

        // Dados do Centro de Custo
        public int CentroCustoID { get; set; }
        public string CentroCustoNome { get; set; }

        // Dados do Plano de Contas
        public int PlanoContaID { get; set; }
        public string PlanoContaNome { get; set; }

        // Dados do Orçamento
        public int Ano { get; set; }
        public int Mes { get; set; }
        public decimal ValorOrcado { get; set; }
    }
}
