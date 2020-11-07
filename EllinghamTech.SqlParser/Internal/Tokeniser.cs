using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EllinghamTech.SqlParser.Exceptions;
using EllinghamTech.SqlParser.Tokens;
using EllinghamTech.SqlParser.Values;

namespace EllinghamTech.SqlParser.Internal
{
    /// <summary>
    /// The tokeniser takes an SQL Query and builds a list of Tokens, which can then be
    /// used to build the AST
    /// </summary>
    public class Tokeniser
    {
        public List<BaseToken> Tokens { get; } = new List<BaseToken>();
        public string SqlString { get; }

        private List<string> _stringParts = new List<string>();
        private StringBuilder _curPart = new StringBuilder();
        private char? _curContainer = null;
        private bool _isEscaped = false;
        private bool _isNumeric = false;
        private char[] _numerics = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private char _curChar = '\0';

        public Tokeniser(string sqlString)
        {
            sqlString = sqlString.Trim();
            sqlString = sqlString.TrimEnd(';');
            SqlString = sqlString;
        }

        /// <summary>
        /// Build the tokens
        /// </summary>
        public void Perform()
        {
            SplitString();
            BuildTokens();
        }

        /// <summary>
        /// Splits the SQL string into a set of parts, each part is the minimal representation allows
        /// for a token.  String is split by Constants.EmptyChars and Constants.BreakingChars, unless
        /// we are in a "container".
        /// </summary>
        /// <exception cref="InvalidCharacterException"></exception>
        /// <exception cref="InvalidEndException"></exception>
        private void SplitString()
        {
            // Split string into parts
            foreach (char t in SqlString)
            {
                _curChar = t;

                if (_curChar == '\\')
                    _isEscaped = !_isEscaped;

                if (IsContainerCharacter())
                {
                    _isNumeric = false;
                    HandleContainerCharacter();
                    continue;
                }

                // If we are in a container, write the value and move on
                if (_curContainer != null)
                {
                    _curPart.Append(_curChar);
                    continue;
                }

                // If this is a space or split and move on
                if (Constants.EmptyChars.Contains(_curChar))
                {
                    if (_curPart.Length > 0)
                    {
                        _stringParts.Add(_curPart.ToString());
                        _curPart.Clear();
                        _isNumeric = false;
                    }

                    continue;
                }

                // If it's a numeric
                if (_numerics.Contains(_curChar))
                {
                    // Not currently a numeric, split and start numeric reading
                    if (!_isNumeric)
                    {
                        if (_curPart.Length > 0)
                        {
                            _stringParts.Add(_curPart.ToString());
                            _curPart.Clear();
                        }

                        _isNumeric = true;
                    }

                    _curPart.Append(_curChar);
                    continue;
                }

                // We are reading a numeric, but this is not a numeric character
                if (_isNumeric)
                {
                    // Allow a '.', allowing for decimals
                    if (_curChar == '.')
                    {
                        // Can only accept first '.'
                        if (!_curPart.ToString().Contains('.'))
                        {
                            _curPart.Append(_curChar);
                            continue;
                        }

                        // Unknown Operation
                        throw new InvalidCharacterException('.', _curPart.ToString(), "Not expecting another . in a numeric");
                    }

                    // Split and move on
                    if (_curPart.Length > 0)
                    {
                        _stringParts.Add(_curPart.ToString());
                        _curPart.Clear();
                    }

                    _isNumeric = false;
                }

                // If breaking char, it's a token on it's own
                if (Constants.BreakingChars.Contains(_curChar))
                {
                    if (_curPart.Length > 0)
                    {
                        _stringParts.Add(_curPart.ToString());
                        _curPart.Clear();
                    }

                    _stringParts.Add(_curChar.ToString());
                    continue;
                }

                // Otherwise we only write the value to the current part
                _curPart.Append(_curChar);
            }

            // State check
            // Invalid if we are in a container
            if(_curContainer != null)
                throw new InvalidEndException();

            // If we are in an escape
            if (_isEscaped)
                throw new InvalidEndException();

            if (_curPart.Length != 0)
                _stringParts.Add(_curPart.ToString());
        }

        /// <summary>
        /// Taking the already split parts of the SQL Query, builds a list of tokens.
        /// </summary>
        private void BuildTokens()
        {
            List<string> parts = new List<string>(_stringParts);
            List<string> collectedParts = new List<string>();
            bool createdToken = false;

            while (parts.Any())
            {
                // If currently part is a container
                _curChar = parts[0][0];
                if (IsContainerCharacter())
                {
                    Type type = Constants.TokenContainers.Where(s => s.Key[0] == _curChar)
                        .Select(s => s.Value).First();
                    Tokens.Add(ConstructToken(type, parts[0]));
                    parts.RemoveAt(0);
                    continue;
                }

                // Is this a numeric?
                if (PartIsNumeric(parts[0]))
                {
                    NumericValue numericValue = new NumericValue {Raw = parts[0]};
                    Tokens.Add(numericValue);
                    parts.RemoveAt(0);
                    continue;
                }

                // Otherwise, collect parts until we reach a container, numeric or end.
                foreach (string part in parts)
                {
                    _curChar = part[0];
                    if (IsContainerCharacter())
                        break;

                    if (PartIsNumeric(part))
                        break;

                    collectedParts.Add(part);
                }

                // It is possible for a token to be formed from more than one part, so we need to take all
                // of the parts we can and work our way down.
                // For example, take LEFT JOIN test ON `test`.id = ...
                // [0] => LEFT JOIN test ON -> No token matches
                // [1] => LEFT JOIN test -> No token matches
                // [2] => LEFT JOIN -> Token Matches
                //
                // The reason this is important is that, in the example above, LEFT modifies the Join type.  In SQL
                // it is valid to use JOIN on it's own without LEFT/RIGHT/INNER/... so we can't just take JOIN and
                // create a token.
                for (int x = collectedParts.Count; x > 0; x--)
                {
                    Type type = GetToken(collectedParts);

                    if (type == null)
                    {
                        // No token matches, so remove the last part from the collected parts and
                        // try again, see example above.
                        collectedParts.RemoveAt(x - 1);
                    }
                    else
                    {
                        // Remove the used parts.  Example: INNER JOIN is two parts, so remove these
                        // two parts from the beginning of the parts enumerable.
                        for (int y = 0; y < x; y++)
                        {
                            parts.RemoveAt(0);
                        }

                        // Construct token and get out of the loop
                        Tokens.Add(ConstructToken(type, string.Join(" ", collectedParts)));
                        createdToken = true;
                        break;
                    }
                }

                collectedParts.Clear();

                // No idea what this is, so it's "unknown".  It is VITAL that we do something with the first
                // part in the working parts otherwise the parent loop can become infinite.
                if (!createdToken)
                {
                    Tokens.Add(new UnknownToken { Raw = parts[0] });
                    parts.RemoveAt(0);
                }

                createdToken = false;
            }
        }

        /// <summary>
        /// Taking a type (that is expected to be a BaseToken) and the raw string it is created from, construct
        /// the Token.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="raw"></param>
        /// <returns></returns>
        private BaseToken ConstructToken(Type type, string raw)
        {
            BaseToken token = (BaseToken)Activator.CreateInstance(type);
            token.Raw = raw;
            return token;
        }

        /// <summary>
        /// Try to get a token from the provided parts. If a single match is found, returns the type
        /// of the found token otherwise returns null.
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        private Type GetToken(IEnumerable<string> parts)
        {
            string token = string.Join(" ", parts);

            // If there is an exact match
            Type[] types = Constants.Tokens.Where(s => s.Value.Contains(token.ToLowerInvariant()))
                .Select(s => s.Key)
                .Distinct().ToArray();

            int count = types.Length;

            if (count == 1)
                return types.First();

            return null;
        }

        /// <summary>
        /// Test if the part is a numeric.  A part is numeric if it is not just a . and contains only numeric
        /// characters (include . as a decimal place representation).
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private bool PartIsNumeric(string part)
        {
            // Edge case: if the part is just a . then it is NOT numeric
            if (part.Length == 1 && part[0] == '.')
                return false;

            return part.All(c => _numerics.Contains(c) || c == '.');
        }

        /// <summary>
        /// Test if the _curChar is a container character
        /// </summary>
        /// <returns></returns>
        private bool IsContainerCharacter()
        {
            if (_isEscaped)
                return false;

            char[][] tokenContainers = Constants.TokenContainers.Select(s => s.Key).ToArray();

            // If in container, we are looking for the end separator
            if (_curContainer != null)
            {
                char[] container = tokenContainers.First(s => s[0] == _curContainer);
                return _curChar == container[1];
            }
            else
            {
                char[] container = tokenContainers.FirstOrDefault(s => s[0] == _curChar);
                return container != null;
            }
        }

        /// <summary>
        /// If the _curChar is a container character, this method can handle it appropriately whether it represents
        /// the start of beginning of the container.
        /// </summary>
        private void HandleContainerCharacter()
        {
            // We may be handling a container character, but not the correct one.
            // E.g. the ` in "it's"
            if (_curContainer != null && _curChar != _curContainer)
            {
                _curPart.Append(_curChar);
                return;
            }

            // Are we already in a container?
            if (_curContainer != null)
            {
                // We are in a container
                _curPart.Append(_curChar);
                _stringParts.Add(_curPart.ToString());
                _curPart.Clear();
                _curContainer = null;
            }
            else
            {
                // We are no longer in a container but we have been in a container,
                // otherwise we have just started the container
                if (_curPart.Length > 0)
                {
                    _stringParts.Add(_curPart.ToString());
                    _curPart.Clear();
                }

                _curPart.Append(_curChar);
                _curContainer = _curChar;
            }
        }
    }
}
