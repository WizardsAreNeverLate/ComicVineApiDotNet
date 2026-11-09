using System;
using System.Collections.Generic;

namespace ComicVine.Api
{
    /// <summary>One page of results from a Comic Vine list endpoint.</summary>
    public sealed class Page<T>
    {
        public Page(IReadOnlyList<T> items, int totalCount, int offset)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            TotalCount = totalCount;
            Offset = offset;
        }

        /// <summary>The entries on this page.</summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>Number of entries across all pages that match the filter.</summary>
        public int TotalCount { get; }

        /// <summary>Zero-based index of the first entry on this page.</summary>
        public int Offset { get; }

        /// <summary>True when more matching entries exist after this page.</summary>
        public bool HasNext => Items.Count > 0 && Offset + Items.Count < TotalCount;
    }
}
