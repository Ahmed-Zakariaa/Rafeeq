using System.Linq.Expressions;

namespace Rafeeq.Domain.Common;

public enum SortType { Asc = 0, Desc = 1 }

public class OrderModel<TSort> where TSort : struct, Enum
{
    public TSort? FieldName { get; set; }
    public SortType SortType { get; set; } = SortType.Asc;
}

/// <summary>Standard list request: filter + sort + pagination.</summary>
public class QueryViewModel<TFilter, TSort> where TSort : struct, Enum
{
    public TFilter? FilterModel { get; set; }
    public OrderModel<TSort>? OrderModel { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>Composable LINQ helpers used by every list query.</summary>
public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        => condition ? query.Where(predicate) : query;

    public static IQueryable<T> SortIf<T, TKey>(this IQueryable<T> query, bool condition,
        Expression<Func<T, TKey>> keySelector, SortType sortType)
    {
        if (!condition) return query;
        return sortType == SortType.Desc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
