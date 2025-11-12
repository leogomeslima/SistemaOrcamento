using System.ComponentModel.DataAnnotations;

namespace SistemaOrcamento.Api.DTOs
{
    public class CentroCustoCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(20)]
        public string Codigo { get; set; }

        [Required]
        public int GestorID { get; set; }
              
        public int? CentroCustoPaiID { get; set; }
    }
}
