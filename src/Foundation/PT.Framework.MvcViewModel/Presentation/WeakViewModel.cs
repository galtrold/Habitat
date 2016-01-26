using System;
using System.Linq.Expressions;
using System.Web;
using PT.Framework.ViewModelResolver.Infrastructure;

namespace PT.Framework.ViewModelResolver.Presentation
{
  public class WeakViewModel<T> : ViewModel
  {
    public virtual HtmlString BeginField(Expression<Func<T, object>> expression, object parameters = null)
    {
      var propName = GetPropertyNameFromExpressionService.Get(expression);
      return BeginField(propName, parameters);
    }

    public virtual HtmlString RenderField(Expression<Func<T, object>> expression, object parameters = null)
    {
      var propName = GetPropertyNameFromExpressionService.Get(expression);
      return RenderField(propName, parameters);
    }

    public virtual string GetFieldValue(Expression<Func<T, object>> expression)
    {
      var propName = GetPropertyNameFromExpressionService.Get(expression);
      return Item[propName];
    }
  }
}