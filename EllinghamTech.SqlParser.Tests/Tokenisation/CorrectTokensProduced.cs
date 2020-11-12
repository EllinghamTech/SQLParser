using System;
using System.Collections.Generic;
using EllinghamTech.SqlParser.Internal;
using EllinghamTech.SqlParser.Tokens;
using EllinghamTech.SqlParser.Values;
using NUnit.Framework;

namespace EllinghamTech.SqlParser.Tests.Tokenisation
{
    public class CorrectTokensProduced
    {
        /// <summary>
        /// Add queries and the appropriate token list it generates here.  The method
        /// CorrectTokensProducedFromSqlQuery runs for each element on the object[] and
        /// tests if the SQL Query String generates the equivalent token list
        /// </summary>
        private static List<TestCaseData> QueryTokens = new List<TestCaseData>
        {
            new TestCaseData("SELECT * FROM `test`;", new List<BaseToken>
            {
                new SelectToken { Raw = "SELECT" },
                new UnknownToken { Raw = "*" },
                new FromToken { Raw = "FROM" },
                new EscapedValue { Raw = "`test`" }
            }).SetName("Simple query"),

            new TestCaseData("SELECT * FROM `test` WHERE column = 1;", new List<BaseToken>
            {
                new SelectToken { Raw = "SELECT" },
                new UnknownToken { Raw = "*" },
                new FromToken { Raw = "FROM" },
                new EscapedValue { Raw = "`test`" },
                new WhereToken { Raw = "WHERE" },
                new UnknownToken { Raw = "column" },
                new EqualsToken { Raw = "=" },
                new NumericValue { Raw = "1" },
            }).SetName("Simple query with basic where clause"),

            new TestCaseData("SELECT * FROM `test` WHERE column=1;", new List<BaseToken>
            {
                new SelectToken { Raw = "SELECT" },
                new UnknownToken { Raw = "*" },
                new FromToken { Raw = "FROM" },
                new EscapedValue { Raw = "`test`" },
                new WhereToken { Raw = "WHERE" },
                new UnknownToken { Raw = "column" },
                new EqualsToken { Raw = "=" },
                new NumericValue { Raw = "1" },
            }).SetName("Query with where clause, no space between field comparer and value"),
        };

        [Test, TestCaseSource(nameof(QueryTokens))]
        public void CorrectTokensProducedFromSqlQuery(string sqlQuery, List<BaseToken> tokenList)
        {
            Tokeniser tokeniser = new Tokeniser(sqlQuery);
            tokeniser.Perform();

            // Sanity check, are they the same length
            Assert.IsTrue(tokenList.Count == tokeniser.Tokens.Count);

            // Loop through each, compare type and RAW value of each
            for (int x = 0; x < tokenList.Count; x++)
            {
                Type initialType = tokenList[x].GetType();
                Type secondaryType = tokeniser.Tokens[x].GetType();

                // Compare types are the same
                Assert.AreEqual(initialType, secondaryType);

                // Compare "RAW" property is the same
                Assert.AreEqual(tokenList[x].Raw, tokeniser.Tokens[x].Raw);
            }
        }
    }
}
