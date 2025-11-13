using System;
using System.Collections.Generic;

namespace SistemaOrcamento.Api.Models;

public partial class Orcamento
{
    public int OrcamentoID { get; set; }

    public int CentroCustoID { get; set; }

    public int PlanoContaID
    { get; set; }

    public int Ano { get; set; }

    public int Mes { get; set; }

    public decimal ValorOrcado { get; set; }

    public virtual CentrosCusto CentroCusto { get; set; } = null!;

    public virtual PlanoConta PlanoConta { get; set; } = null!;
}
