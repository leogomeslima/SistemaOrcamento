using System;
using System.Collections.Generic;

namespace SistemaOrcamento.Api.Models;

public partial class Requisicao
{
    public int RequisicaoId { get; set; }

    public int SolicitanteId { get; set; }

    public int CentroCustoId { get; set; }

    public int PlanoContaId { get; set; }

    public string Descricao { get; set; } = null!;

    public decimal ValorSolicitado { get; set; }

    public DateTime DataSolicitacao { get; set; }

    public string Status { get; set; } = null!;

    public int? AprovadorId { get; set; }

    public DateTime? DataAprovacao { get; set; }

    public virtual Usuario? Aprovador { get; set; }

    public virtual CentrosCusto CentroCusto { get; set; } = null!;

    public virtual PlanoConta PlanoConta { get; set; } = null!;

    public virtual Usuario Solicitante { get; set; } = null!;
}
