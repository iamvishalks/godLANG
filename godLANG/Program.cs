using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace godLANG
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            bool showTree = false;

            while (true)
            {
                Console.Write(">> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                if (line == "#showTree")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees");
                    continue;
                }
                else if (line == "#cls")
                {
                    Console.Clear();
                    continue;
                }

                var syntaxTree = SyntaxTree.Parse(line);

                if (showTree)
                {

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();
                }

                if (!syntaxTree.Diagnostics.Any())
                {
                    var e = new Evaluator(syntaxTree.Root);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
                }
                else
                {

                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    foreach (var diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);

                    Console.ResetColor();
                }
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();

            indent += isLast ? "   " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                PrettyPrint(child, indent, child == lastChild);
        }
    }


    public enum SyntaxKind
    {
        //Tokens
        BadToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,

        //Expressions
        LiteralExpression,
        BinaryExpression,
        ParenthesizedExpression,

    }

    public sealed class SyntaxToken : SyntaxNode
    {
        //private int value;

        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;


        }



        public override SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }


        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    } //

    internal sealed class Lexer
    {
        private readonly string _text;
        private int _position;
        private List<string> _diagnostics = new List<string>();

        public Lexer(string text)
        {
            _text = text;
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

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

        public SyntaxToken Lex()
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
                if (!int.TryParse(text, out var value))
                {
                    _diagnostics.Add($"The number {_text} IS NOT of type 'Int32'");
                }


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


            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);
            }

            _diagnostics.Add($"ERROR:: bad character input: '{Current}' ");

            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);

        }

    } //


    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind
        {
            get;
        }

        public abstract IEnumerable<SyntaxNode> GetChildren();
    } //

    public abstract class ExpressionSyntax : SyntaxNode
    {

    } //

    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        // private readonly SyntaxToken NumberToken;



        public LiteralExpressionSyntax(SyntaxToken literalToken)
        {
            LiteralToken = literalToken;
        }


        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        public SyntaxToken LiteralToken { get; }


        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }


    } //

    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {


        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            Right = right;
            OperatorToken = operatorToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public ExpressionSyntax Left { get; }
        public ExpressionSyntax Right { get; }
        public SyntaxToken OperatorToken { get; }



        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;

        }
    } //

    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {


        public ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closeParenthesisToken)
        {
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            CloseParenthesisToken = closeParenthesisToken;
            // Expression = expression;
            // CloseParenthesisToken = closeParenthesisToken;

        }


        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken CloseParenthesisToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return CloseParenthesisToken;
        }

    } //

    public sealed class SyntaxTree
    {
        public SyntaxTree(IReadOnlyList<string> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }

        public static SyntaxTree Parse(string text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }
    } //

    internal sealed class Parser
    {

        private readonly SyntaxToken[] _tokens;

        private List<string> _diagnostics = new List<string>();
        private int _position;

        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.Add($"ERROR: Unexpected token <{Current.Kind}>, expected <{kind}>");
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public SyntaxTree Parse()
        {
            var expresion = ParseExpression();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expresion, endOfFileToken);
        }


        private ExpressionSyntax ParseExpression(int parentPresedence = 0)
        {
            var left = ParsePrimaryExpression();

            while (true)
            {
                var presedence = GetBinaryOperatorPresedence(Current.Kind);
                if (presedence == 0 || presedence <= parentPresedence)
                    break;

                var operatorToken = NextToken();
                var right = ParseExpression(presedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private static int GetBinaryOperatorPresedence(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 2;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 1;
                default:
                    return 0;
            }
        }




        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = MatchToken(SyntaxKind.CloseParenthesisToken);
                return new ParenthesizedExpressionSyntax(left, expression, right);
            }

            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        } //
    } //
    public sealed class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            this._root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            if (node is LiteralExpressionSyntax n)
            {
                return (int)n.LiteralToken.Value;
            }

            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
                {
                    return left + right;
                }

                else if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
                {
                    return left - right;
                }
                else if (b.OperatorToken.Kind == SyntaxKind.StarToken)
                {
                    return left * right;
                }
                else if (b.OperatorToken.Kind == SyntaxKind.SlashToken)
                {
                    return left / right;
                }
                else
                {
                    throw new Exception($"Unexpected BINARY OPERATOR {b.OperatorToken.Kind}");
                }

            }

            if (node is ParenthesizedExpressionSyntax p)
            {
                return EvaluateExpression(p.Expression);


            }

            throw new Exception($"Unexpected NODE {node.Kind}");


        }
    }

}

