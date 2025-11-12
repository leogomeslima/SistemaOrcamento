using System;
using System.Collections.Generic;

namespace SistemaOrcamento.Api.Models;

public partial class PlanoConta
{
    public int PlanoContaID { get; set; }

    public string Nome { get; set; } = null!;

    public string CodigoConta { get; set; } = null!;

    public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();

    public virtual ICollection<Requisicao> Requisicos { get; set; } = new List<Requisicao>();
}
