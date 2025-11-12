using System.ComponentModel.DataAnnotations;

namespace SistemaOrcamento.Api.DTOs
{
    public class PlanoContasCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(20)]
        public string CodigoConta { get; set; }
    }
}
