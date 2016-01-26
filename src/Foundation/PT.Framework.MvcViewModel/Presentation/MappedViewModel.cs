using System;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using PT.Framework.ViewModelResolver.Infrastructure;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace PT.Framework.ViewModelResolver.Presentation
{
  public class MappedViewModel<T> : ViewModel
  {
    public virtual HtmlString RenderField(Expression<Func<T, object>> expression, object parameters = null)
    {
      var fieldId = GetFieldId(expression);
      if (!fieldId.IsNull)
      {
        return RenderField(fieldId, parameters);
      }
      Log.Info(string.Format("Property {0} does not have a field ID mapped on {1}", expression.Body, GetType().Name),
        this);
      return new HtmlString(string.Empty);
    }

    public virtual HtmlString BeginField(Expression<Func<T, object>> expression, object parameters = null)
    {
      var fieldId = GetFieldId(expression);
      if (!fieldId.IsNull)
      {
        return BeginField(fieldId, parameters);
      }
      Log.Info(string.Format("Property {0} does not have a field ID mapped on {1}", expression.Body, GetType().Name),
        this);
      return new HtmlString(string.Empty);
    }

    public virtual IDisposable RenderField(HtmlHelper htmlHelper, Expression<Func<T, object>> expression, object parameters = null)
    {
      var fieldId = GetFieldId(expression);
      if (!fieldId.IsNull)
      {
        return RenderField(htmlHelper, fieldId, parameters);
      }
      Log.Info(string.Format("Property {0} does not have a field ID mapped on {1}", expression.Body, GetType().Name),
        this);
      return null;
    }

    public virtual string GetFieldValue(Expression<Func<T, object>> expression)
    {
      var fieldId = GetFieldId(expression);
      if (!fieldId.IsNull)
        return Item[fieldId];
      Log.Info(string.Format("Property {0} does not have a field ID mapped on {1}", expression.Body, GetType().Name),
        this);
      return string.Empty;
    }

    public virtual ID GetFieldId(Expression<Func<T, object>> expression)
    {
      var propName = GetPropertyNameFromExpressionService.Get(expression);
      if (string.IsNullOrEmpty(propName))
        return ID.Null;

      var fullyQualifiedName = typeof (T).FullName;
      var key = string.Format("{0}.{1}", fullyQualifiedName, propName);

      ID itemId;
      if (MappingTable.Instance.Map.TryGetValue(key, out itemId))
        return itemId;

      Log.Error(string.Concat("Could not find ID for", key, " in MappingTable"), this);
      return ID.Null;
    }
  }
}