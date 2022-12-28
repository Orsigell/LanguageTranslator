using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageTranslator
{
    public class LexicalAnalyser
    {
        public List<string> Separators = (new string[] { ",", ":", ";", ":=", "..", "*", "/", "+", "-", ".", "(", ")" }).ToList();
        public List<string> Keys = (new string[] { "var", "integer", "float", "double", "begin", "case", "of", "end", "else" }).ToList();
        public List<string> Variables = new List<string>();
        public List<string> Literals = new List<string>();
        public List<string> this[Token.TokenType index]
        {
            get
            {
                switch (index)
                {
                    case Token.TokenType.Keys:
                        return Keys;
                    case Token.TokenType.Separators:
                        return Separators;
                    case Token.TokenType.Variables:
                        return Variables;
                    case Token.TokenType.Literals:
                        return Literals;
                }
                return null;
            }
        }
        public class Lexeme
        {
            public Lexeme(string text, LexemeType state)
            {
                Text = text;
                State = state;
            }
            public enum LexemeType
            {
                S,
                R,
                L,
                I
            }
            public string Text { get; set; }
            public LexemeType State { get; set; }
            public string StateToString()
            {
                switch (State)
                {
                    case LexemeType.R:
                        return "Разделитель";
                    case LexemeType.L:
                        return "Литерал";
                    case LexemeType.I:
                        return "Идентификатор";
                    default:
                        return "";
                }
            }
        }
        private readonly string text;
        private string buffer;
        private Lexeme.LexemeType state;
        readonly List<Lexeme> Lexemes = new List<Lexeme>();
        public LexicalAnalyser(string text)
        {
            Lexemes = new List<Lexeme>();
            this.text = text;
        }
        public List<Lexeme> Analys()
        {
            foreach (char item in text)
            {
                if (IsSkipChar(item))
                {
                    switch (state)
                    {
                        case Lexeme.LexemeType.R:
                        case Lexeme.LexemeType.L:
                        case Lexeme.LexemeType.I:
                            OutClear();
                            break;
                    }
                }
                else if (IsDigit(item))
                {
                    switch (state)
                    {
                        case Lexeme.LexemeType.S:
                            buffer += item;
                            state = Lexeme.LexemeType.L;
                            break;
                        case Lexeme.LexemeType.R:
                            OutClear();
                            buffer += item;
                            state = Lexeme.LexemeType.L;
                            break;
                        case Lexeme.LexemeType.L:
                        case Lexeme.LexemeType.I:
                            buffer += item;
                            break;
                    }
                }
                else if (IsChar(item))
                {
                    switch (state)
                    {
                        case Lexeme.LexemeType.S:
                            buffer += item;
                            state = Lexeme.LexemeType.I;
                            break;
                        case Lexeme.LexemeType.R:
                            OutClear();
                            buffer += item;
                            state = Lexeme.LexemeType.I;
                            break;
                        case Lexeme.LexemeType.L:
                            OutClear();
                            buffer += item;
                            state = Lexeme.LexemeType.I;
                            break;
                        case Lexeme.LexemeType.I:
                            buffer += item;
                            break;
                    }
                }
                else if (IsSeparator(item))
                {
                    switch (state)
                    {
                        case Lexeme.LexemeType.S:
                            buffer += item;
                            state = Lexeme.LexemeType.R;
                            break;
                        case Lexeme.LexemeType.R:
                            if (IsCompositeSeparator(item, buffer.Length))
                            {
                                buffer += item;
                            }
                            else
                            {
                                OutClear();
                                buffer += item;
                                state = Lexeme.LexemeType.R;
                            }
                            break;
                        case Lexeme.LexemeType.L:
                        case Lexeme.LexemeType.I:
                            OutClear();
                            buffer += item;
                            state = Lexeme.LexemeType.R;
                            break;
                    }
                }
                else
                {
                    throw new Exception("Неразрешенный символ");
                }
            }
            OutClear();
            return Lexemes;
        }
        public class Token
        {
            public enum TokenType
            {
                Keys = 0,
                Separators = 1,
                Variables = 2,
                Literals = 3
            }
            public Token(TokenType type, int index)
            {
                Type = type;
                Index = index;
            }
            public TokenType Type;
            public int Index;
        }
        public List<Token> CompilingTokens(List<Lexeme> lexemes)
        {
            List<Token> tokens = new List<Token>();
            foreach (var item in lexemes)
            {
                switch (item.State)
                {
                    case Lexeme.LexemeType.L:
                        if (!Literals.Contains(item.Text))
                        {
                            Literals.Add(item.Text);
                            tokens.Add(new Token(Token.TokenType.Literals, Literals.Count - 1));
                        }
                        else
                        {
                            tokens.Add(new Token(Token.TokenType.Literals, Literals.IndexOf(item.Text)));
                        }
                        break;
                    case Lexeme.LexemeType.I:
                        if (!Keys.Contains(item.Text))
                        {
                            if (!Variables.Contains(item.Text))
                            {
                                Variables.Add(item.Text);
                                tokens.Add(new Token(Token.TokenType.Variables, Variables.Count - 1));
                            }
                            else
                            {
                                tokens.Add(new Token(Token.TokenType.Variables, Variables.IndexOf(item.Text)));
                            }
                        }
                        else
                        {
                            tokens.Add(new Token(Token.TokenType.Keys, Keys.IndexOf(item.Text)));
                        }
                        break;
                    case Lexeme.LexemeType.R:
                        tokens.Add(new Token(Token.TokenType.Separators, Separators.IndexOf(item.Text)));
                        break;
                }
            }
            return tokens;
        }

        //private int GetStrIndexFromArr(string[] keys,string str)
        //{
        //    for (int i = 0; i < keys.Length; i++)
        //    {
        //        if (keys[i] == str)
        //        {
        //            return i;
        //        }
        //    }
        //    throw new Exception("Последовательность не содержит строки");
        //}

        private bool IsSkipChar(char item)
        {
            switch (item)
            {
                case ' ':
                    return true;
                case '\r':
                    return true;
                case '\n':
                    return true;
            }
            return false;
        }

        private void OutClear()
        {
            Lexemes.Add(new Lexeme(buffer, state));
            buffer = "";
            state = Lexeme.LexemeType.S;
        }

        private bool IsSeparator(char item)
        {
            if (Separators.FirstOrDefault(s => s.Contains(item)) != null)
            {
                return true;
            }
            return false;
        }
        private bool IsCompositeSeparator(char item, int index)
        {
            if (Separators.FirstOrDefault(s => (s.Length > index) && (s[index] == item)) != null)
            {
                return true;
            }
            return false;
        }

        private bool IsChar(char item)
        {
            if (item >= 65 && item <= 90 || item >= 97 && item <= 122)
            {
                return true;
            }
            return false;
        }

        private bool IsDigit(char item)
        {
            if (char.IsDigit(item))
            {
                return true;
            }
            return false;
        }
    }
}
