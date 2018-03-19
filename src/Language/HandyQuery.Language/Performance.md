# Ideas

## TokenizationResult to struct

```c#
enum TokenType 
{
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

## Zero cost extension points

```c#
interface ICharComparer 
{
    bool Equals(char a, char b);
}
struct CaseInsensitiveCharComparer : ICharComparer 
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(char a, char b) => a == b || char.ToLower(a) == char.ToLower(b);
}
struct CaseSensitiveCharComparer : ICharComparer 
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(char a, char b) => a == b;
}
class Parser
{
    public void Parse<TCharComparer>(TCharComparer comparer) : where TCharComparer : struct, ICharComparer
    {
        // ...
        // thanks to `where TCharComparer : struct, ICharComparer`
        // either CaseInsensitiveCharComparer.Equals or CaseSensitiveCharComparer.Equals will be inlined here!
        // it will generate code twice if called once with one version then with the other
        // no need for any branches like if (caseSensitive) { ... } else { ... }
        var equals = comparer.Equals(a, b);
        // ...
    }
}
```

Source: https://www.youtube.com/watch?time_continue=2026&v=7GTpwgsmHgU

Could be useful with various syntax options (e.g. case sensivity).