using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SistemaOrcamento.Api.Models;

public partial class SistemaOrcamentoContext : DbContext
{
    public SistemaOrcamentoContext(DbContextOptions<SistemaOrcamentoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CentrosCusto> CentrosCustos { get; set; }

    public virtual DbSet<Orcamento> Orcamentos { get; set; }

    public virtual DbSet<PlanoConta> PlanoContas { get; set; }

    public virtual DbSet<Requisicao> Requisicoes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CentrosCusto>(entity =>
        {
            entity.HasKey(e => e.CentroCustoId).HasName("PK__CentrosC__7A50D5B277255D3C");

            entity.ToTable("CentrosCusto");

            entity.HasIndex(e => e.Codigo, "UQ__CentrosC__06370DAC191A9081").IsUnique();

            entity.Property(e => e.CentroCustoId).HasColumnName("CentroCustoID");
            entity.Property(e => e.CentroCustoPaiId).HasColumnName("CentroCustoPaiID");
            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.GestorId).HasColumnName("GestorID");
            entity.Property(e => e.Nome).HasMaxLength(100);

            entity.HasOne(d => d.CentroCustoPai).WithMany(p => p.InverseCentroCustoPai)
                .HasForeignKey(d => d.CentroCustoPaiId)
                .HasConstraintName("FK_CentrosCusto_Pai");

            entity.HasOne(d => d.Gestor).WithMany(p => p.CentrosCustos)
                .HasForeignKey(d => d.GestorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CentrosCusto_Gestor");
        });

        modelBuilder.Entity<Orcamento>(entity =>
        {
            entity.HasKey(e => e.OrcamentoId).HasName("PK__Orcament__4E96F759AF4A05AE");

            entity.ToTable("Orcamento");

            entity.HasIndex(e => new { e.CentroCustoId, e.PlanoContaId, e.Ano, e.Mes }, "UQ_Orcamento_Item").IsUnique();

            entity.Property(e => e.OrcamentoId).HasColumnName("OrcamentoID");
            entity.Property(e => e.CentroCustoId).HasColumnName("CentroCustoID");
            entity.Property(e => e.PlanoContaId).HasColumnName("PlanoContaID");
            entity.Property(e => e.ValorOrcado).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CentroCusto).WithMany(p => p.Orcamentos)
                .HasForeignKey(d => d.CentroCustoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orcamento_CentroCusto");

            entity.HasOne(d => d.PlanoConta).WithMany(p => p.Orcamentos)
                .HasForeignKey(d => d.PlanoContaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orcamento_PlanoConta");
        });

        modelBuilder.Entity<PlanoConta>(entity =>
        {
            entity.HasKey(e => e.PlanoContaId).HasName("PK__PlanoCon__FB9B5B94C86EF99B");

            entity.HasIndex(e => e.CodigoConta, "UQ__PlanoCon__800717B66A76F390").IsUnique();

            entity.Property(e => e.PlanoContaId).HasColumnName("PlanoContaID");
            entity.Property(e => e.CodigoConta).HasMaxLength(20);
            entity.Property(e => e.Nome).HasMaxLength(100);
        });

        modelBuilder.Entity<Requisicao>(entity =>
        {
            entity.HasKey(e => e.RequisicaoId).HasName("PK__Requisic__8120ACF1388BBB6D");

            entity.Property(e => e.RequisicaoId).HasColumnName("RequisicaoID");
            entity.Property(e => e.AprovadorId).HasColumnName("AprovadorID");
            entity.Property(e => e.CentroCustoId).HasColumnName("CentroCustoID");
            entity.Property(e => e.DataAprovacao).HasColumnType("datetime");
            entity.Property(e => e.DataSolicitacao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.PlanoContaId).HasColumnName("PlanoContaID");
            entity.Property(e => e.SolicitanteId).HasColumnName("SolicitanteID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.ValorSolicitado).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Aprovador).WithMany(p => p.RequisicoAprovadors)
                .HasForeignKey(d => d.AprovadorId)
                .HasConstraintName("FK_Requisicoes_Aprovador");

            entity.HasOne(d => d.CentroCusto).WithMany(p => p.Requisicos)
                .HasForeignKey(d => d.CentroCustoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Requisicoes_CentroCusto");

            entity.HasOne(d => d.PlanoConta).WithMany(p => p.Requisicos)
                .HasForeignKey(d => d.PlanoContaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Requisicoes_PlanoConta");

            entity.HasOne(d => d.Solicitante).WithMany(p => p.RequisicoSolicitantes)
                .HasForeignKey(d => d.SolicitanteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Requisicoes_Solicitante");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioID).HasName("PK__Usuarios__2B3DE7982D33D89C");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D10534B77562CE").IsUnique();

            entity.Property(e => e.UsuarioID).HasColumnName("UsuarioID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
