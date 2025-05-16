using System.Collections.Generic;
using System.Text;
static class Parser
{
    public static Rule Parse(string str)
    {
        var rule = new Rule(new BitArray(10), new BitArray(10));
        foreach (var token in Tokenize(str))
        {
            LoadToken(token, ref rule);
        }
        return rule;
    }
    public static List<Token> Tokenize(string str)
    {
        var result = new List<Token>();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '/')
            {
                var token = builder.ToString();
                result.Add(new Token(token, GetTokenType(token)));
                builder = new StringBuilder();
                i++;
            }
            builder.Append(str[i]);
        }
        var lastToken = builder.ToString();
        result.Add(new Token(lastToken, GetTokenType(lastToken)));
        return result;
    }
    public static TokenType GetTokenType(string token)
    {
        switch (token[0])
        {
            case 'B':
                return TokenType.Birth;
            case 'S':
                return TokenType.Survival;
            default:
                return TokenType.Unknown;
        }
    }
    public static void LoadToken(Token token, ref Rule rule)
    {
        switch (token.Type)
        {
            case TokenType.Birth:
                rule.Birth = GetValuesFromString(token.Content, false);
                break;
            case TokenType.Survival:
                rule.Survival = GetValuesFromString(token.Content, false);
                break;
        }
    }
    public static BitArray GetValuesFromString(string str, bool ignoreFirstSymbol)
    {
        var result = new BitArray(10);
        foreach (var symbol in str)
        {
            if (ignoreFirstSymbol)
            {
                ignoreFirstSymbol = false;
                continue;
            }
            switch (symbol)
            {
                case '0':
                    result[0] = true;
                    break;
                case '1':
                    result[1] = true;
                    break;
                case '2':
                    result[2] = true;
                    break;
                case '3':
                    result[3] = true;
                    break;
                case '4':
                    result[4] = true;
                    break;
                case '5':
                    result[5] = true;
                    break;
                case '6':
                    result[6] = true;
                    break;
                case '7':
                    result[7] = true;
                    break;
                case '8':
                    result[8] = true;
                    break;
                case '9':
                    result[9] = true;
                    break;
            }
        }
        return result;
    }
}
class Token
{
    public Token(string content, TokenType type)
    {
        Content = content;
        Type = type;
    }

    public string Content { get; }
    public TokenType Type { get; }
}
enum TokenType
{
    Unknown,
    Birth,
    Survival,
}