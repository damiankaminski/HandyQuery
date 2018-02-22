# Ideas

## TokenizationResult to struct

```c#
enum TokenType {
    ColumnName,
    // ...
}
 
struct TokenizationResult
{
    public readonly bool Success;
    public readonly Error Error;
    public readonly TokenType TokenType;
}
 
struct Error 
{
    public readonly string Message;
    public readonly ErrorId Id;
 
    public Error(string message, ErrorId id)
    {
        Message = message;
        Id = id;
    }
}
 
class ColumnNameTokenizer
{
    // Code generation would be needed to invoke this method. 
    // Structs cannot use interfaces to make them live on the stack.
    (TokenizationResult, ColumnNameToken?) Tokenize(LexerRuntimeInfo info) { /* ... */ }
}
```

## Error messages pool / lazy creation

Each error message is a string allocated on the heap. They are allocated even if not used (could happen if 
other lexer path is valid one). It should be possible to create a messages pool, or create them only on demand.