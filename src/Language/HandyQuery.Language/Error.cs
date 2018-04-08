namespace HandyQuery.Language
{
    // TODO: change to struct
    internal sealed class Error
    {
        public readonly string Message;
        public readonly ErrorId Id;
        public readonly string Subject;
        public readonly RangeInfo Range;

        public Error(string message, ErrorId id, RangeInfo range, string subject = null)
        {
            Message = message;
            Id = id;
            Range = range;
            Subject = subject;
        }

        internal struct RangeInfo
        {
            public readonly int Position;
            public readonly int Length;

            public RangeInfo(int position, int length)
            {
                Position = position;
                Length = length;
            }
        }
    }
    
    internal enum ErrorId
    {
        ColumnNotFound,
        StatementNotFound,
        CompareOperatorNotFound,
        LogicalOperatorNotFound
    }
}