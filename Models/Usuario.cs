using System;
using System.Collections.Generic;

namespace SistemaOrcamento.Api.Models;

public partial class Usuario
{
    public int UsuarioID { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SenhaHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<CentrosCusto> CentrosCustos { get; set; } = new List<CentrosCusto>();

    public virtual ICollection<Requisicao> RequisicoAprovadors { get; set; } = new List<Requisicao>();

    public virtual ICollection<Requisicao> RequisicoSolicitantes { get; set; } = new List<Requisicao>();
}
