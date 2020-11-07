using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EllinghamTech.SqlParser.Exceptions;
using EllinghamTech.SqlParser.Tokens;
using EllinghamTech.SqlParser.Values;

namespace EllinghamTech.SqlParser.Internal
{
    public class Tokeniser
    {
        public List<BaseToken> Tokens { get; protected set; } = new List<BaseToken>();
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

        public void Perform()
        {
            SplitString();
            BuildTokens();
        }

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
        }

        private void BuildTokens()
        {
            List<string> parts = new List<string>(_stringParts);
            List<string> collectedParts = new List<string>();

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

                // Otherwise, collect until we reach a container or end
                foreach (string part in parts)
                {
                    _curChar = part[0];
                    if (IsContainerCharacter())
                        break;

                    if (PartIsNumeric(part))
                        break;

                    collectedParts.Add(part);
                }

                // We need to get the largest node possible
                bool createdNode = false;

                for (int x = collectedParts.Count; x > 0; x--)
                {
                    Type type = GetNode(collectedParts);

                    if (type == null)
                    {
                        collectedParts.RemoveAt(x - 1);
                    }
                    else
                    {
                        for (int y = (x - 1); y >= 0; y--)
                        {
                            parts.RemoveAt(y);
                        }

                        Tokens.Add(ConstructToken(type, string.Join(" ", collectedParts)));
                        createdNode = true;
                        break;
                    }
                }

                collectedParts.Clear();

                // No idea what this is, so it's "unknown"
                if (!createdNode)
                {
                    BaseToken token = new UnknownToken();
                    token.Raw = parts[0];

                    Tokens.Add(token);
                    parts.RemoveAt(0);
                }
            }
        }

        private BaseToken ConstructToken(Type type, string raw)
        {
            BaseToken token = (BaseToken)Activator.CreateInstance(type);
            token.Raw = raw;
            return token;
        }

        private Type GetNode(IEnumerable<string> parts)
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

        private bool PartIsNumeric(string part)
        {
            // Edge case: if the part is just a . then it is NOT numeric
            if (part.Length == 1 && part[0] == '.')
                return false;

            return part.All(c => _numerics.Contains(c) || c == '.');
        }

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

        private void HandleContainerCharacter()
        {
            // We may be handling a container character, but not the correct one.
            // E.g. the ` in "it's"
            if (_curContainer != null && _curChar != _curContainer)
                return;

            // If _curPart had values, this indicates we are currently in a value container
            // so we are at the end of the container.
            if (_curContainer != null)
            {
                _curPart.Append(_curChar);
                _stringParts.Add(_curPart.ToString());
                _curPart.Clear();
                _curContainer = null;
            }
            else
            {
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
