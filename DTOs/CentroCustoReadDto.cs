namespace SistemaOrcamento.Api.DTOs
{
    public class CentroCustoReadDto
    {
        public int CentroCustoID { get; set; }
        public string Nome { get; set; }
        public string Codigo { get; set; }

        // Dados do Gestor
        public int GestorID { get; set; }
        public string GestorNome { get; set; }

        // Dados do Pai
        public int? CentroCustoPaiID { get; set; }
        public string? CentroCustoPaiNome { get; set; }
    }
}
