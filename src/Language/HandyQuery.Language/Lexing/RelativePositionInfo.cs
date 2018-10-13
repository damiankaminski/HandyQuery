namespace HandyQuery.Language.Lexing
{
    internal readonly ref struct RelativePositionInfo
    {
        public readonly int Line;
        public readonly int Column;

        public RelativePositionInfo(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }
}