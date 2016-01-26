using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Data;
using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Helpers;
using Sitecore.Mvc.Presentation;
using Sitecore.Pipelines;
using Sitecore.Pipelines.RenderField;

namespace PT.Framework.ViewModelResolver.Presentation
{
  public abstract class ViewModel : RenderingModel
  {
    private Stack<string> _endFieldStack;

    protected virtual Stack<string> EndFieldStack
    {
      get { return _endFieldStack ?? (_endFieldStack = new Stack<string>()); }
    }

    public virtual IDisposable RenderField(HtmlHelper htmlHelper, ID fieldId)
    {
      return RenderField(htmlHelper, fieldId, null);
    }

    public virtual IDisposable RenderField(HtmlHelper htmlHelper, ID fieldId, object parameters)
    {
      var disposableFieldBlock = new RenderedFieldBlock(htmlHelper, this);
      disposableFieldBlock.BeginField(fieldId, parameters);
      return disposableFieldBlock;
    }

    public virtual HtmlString BeginField(ID fieldId, bool skipIfFieldIsEmpty = false)
    {
      return BeginField(fieldId.ToString(), null, skipIfFieldIsEmpty);
    }

    public virtual HtmlString BeginField(ID fieldId, object parameters, bool skipIfFieldIsEmpty = false)
    {
      return BeginField(fieldId.ToString(), parameters, skipIfFieldIsEmpty);
    }

    public virtual HtmlString RenderField(ID fieldId)
    {
      return RenderField(fieldId.ToString(), null);
    }

    public virtual HtmlString RenderField(ID fieldId, object parameters)
    {
      return RenderField(fieldId.ToString(), parameters);
    }

    protected virtual HtmlString RenderField(string fieldNameOrId, object parameters)
    {
      return new HtmlString(BeginField(fieldNameOrId, parameters).ToString() + EndField());
    }

    protected virtual HtmlString BeginField(string fieldNameOrId, object parameters, bool skipIfFieldIsEmpty = false)
    {
      var renderFieldArgs = new RenderFieldArgs
      {
        Item = Item,
        FieldName = fieldNameOrId
      };
      if (parameters != null)
      {
        TypeHelper.CopyProperties(parameters, renderFieldArgs);
        TypeHelper.CopyProperties(parameters, renderFieldArgs.Parameters);
      }
      renderFieldArgs.Item = renderFieldArgs.Item ?? PageItem;
      if (renderFieldArgs.Item == null)
      {
        EndFieldStack.Push(string.Empty);
        return new HtmlString(string.Empty);
      }
      CorePipeline.Run("renderField", renderFieldArgs);
      var renderFieldResult = renderFieldArgs.Result;
      var firstPart = renderFieldResult.ValueOrDefault(result => result.FirstPart).OrEmpty();
      EndFieldStack.Push(renderFieldResult.ValueOrDefault(result => result.LastPart).OrEmpty());
      return new HtmlString(firstPart);
    }

    public virtual HtmlString EndField()
    {
      if (!EndFieldStack.Any())
        throw new InvalidOperationException("There was a call to EndField with no corresponding call to BeginField");
      return new HtmlString(EndFieldStack.Pop());
    }
  }
}