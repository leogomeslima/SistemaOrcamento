using System.ComponentModel.DataAnnotations;

namespace SistemaOrcamento.Api.DTOs
{
    public class UsuarioCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Senha { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
