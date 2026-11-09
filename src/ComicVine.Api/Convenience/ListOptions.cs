using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ComicVine.Api
{
    /// <summary>Query parameters shared by the generated list endpoints, implemented by partial classes in <c>QueryPartials.cs</c>.</summary>
    internal interface IListQuery
    {
        int? Limit { get; set; }
        int? Offset { get; set; }
        string? Sort { get; set; }
        string? Filter { get; set; }
        string[]? FieldList { get; set; }
    }

    /// <summary>
    /// Filter, sort, field selection and page size for a list call. Build one with the fluent methods:
    /// <c>new ListOptions().Where("name", "Batman").SortBy("count_of_issues", descending: true).Select("id", "name")</c>.
    /// </summary>
    public sealed class ListOptions
    {
        public const int MaxPageSize = 100;

        private readonly List<KeyValuePair<string, string>> _filters = new List<KeyValuePair<string, string>>();
        private string[]? _fields;
        private string? _sort;
        private int _pageSize = MaxPageSize;

        /// <summary>Entries per request, 1 to 100. Defaults to 100.</summary>
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value < 1 || value > MaxPageSize)
                    throw new ArgumentOutOfRangeException(nameof(value), "PageSize must be between 1 and 100.");
                _pageSize = value;
            }
        }

        /// <summary>Adds <c>field:value</c>. Names and values are case-insensitive substring matches on most fields.</summary>
        public ListOptions Where(string field, string value)
        {
            if (string.IsNullOrWhiteSpace(field)) throw new ArgumentException("Field is required.", nameof(field));
            _filters.Add(new KeyValuePair<string, string>(field, value ?? string.Empty));
            return this;
        }

        /// <summary>Adds <c>field:id</c>, for filtering by a related resource id such as <c>volume</c>.</summary>
        public ListOptions Where(string field, int value) =>
            Where(field, value.ToString(CultureInfo.InvariantCulture));

        /// <summary>Adds the date range <c>field:from|to</c>.</summary>
        public ListOptions WhereBetween(string field, DateTime from, DateTime to) =>
            Where(field, from.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "|" +
                         to.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

        public ListOptions SortBy(string field, bool descending = false)
        {
            if (string.IsNullOrWhiteSpace(field)) throw new ArgumentException("Field is required.", nameof(field));
            _sort = field + ":" + (descending ? "desc" : "asc");
            return this;
        }

        /// <summary>Limits the response to these fields, which also shrinks the payload.</summary>
        public ListOptions Select(params string[] fields)
        {
            _fields = fields != null && fields.Length > 0 ? fields : null;
            return this;
        }

        internal void Apply(IListQuery query, int offset)
        {
            query.Limit = _pageSize;
            query.Offset = offset;
            query.Sort = _sort;
            query.Filter = _filters.Count == 0
                ? null
                : string.Join(",", _filters.Select(f => f.Key + ":" + f.Value));
            query.FieldList = _fields;
        }
    }
}
