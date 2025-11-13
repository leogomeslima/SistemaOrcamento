using System.ComponentModel.DataAnnotations;

namespace SistemaOrcamento.Api.DTOs
{
    public class RequisicaoCreateDto
    {
        [Required]
        public int SolicitanteID { get; set; } // Em um sistema real, isso viria do Token de Auth

        [Required]
        public int CentroCustoID { get; set; }

        [Required]
        public int PlanoContaID { get; set; }

        [Required]
        [StringLength(500)]
        public string Descricao { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal ValorSolicitado { get; set; }
    }
}