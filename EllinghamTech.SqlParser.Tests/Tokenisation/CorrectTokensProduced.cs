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

            new TestCaseData(@"SELECT * FROM `myTable` AS a
            INNER JOIN `myOtherTable` ON `a`.`id` = `myOtherTable`.`my_table_id`
            LEFT JOIN `leftTable` ON `myOtherTable`.id = `leftTable`.`my_other_table_id`
            WHERE `myValue` >= 5 AND a.myOtherValue IS NOT NULL AND `leftTable`.anotherOne IN (10, 15, 20) LIMIT 5;", new List<BaseToken>
            {
                new SelectToken { Raw = "SELECT" },
                new UnknownToken { Raw = "*" },
                new FromToken { Raw = "FROM" },
                new EscapedValue { Raw = "`myTable`" },
                new AsToken { Raw = "AS" },
                new UnknownToken { Raw = "a" },
                new InnerJoinToken { Raw = "INNER JOIN" },
                new EscapedValue { Raw = "`myOtherTable`" },
                new JoinOnToken { Raw = "ON" },
                new EscapedValue { Raw = "`a`" },
                new UnknownToken { Raw = "." },
                new EscapedValue { Raw = "`id`" },
                new EqualsToken { Raw = "=" },
                new EscapedValue { Raw = "`myOtherTable`" },
                new UnknownToken { Raw = "." },
                new EscapedValue { Raw = "`my_table_id`" },
                new LeftJoinToken { Raw = "LEFT JOIN" },
                new EscapedValue { Raw = "`leftTable`" },
                new JoinOnToken { Raw = "ON" },
                new EscapedValue { Raw = "`myOtherTable`" },
                new UnknownToken { Raw = "." },
                new UnknownToken { Raw = "id" },
                new EqualsToken { Raw = "=" },
                new EscapedValue { Raw = "`leftTable`" },
                new UnknownToken { Raw = "." },
                new EscapedValue { Raw = "`my_other_table_id`" },
                new WhereToken { Raw = "WHERE" },
                new EscapedValue { Raw = "`myValue`" },
                new GreaterThanEqualToToken { Raw = ">=" },
                new NumericValue { Raw = "5" },
                new AndToken { Raw = "AND" },
                new UnknownToken { Raw = "a" },
                new UnknownToken { Raw = "." },
                new UnknownToken { Raw = "myOtherValue" },
                new IsNotNullToken { Raw = "IS NOT NULL" },
                new AndToken { Raw = "AND" },
                new EscapedValue { Raw = "`leftTable`" },
                new UnknownToken { Raw = "." },
                new UnknownToken { Raw = "anotherOne" },
                new UnknownToken { Raw = "IN" },
                new ExpressionToken { Raw = "(10, 15, 20)" },
                new LimitToken { Raw = "LIMIT" },
                new NumericValue { Raw = "5" },
            }).SetName("Query with complex joins and complex where clause"),
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
