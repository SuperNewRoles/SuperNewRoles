using System;
using System.Runtime.CompilerServices;

namespace SuperNewRoles.Extensions;

/// <summary>
/// IL2CPP 環境の <see cref="Il2CppSystem.Collections.Generic.List{T}"/> に対して
/// 代表的な LINQ 操作を提供する拡張メソッド群です。
///
/// System.Linq の拡張メソッドは <see cref="Il2CppSystem.Collections.Generic.List{T}"/> では
/// 直接利用できないため、必要最低限のメソッドを自前で実装しています。
/// </summary>
public static class Il2CppLinqExtensions
{
    /// <summary>
    /// 条件を満たす要素を列挙します。
    /// </summary>
    /// <param name="source">列挙対象のリスト</param>
    /// <param name="predicate">フィルター条件</param>
    /// <typeparam name="TSource">要素型</typeparam>
    /// <returns>条件を満たす要素の <see cref="IEnumerable{T}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static global::System.Collections.Generic.IEnumerable<TSource> Where<TSource>(this Il2CppSystem.Collections.Generic.List<TSource> source, Func<TSource, bool> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (predicate(item))
                yield return item;
        }
    }

    /// <summary>
    /// 各要素を射影します。
    /// </summary>
    /// <param name="source">列挙対象のリスト</param>
    /// <param name="selector">射影関数</param>
    /// <typeparam name="TSource">入力要素型</typeparam>
    /// <typeparam name="TResult">出力要素型</typeparam>
    /// <returns>射影後の要素の <see cref="IEnumerable{T}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static global::System.Collections.Generic.IEnumerable<TResult> Select<TSource, TResult>(this Il2CppSystem.Collections.Generic.List<TSource> source, Func<TSource, TResult> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        for (int i = 0; i < source.Count; i++)
        {
            yield return selector(source[i]);
        }
    }

    /// <summary>
    /// 最初の要素を返します。存在しない場合は型の既定値を返します。
    /// </summary>
    /// <param name="source">列挙対象のリスト</param>
    /// <typeparam name="TSource">要素型</typeparam>
    /// <returns>最初の要素。存在しない場合は default</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSource FirstOrDefault<TSource>(this Il2CppSystem.Collections.Generic.List<TSource> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Count > 0 ? source[0] : default;
    }

    /// <summary>
    /// 条件を満たす最初の要素を返します。存在しない場合は型の既定値を返します。
    /// </summary>
    /// <param name="source">列挙対象のリスト</param>
    /// <param name="predicate">条件</param>
    /// <typeparam name="TSource">要素型</typeparam>
    /// <returns>条件を満たす最初の要素。存在しない場合は default</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSource FirstOrDefault<TSource>(this Il2CppSystem.Collections.Generic.List<TSource> source, Func<TSource, bool> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (predicate(item))
                return item;
        }
        return default;
    }

}