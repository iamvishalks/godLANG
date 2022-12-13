namespace godLANG.Syntax
{
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

}

