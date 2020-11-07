using System;

namespace EllinghamTech.SqlParser.Exceptions
{
    /// <summary>
    /// The tokeniser is unable to handle this character, given the set of conditions currently set.
    /// </summary>
    public class InvalidCharacterException : Exception
    {
        public char InvalidCharacter { get; }
        public string Following { get; }

        public InvalidCharacterException(char curChar, string following, string message = null) : base(message)
        {
            InvalidCharacter = curChar;
            Following = following;
        }
    }
}
