using System;
using System.Collections.Generic;

namespace SistemaOrcamento.Api.Models;

public partial class CentrosCusto
{
    public int CentroCustoId { get; set; }

    public string Nome { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public int GestorId { get; set; }

    public int? CentroCustoPaiId { get; set; }

    public virtual CentrosCusto? CentroCustoPai { get; set; }

    public virtual Usuario Gestor { get; set; } = null!;

    public virtual ICollection<CentrosCusto> InverseCentroCustoPai { get; set; } = new List<CentrosCusto>();

    public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();

    public virtual ICollection<Requisicao> Requisicos { get; set; } = new List<Requisicao>();
}
