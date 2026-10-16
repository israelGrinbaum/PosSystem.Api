using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PosSystem.Api.Data
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool IsUniqueViolation(this DbUpdateException exception, string indexName) =>
            exception.InnerException is SqlException sqlException
            && (sqlException.Number is 2601 or 2627)
            && sqlException.Message.Contains(indexName);
    }
}
