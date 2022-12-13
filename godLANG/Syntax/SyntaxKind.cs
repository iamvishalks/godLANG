namespace godLANG.Syntax
{
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
        UnaryExpression,
        LiteralExpression,
        BinaryExpression,
        ParenthesizedExpression,
    }

}

