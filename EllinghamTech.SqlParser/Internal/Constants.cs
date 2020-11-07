using System;
using System.Collections.Generic;
using EllinghamTech.SqlParser.Tokens;
using EllinghamTech.SqlParser.Values;

namespace EllinghamTech.SqlParser.Internal
{
    public static class Constants
    {
        public static readonly char[] EmptyChars = {' ', '\n', '\t', '\v', '\f'};
        public static readonly char[] BreakingChars = {',', '.'};

        /// <summary>
        /// Key      => typeof BaseToken concrete implementation
        /// Token    => lowercase token value
        ///
        /// There are some special cases.
        /// - UnknownToken is created when a token does not match anything in this dictionary.
        /// - NumericValue is created when a token is (begins with) a number, can be decimal or other support notations
        ///
        /// A token can contain a whitespace.  Note that if the token contains a whitespace, that whitespace can be
        /// replaced with one or more character(s) in the EmptyChars array.
        /// </summary>
        public static Dictionary<Type, string[]> Tokens { get; } = new Dictionary<Type, string[]>
        {
            { typeof(SelectToken),             new[] { "select" } },
            { typeof(FromToken),               new[] { "from" } },
            { typeof(AsToken),                 new[] { "as" } },
            { typeof(WhereToken),              new[] { "where" } },
            { typeof(InnerJoinToken),          new[] { "join", "inner join" } },
            { typeof(LeftJoinToken),           new[] { "left join" } },
            { typeof(RightJoinToken),          new[] { "right join" } },
            { typeof(JoinOnToken),             new[] { "on" } },
            { typeof(GroupByToken),            new[] { "group by" } },
            { typeof(OrderByToken),            new[] { "order by" } },
            { typeof(LimitToken),              new[] { "limit" } },
            { typeof(OffsetToken),             new[] { "offset" } },
            { typeof(AndToken),                new[] { "and" } },
            { typeof(OrToken),                 new[] { "or" } },
            { typeof(LikeToken),               new[] { "like" } },
            { typeof(BetweenToken),            new[] { "between" } },
            { typeof(NotToken),                new[] { "not" } },
            { typeof(EqualsToken),             new[] { "=" } },
            { typeof(NotEqualsToken),          new[] { "<>" } },
            { typeof(GreaterThanToken),        new[] { ">" } },
            { typeof(GreaterThanEqualToToken), new[] { ">=" } },
            { typeof(LessThanToken),           new[] { "<" } },
            { typeof(LessThanEqualToToken),    new[] { "<=" } },
            { typeof(SumToken),                new[] { "sum" } },
            { typeof(CountToken),              new[] { "count" } },
            { typeof(IsNullToken),             new[] { "is null" } },
            { typeof(IsNotNullToken),          new[] { "is not null" } },
        };

        public static readonly Dictionary<char[], Type> TokenContainers = new Dictionary<char[], Type>
        {
            { new []{ '\'', '\'' }, typeof(StringValue) },
            { new []{ '"', '"' },   typeof(StringValue) },
            { new []{ '[', ']' },   typeof(EscapedValue) },
            { new []{ '{', '}' },   typeof(PlaceholderValue) },
            { new []{ '`', '`' },   typeof(EscapedValue) },
            { new []{ '(', ')' },   typeof(ExpressionToken) },
        };
    }
}
