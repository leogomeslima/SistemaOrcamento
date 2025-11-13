using System.ComponentModel.DataAnnotations;

namespace SistemaOrcamento.Api.DTOs
{
    public class OrcamentoCreateDto
    {
        [Required]
        public int CentroCustoID { get; set; }

        [Required]
        public int PlanoContaID { get; set; }

        [Required]
        [Range(2020, 2100)]
        public int Ano { get; set; }

        [Required]
        [Range(1, 12)]
        public int Mes { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ValorOrcado { get; set; }
    }
}
