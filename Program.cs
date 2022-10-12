using System;
using System.Collections.Generic;
using System.Globalization;

namespace godLANG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write(">> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                var lexer = new Lexer(line);

                while (true)
                {
                    var token = lexer.NextToken();

                    if (token.Kind == SyntaxKind.EndOfFileToken)
                    {
                        break;
                    }

                    Console.Write($"{token.Kind}:  '{token.Text}'");

                    if (token.Value != null)
                    {
                        Console.Write($" {token.Value}");
                    }

                    Console.WriteLine();
                }
            }
        }
    }


    enum SyntaxKind
    {
        NumberToken,
        WhitespaceToken,
        PlusToken,
        MiusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,
        NumberExpression,
        BinaryExpression
    }

    class SyntaxToken
    {
        //private int value;

        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;

        }



        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
        public SyntaxKind Kind { get; }
    }

    class Lexer
    {
        private readonly string _text;
        private int _position = 0;

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current
        {

            get
            {
                if (_position >= _text.Length)
                    return '\0';

                return _text[_position];
            }
        }

        public void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken()
        {

            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            }


            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                {
                    Next();
                }

                var lenght = _position - start;
                var text = _text.Substring(start, lenght);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }


            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                var lenght = _position - start;
                var text = _text.Substring(start, lenght);
                //int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.WhitespaceToken, start, text, null);

            }


            if (Current == '+')
            {
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            }

            else if (Current == '-')
            {
                return new SyntaxToken(SyntaxKind.MiusToken, _position++, "-", null);
            }

            else if (Current == '*')
            {
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
            }

            else if (Current == '/')
            {
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
            }

            else if (Current == '(')
            {
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
            }

            else if (Current == ')')
            {
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);
            }

            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);

        }

    }


    abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind
        {
            get;
        }

        public 
    }

    abstract class ExpressionSyntax: SyntaxNode
    {

    }

    sealed class NumberExpressionSyntax: ExpressionSyntax
    {
        private readonly SyntaxToken numberToken;

        public override SyntaxKind Kind => SyntaxKind.NumberExpression;

        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            this.numberToken = numberToken;
        }

        public SyntaxToken NumberToken { get; }
     
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public ExpressionSyntax Left { get; }
        public ExpressionSyntax Right { get;  }
        public SyntaxToken OperatorToken { get;  }

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            Right = right;
            OperatorToken = operatorToken;
        }
    }

    class Parser
    {

        private readonly SyntaxToken[] _tokens;
        private int _position;

        public Parser(string text)
        {

            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();


                if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }


            } while (token.Kind != SyntaxKind.EndOfFileToken);


            _tokens = tokens.ToArray();


        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
            {
                return _tokens[_tokens.Length - 1];
            }

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if(Current.Kind == kind)
            { 
                return NextToken(); 
            }

            return new SyntaxToken(kind, Current.Position,  null, null);

        }

        public ExpressionSyntax Parse()
        {
            var left = ParsePrimaryExpression();

            while (Current.Kind == SyntaxKind.PlusToken || Current.Kind == SyntaxKind.MiusToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }

}
