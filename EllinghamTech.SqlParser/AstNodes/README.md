# Nodes
Nodes represent every part of an SQL Query.

This parser works differently to the parsers found in MySQL, for example, so syntax errors are likely
to be represented differently.

## Examples:

Take the following valid SQL query:

```
SELECT * FROM `myTable` AS a
    JOIN `myOtherTable` ON `a`.`id` = `myOtherTable`.`my_table_id`
WHERE `myValue` >= 5 AND a.myOtherValue IS NOT NULL LIMIT 5;
```

### First Parse (tokenisation)
```
SelectToken               // SELECT
UnknownToken              // *
FromToken                 // FROM
EscapedValue              // myTable
AsToken                   // AS
UnknownToken              // a
InnerJoinToken            // JOIN
EscapedValue              // myOtherTable
JoinOnToken               // ON
EscapedValue              // a
UnknownToken              // .
EscapedValue              // id
EqualsToken               // =
EscapedValue              // myOtherTable
UnknownToken              // .
EscapedValue              // my_table_id
WhereToken                // WHERE
EscapedValue              // myValue
GreaterThanEqualToToken   // >=
NumericValue              // 5
AndToken                  // AND
UnknownToken              // a
UnknownToken              // .
UnknownToken              // myOtherValue
IsNotNull                 // IS NOT NULL
Limit                     // LIMIT
NumericValue              // 5
```

At this point, we really have no idea what's going on with the query.  Very few checks are performed.  However,
there are some basic checks:
- The first object must be a root node
- Certain ordering is validated, for example a Join cannot be after a EscapedValue, OrderBy, GroupBy...

### Second parse (tree generation)

Once the first parse is complete, a second step is required that produces the following result:

```
SelectExpression (columns: null, table: "myTable")
    InnerJoin (table: "myOtherTable", database: null)
        EqualsConditional
            Field (field: "id", table: "a")
            Field (field: "my_table_id", table: "myOtherTable")
    WhereClause
        AndConditional
            GreaterThanEqualTo
                Field (field: "myValue", table: null)
                NumericValue (value: 5)
            IsNotNull
                Field (field: "myOtherValue", table: "a")
    Limit (limit: 5, offset: 0)            
```

At this point, we are almost complete but we need to deal with aliases.  The table "a" doesn't exist, it is an alias.
  During this stage, the As nodes were removed but a log was kept.  We also need to fill in the details about what
  table is being referred to, providing it is possible to do so.
  
The syntax after creating the AST must be valid.  Each node validates it's child nodes to ensure that the AST is valid.
  
### Finalisation
  
```
SelectExpression (columns: null, table: "myTable", database: null)
    InnerJoin (table: "myOtherTable", database: null)
        EqualsConditional
            Field (field: "id", table: "myTable", database: null)
            Field (field: "my_table_id", table: "myOtherTable", database: null)
    WhereClause
        AndConditional
            GreaterThanEqualTo
                Field (field: "myValue", table: null, database: null)
                NumericValue (value: 5)
            IsNotNull
                Field (field: "myOtherValue", table: "myTable", database: null)
    Limit (limit: 5, offset: 0)     
```

This is our abstract syntax tree.

The final result is not an exact representation.  Some optimisations, such as removal of aliases, is performed.  This
allows better comparisons between queries and more direct analysis of the query, without too much "fluff".

This also cannot validate the query for reference errors, such as whether a table/field exists or field ambiguity.

## Standard Optimisations
- Aliases: aliases are removed from the AST
- Expressions: a generic expression, unless required, will be removed if it is not required

## AST-to-query
It is possible to take the AST result and convert it back to a query.  **The output may not be identical but the functional
result of the query must be.**  This allows for validation of the query to take place and executing a `safe` query against
the database.

## Multiple queries
The parser will only allow sub-queries.  Multiple queries must be split beforehand using a delimiter (default `;`).

## Sub-queries
Sub-queries must be within parenthesis.  Sub-queries are currently not validated very well and can be disabled in the
parser configuration, generating an `IllegalOperationException` if they are disabled.

## Unions
There is no support for unions at this time, however this may be implemented in the future.

## Placeholders

Placeholders, (`{myNamedPlaceholder}`, `$1`), are useful for:
- Specifying the location of an unknown value
- Sanitising inputs before execution
- Allowing easier contextual information concerning what the values "mean" or represent

Placeholders are not validated and are treated as a "value" of any type.  In AST-to-query they are outputted as-is without
any modification.  Values enclosed in curly braces can be anything, except another curly brace.  Values after the "dollar"
symbol must be an integer.

### Expandable placeholder
Sometimes it is necessary for a placeholder to represent `one or more` values.  In this case, the placeholder should be
appended with an ellipsis.  Example: `IN($1...)`, `IN({myNamedPlaceholder}...)`.

## Unsafe queries
At this time, the parser is not capable of generating an AST for anything but `SELECT` SQL statements.  In the future,
we plan to implement other query-types including `UPDATE`, `INSERT`, `DROP`.  In order to prevent future security risks,
you are required to specify allowed query types as part of the parser configuration.  
