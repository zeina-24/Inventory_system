 using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;

namespace ImsPosSystem.Tests.TestHelpers;

/// <summary>
/// Wraps an in-memory <see cref="IQueryable{T}"/> with an EF-Core-compatible
/// async query provider, enabling Entity Framework async extension methods
/// (e.g. <c>ToListAsync</c>, <c>FirstOrDefaultAsync</c>) inside unit tests
/// without needing a real database or third-party library.
/// </summary>
public static class AsyncQueryHelper
{
    /// <summary>Returns an async-queryable wrapper around any in-memory sequence.</summary>
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
        => new AsyncQueryable<T>(source.AsQueryable());
}

// ── Internal wrappers ─────────────────────────────────────────────────────────

internal class AsyncQueryable<T> : IQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _inner;

    public AsyncQueryable(IQueryable<T> inner) => _inner = inner;

    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => new AsyncQueryProvider<T>(_inner.Provider);

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new AsyncEnumeratorWrapper<T>(_inner.GetEnumerator());
}

internal class AsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private static readonly System.Reflection.MethodInfo s_executeGeneric =
        typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == "Execute" && m.IsGenericMethodDefinition);

    private readonly IQueryProvider _inner;

    public AsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => _inner.CreateQuery(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new AsyncQueryable<TElement>(_inner.CreateQuery<TElement>(expression));

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        // TResult is Task<T> — unwrap the inner type T
        var innerType = typeof(TResult).GetGenericArguments()[0];

        // Call IQueryProvider.Execute<T>(expression) to get the synchronous result
        var result = s_executeGeneric
            .MakeGenericMethod(innerType)
            .Invoke(_inner, new object[] { expression });

        // Return Task.FromResult<T>(result) cast to TResult (= Task<T>)
        return (TResult)(object)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(innerType)
            .Invoke(null, new[] { result })!;
    }
}

internal class AsyncEnumeratorWrapper<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public AsyncEnumeratorWrapper(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
