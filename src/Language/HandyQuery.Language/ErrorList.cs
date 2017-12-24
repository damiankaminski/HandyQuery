using System.Collections.Generic;
using System.Linq;

namespace HandyQuery.Language
{
    internal class ErrorList : List<Error>
    {
        /// <summary>
        /// Combines multiple errors into just one, readable even for non-technical people.
        /// </summary>
        public bool TryGetSingleError(out string error)
        {
            if (Count == 1)
            {
                error = this[0].Message;
                return true;
            }

            if (Count == 2
                && this.Any(x => x.Id == ErrorId.StatementNotFound)
                && this.Any(x => x.Id == ErrorId.CompareOperatorNotFound)
                && this[0].Subject == this[1].Subject)
            {
                error = $"\"{this[0].Subject}\" is neither statement nor compare operator.";
                return true;
            }

            // these are rather not helpful to non-technical people
            // TODO: RemoveAll(x => x.Id == ErrorId.ParenOpenNotFound);
            // TODO: RemoveAll(x => x.Id == ErrorId.ParenCloseNotFound);

            // TODO: translations?
            error = this.FirstOrDefault(x => x.Id == ErrorId.ColumnNotFound)?.Message
                // TODO: ?? this.FirstOrDefault(x => x.Id == ErrorId.NotSupportedCompareOperator)?.Message
                // TODO: ?? this.FirstOrDefault(x => x.Id == ErrorId.ParenCloseWithoutParenOpen)?.Message;
                ;

            return error != null;
        }

        public string[] GetSingleMessageIfPossible()
        {
            string error;
            if (TryGetSingleError(out error))
            {
                return new[] { error };
            }

            return this.Select(x => x.Message).ToArray();
        }
    }
}