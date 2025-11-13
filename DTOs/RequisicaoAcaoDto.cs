// DTOs/RequisicaoAcaoDto.cs

using System.ComponentModel.DataAnnotations;

namespace SistemaOrcamento.Api.DTOs
{
    public class RequisicaoAcaoDto
    {
        [Required]
        public int GestorID { get; set; }
    }
}