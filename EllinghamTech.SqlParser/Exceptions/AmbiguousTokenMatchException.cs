using System;

namespace EllinghamTech.SqlParser.Exceptions
{
    public class AmbiguousTokenMatchException : Exception
    {
        public string TokenString { get; }

        public AmbiguousTokenMatchException(string tokenString)
        {
            TokenString = tokenString;
        }
    }
}
