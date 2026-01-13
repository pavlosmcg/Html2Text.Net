namespace Html2Text.Lexing;

internal enum TokenType
{
    Text,
    Opening,
    Closing,
    SelfClosing,
    DocType,
    ProcessingInstruction,
    Comment
}
