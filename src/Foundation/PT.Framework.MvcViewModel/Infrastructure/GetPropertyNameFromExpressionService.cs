using System;
using System.Linq.Expressions;

namespace PT.Framework.ViewModelResolver.Infrastructure
{
  internal class GetPropertyNameFromExpressionService
  {
    internal static string Get<T>(Expression<Func<T, object>> expression)
    {
      var body = expression.Body as MemberExpression;
      return body != null ? body.Member.Name : string.Empty;
    }
  }
}