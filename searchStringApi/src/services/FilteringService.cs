using System;
using System.Collections.Generic;
using System.Linq;
using MyApp.Models;
using MyApp.DTOs;

public static class ArticleFilterService
{
    /// Filtra artigos que possuem URL e são de acesso aberto
    public static List<ArticlePaperMiner> FiltrarAcessoAbertoComUrl(IEnumerable<ArticlePaperMiner> artigos)
    {
        return artigos
            .Where(a =>
                !string.IsNullOrWhiteSpace(a.Url) &&
                a.IsOpenAccess == true)
            .ToList();
    }

    /// Ordena artigos pela métrica de relevância (relevanceMetric) decrescente
    public static List<ArticlePaperMiner> OrdenarPorRelevancia(IEnumerable<ArticlePaperMiner> artigos)
    {
        return artigos
            .OrderByDescending(a => a.RelevanceMetric ?? 0)
            .ToList();
    }

    /// Método utilitário: filtro + ordenação
    public static List<ArticlePaperMiner> FiltrarEAgrupar(IEnumerable<ArticlePaperMiner> artigos)
    {
        return OrdenarPorRelevancia(FiltrarAcessoAbertoComUrl(artigos));
    }
}
