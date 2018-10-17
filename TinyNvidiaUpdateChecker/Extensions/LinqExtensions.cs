using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TinyNvidiaUpdateChecker.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        ///     Returns distinct elements from a sequence using the specified equality and hash functions.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" /></typeparam>
        /// <param name="source">The sequence to remove duplicate elements from</param>
        /// <param name="equals">A <see cref="Func{T1,T2,TResult}" /> to compare values</param>
        /// <param name="getHashCode">A <see cref="Func{T1,TResult}" /> calculate the hash code of values</param>
        /// <returns>An <see cref="IEnumerable{T}" /> that contains distinct elements from the source sequence</returns>
        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, bool> equals, Func<TSource, int> getHashCode)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (equals == null)
                throw new ArgumentNullException(nameof(equals));
            if (getHashCode == null)
                throw new ArgumentNullException(nameof(getHashCode));

            return source.Distinct(new AnonymousEqualityComparer<TSource>(equals, getHashCode));
        }

        /// <summary>
        ///     Returns distinct elements from a sequence using the specified equality and hash functions.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" /></typeparam>
        /// <param name="source">The sequence to remove duplicate elements from</param>
        /// <param name="equals">A <see cref="Func{T1,T2,TResult}" /> to compare values</param>
        /// <param name="getHashCode">A <see cref="Func{T1,TResult}" /> calculate the hash code of values</param>
        /// <returns>An <see cref="IEnumerable{T}" /> that contains distinct elements from the source sequence</returns>
        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable source, Func<TSource, TSource, bool> equals, Func<TSource, int> getHashCode)
        {
            return Distinct(source?.Cast<TSource>(), equals, getHashCode);
        }
    }
}