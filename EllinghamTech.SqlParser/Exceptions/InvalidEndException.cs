using System;

namespace EllinghamTech.SqlParser.Exceptions
{
    public class InvalidEndException : Exception
    {
        public InvalidEndException(string message = "Unexpected end of input") : base(message)
        { }
    }
}
