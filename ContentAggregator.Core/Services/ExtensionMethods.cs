using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContentAggregator.Core.Services
{
    public static class ExtensionMethods
    {
        //private const string ErrorCodeKey = "errorCode";

        //// This might be used in ErrorHandlingMiddleware to return small hashes for the same type of problems to web users
        //// TODO: For now not needed. Might move out to a feature branch for future reference.
        //public static Exception AddErrorCode(this Exception exception)
        //{
        //    using var sha1 = SHA1.Create();
        //    var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(exception.Message));
        //    var errorCode = string.Concat(hash[..5].Select(b => b.ToString("x")));
        //    exception.Data[ErrorCodeKey] = errorCode;
        //    return exception;
        //}

        //public static string? GetErrorCode(this Exception exception)
        //{
        //    return (string?)exception.Data[ErrorCodeKey];
        //}
    }
}
