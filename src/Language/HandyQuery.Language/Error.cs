namespace HandyQuery.Language
{
    // TODO: change to struct
    internal sealed class Error
    {
        public readonly string Message;
        public readonly ErrorId Id;
        public readonly string Subject;

        public Error(string message, ErrorId id, string subject = null)
        {
            Message = message;
            Id = id;
            Subject = subject;
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