using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization; // For NumberStyles and CultureInfo

namespace SuperNewRoles.Modules
{
    public class JsonParser
    {
        private readonly string _json;
        private int _pos;

        private JsonParser(string json)
        {
            _json = json;
            _pos = 0;
        }

        public static object Parse(string json)
        {
            var parser = new JsonParser(json);
            var result = parser.ParseValue();
            parser.SkipWhitespace();
            if (parser._pos != parser._json.Length)
                throw new JsonParseException($"Unexpected character '{parser._json[parser._pos]}' after JSON value at position {parser._pos}", parser._pos);
            return result;
        }

        private void SkipWhitespace()
        {
            while (_pos < _json.Length && char.IsWhiteSpace(_json[_pos]))
                _pos++;
        }

        private object ParseValue()
        {
            SkipWhitespace();
            if (_pos >= _json.Length)
                throw new JsonParseException("Unexpected end of JSON input", _pos);

            char c = _json[_pos];
            switch (c)
            {
                case '{': return ParseObject();
                case '[': return ParseArray();
                case '"': return ParseString();
                case 't': return ParseTrue();
                case 'f': return ParseFalse();
                case 'n': return ParseNull();
                default:
                    if (c == '-' || char.IsDigit(c))
                        return ParseNumber();
                    throw new JsonParseException($"Unexpected character '{c}' at position {_pos}", _pos);
            }
        }

        private object ParseObject()
        {
            _pos++; // consume '{'
            var dict = new Dictionary<string, object>();
            SkipWhitespace();
            if (_pos < _json.Length && _json[_pos] == '}')
            {
                _pos++;
                return dict;
            }
            while (true)
            {
                SkipWhitespace();
                string key = ParseString(); // ParseString already handles the leading '"' check and consumes it
                SkipWhitespace();
                if (_pos >= _json.Length || _json[_pos] != ':')
                    throw new JsonParseException($"Expected ':' after key \"{key}\" in object at position {_pos}", _pos);
                _pos++; // consume ':'
                object value = ParseValue();
                dict[key] = value;
                SkipWhitespace();
                if (_pos >= _json.Length)
                    throw new JsonParseException("Unexpected end of JSON input in object", _pos);

                char nextChar = _json[_pos];
                if (nextChar == ',')
                {
                    _pos++;
                    // Check for trailing comma before '}'
                    SkipWhitespace();
                    if (_pos < _json.Length && _json[_pos] == '}')
                        throw new JsonParseException("Trailing comma is not allowed in object", _pos);
                    continue;
                }
                if (nextChar == '}')
                {
                    _pos++;
                    break;
                }
                throw new JsonParseException($"Expected ',' or '}}' in object at position {_pos}", _pos);
            }
            return dict;
        }

        private object ParseArray()
        {
            _pos++; // consume '['
            var list = new List<object>();
            SkipWhitespace();
            if (_pos < _json.Length && _json[_pos] == ']')
            {
                _pos++;
                return list;
            }
            while (true)
            {
                object item = ParseValue();
                list.Add(item);
                SkipWhitespace();
                if (_pos >= _json.Length)
                    throw new JsonParseException("Unexpected end of JSON input in array", _pos);

                char nextChar = _json[_pos];
                if (nextChar == ',')
                {
                    _pos++;
                    // Check for trailing comma before ']'
                    SkipWhitespace();
                    if (_pos < _json.Length && _json[_pos] == ']')
                        throw new JsonParseException("Trailing comma is not allowed in array", _pos);
                    continue;
                }
                if (nextChar == ']')
                {
                    _pos++;
                    break;
                }
                throw new JsonParseException($"Expected ',' or ']' in array at position {_pos}", _pos);
            }
            return list;
        }

        private string ParseString()
        {
            SkipWhitespace(); // Ensure we start at the quote
            if (_pos >= _json.Length || _json[_pos] != '"')
                throw new JsonParseException($"Expected '\"' to start a string at position {_pos}", _pos);

            _pos++; // consume opening '"'
            var sb = new StringBuilder();
            while (_pos < _json.Length)
            {
                char c = _json[_pos++];
                if (c == '"')
                    return sb.ToString(); // Found closing quote
                if (c == '\\') // Handle escape sequences
                {
                    if (_pos >= _json.Length)
                        throw new JsonParseException("Unexpected end of JSON input after escape character in string", _pos);
                    char esc = _json[_pos++];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break; // Solidus (optional escape)
                        case 'b': sb.Append('\b'); break; // Backspace
                        case 'f': sb.Append('\f'); break; // Form feed
                        case 'n': sb.Append('\n'); break; // Newline
                        case 'r': sb.Append('\r'); break; // Carriage return
                        case 't': sb.Append('\t'); break; // Horizontal tab
                        case 'u': // Unicode escape \uXXXX
                            if (_pos + 4 > _json.Length)
                                throw new JsonParseException("Invalid Unicode escape sequence: not enough characters", _pos - 1);
                            string hex = _json.Substring(_pos, 4);
                            if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int unicodeChar))
                                throw new JsonParseException($"Invalid Unicode escape sequence '\\u{hex}'", _pos - 1);
                            sb.Append((char)unicodeChar);
                            _pos += 4;
                            break;
                        default:
                            throw new JsonParseException($"Invalid escape character '\\{esc}' in string", _pos - 1);
                    }
                }
                else if (c < ' ') // Control characters (U+0000 to U+001F) must be escaped
                {
                    throw new JsonParseException($"Invalid character '{c}' ({(int)c}) in string. Control characters must be escaped.", _pos - 1);
                }
                else
                {
                    sb.Append(c); // Regular character
                }
            }
            // If loop finishes, we didn't find the closing quote
            throw new JsonParseException("Unexpected end of JSON input: unterminated string", _pos);
        }

        private object ParseNumber()
        {
            int start = _pos;
            var sb = new StringBuilder();
            bool isDouble = false;

            // Optional minus sign
            if (_json[_pos] == '-')
            {
                sb.Append(_json[_pos]);
                _pos++;
            }

            if (_pos >= _json.Length)
                throw new JsonParseException("Unexpected end of JSON input after sign in number", _pos);


            // Integer part
            int integerStart = _pos;
            if (_json[_pos] == '0')
            {
                sb.Append(_json[_pos]);
                _pos++;
                // Check for invalid leading zero like "01", "02", etc.
                if (_pos < _json.Length && char.IsDigit(_json[_pos]))
                    throw new JsonParseException("Leading zeros are not allowed in numbers", integerStart);
            }
            else if (char.IsDigit(_json[_pos]))
            {
                while (_pos < _json.Length && char.IsDigit(_json[_pos]))
                {
                    sb.Append(_json[_pos]);
                    _pos++;
                }
            }
            else
            {
                throw new JsonParseException($"Expected digit after sign in number at position {_pos}", _pos);
            }


            // Fractional part
            if (_pos < _json.Length && _json[_pos] == '.')
            {
                isDouble = true;
                sb.Append(_json[_pos]);
                _pos++;

                if (_pos >= _json.Length || !char.IsDigit(_json[_pos]))
                    throw new JsonParseException($"Expected digit after decimal point in number at position {_pos}", _pos);

                while (_pos < _json.Length && char.IsDigit(_json[_pos]))
                {
                    sb.Append(_json[_pos]);
                    _pos++;
                }
            }

            // Exponent part
            if (_pos < _json.Length && (_json[_pos] == 'e' || _json[_pos] == 'E'))
            {
                isDouble = true;
                sb.Append(_json[_pos]);
                _pos++;

                if (_pos >= _json.Length)
                    throw new JsonParseException("Unexpected end of JSON input in number exponent", _pos);


                if (_json[_pos] == '+' || _json[_pos] == '-')
                {
                    sb.Append(_json[_pos]);
                    _pos++;
                }

                if (_pos >= _json.Length || !char.IsDigit(_json[_pos]))
                    throw new JsonParseException($"Expected digit after exponent sign in number at position {_pos}", _pos);


                while (_pos < _json.Length && char.IsDigit(_json[_pos]))
                {
                    sb.Append(_json[_pos]);
                    _pos++;
                }
            }

            string numStr = sb.ToString();

            try
            {
                if (isDouble)
                {
                    // Use InvariantCulture to ensure '.' is the decimal separator
                    if (double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                        return d;
                }
                else
                {
                    // Use InvariantCulture for consistency
                    if (long.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
                        return l;
                    // If long parsing fails but the number is very large, try decimal or double
                    if (decimal.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal dec))
                        return dec; // Or handle as double if decimal is not desired
                    if (double.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out double dbl))
                        return dbl;
                }
            }
            catch (OverflowException ex)
            {
                throw new JsonParseException($"Number '{numStr}' is too large or too small at position {start}", start, ex);
            }


            throw new JsonParseException($"Invalid number format '{numStr}' at position {start}", start);
        }

        // Optimized literal parsing without Substring
        private object ParseTrue()
        {
            if (_pos + 3 < _json.Length &&
                _json[_pos] == 't' && _json[_pos + 1] == 'r' &&
                _json[_pos + 2] == 'u' && _json[_pos + 3] == 'e')
            {
                _pos += 4;
                return true;
            }
            throw new JsonParseException($"Expected 'true' literal at position {_pos}", _pos);
        }

        private object ParseFalse()
        {
            if (_pos + 4 < _json.Length &&
               _json[_pos] == 'f' && _json[_pos + 1] == 'a' &&
               _json[_pos + 2] == 'l' && _json[_pos + 3] == 's' &&
               _json[_pos + 4] == 'e')
            {
                _pos += 5;
                return false;
            }
            throw new JsonParseException($"Expected 'false' literal at position {_pos}", _pos);
        }

        private object ParseNull()
        {
            if (_pos + 3 < _json.Length &&
               _json[_pos] == 'n' && _json[_pos + 1] == 'u' &&
               _json[_pos + 2] == 'l' && _json[_pos + 3] == 'l')
            {
                _pos += 4;
                return null;
            }
            throw new JsonParseException($"Expected 'null' literal at position {_pos}", _pos);
        }
    }

    // Custom exception class for better error reporting
    public class JsonParseException : Exception
    {
        public int Position { get; }

        public JsonParseException(string message, int position) : base($"{message} (Position: {position})")
        {
            Position = position;
        }
        public JsonParseException(string message, int position, Exception innerException) : base($"{message} (Position: {position})", innerException)
        {
            Position = position;
        }
    }
}
