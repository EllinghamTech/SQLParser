using System;
using EllinghamTech.SqlParser.Internal;
using EllinghamTech.SqlParser.Tokens;

namespace Playground
{
    /// <summary>
    /// This is just a playground for testing the output of the parser.  Later it will
    /// become something more interactive.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string Sql = @"SELECT * FROM `myTable` AS a
    INNER JOIN `myOtherTable` ON `a`.`id` = `myOtherTable`.`my_table_id`
    LEFT JOIN `leftTable` ON `myOtherTable`.id = `leftTable`.`my_other_table_id`
WHERE `myValue` >= 5 AND a.myOtherValue IS NOT NULL AND `leftTable`.anotherOne IN (10, 15, 20) LIMIT 5;";

            Tokeniser tokeniser = new Tokeniser(Sql);
            tokeniser.Perform();

            foreach (BaseToken token in tokeniser.Tokens)
            {
                Console.WriteLine($"{token.GetType().Name} (Raw: {token.Raw})");
            }
        }
    }
}
