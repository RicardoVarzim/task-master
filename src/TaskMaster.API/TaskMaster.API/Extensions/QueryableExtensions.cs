using Microsoft.EntityFrameworkCore;
using TaskMaster.API.Models;

namespace TaskMaster.API.Extensions;

public static class QueryableExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> query, PagedQuery pagedQuery)
    {
        var totalCount = query.Count();
        var items = query
            .Skip(pagedQuery.Skip)
            .Take(pagedQuery.Take)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pagedQuery.PageNumber,
            PageSize = pagedQuery.PageSize
        };
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PagedQuery pagedQuery)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(pagedQuery.Skip)
            .Take(pagedQuery.Take)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pagedQuery.PageNumber,
            PageSize = pagedQuery.PageSize
        };
    }
}

