using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using Sitecore.Data;
using Sitecore.Mvc.Extensions;

namespace PT.Framework.ViewModelResolver.Presentation
{
  public class RenderedFieldBlock : IDisposable
  {
    private readonly HtmlHelper _htmlHelper;
    private readonly ViewModel _model;
    private readonly StringBuilder _pageContent;
    private readonly StringBuilder _previousPageContent;
    private bool _disposed;
    private bool _shouldRender;

    public RenderedFieldBlock(HtmlHelper htmlHelper, ViewModel model)
    {
      _htmlHelper = htmlHelper;
      _model = model;
      _pageContent = ((StringWriter) _htmlHelper.ViewContext.Writer).GetStringBuilder();
      _previousPageContent = new StringBuilder().Append(_pageContent);
    }

    public void Dispose()
    {
      if (_disposed)
        return;
      _disposed = true;

      if (!_shouldRender)
      {
        _pageContent.Length = 0;
        _pageContent.Append(_previousPageContent);
        return;
      }
      _htmlHelper.ViewContext.Writer.Write(_model.EndField());
    }

    public virtual void BeginField(ID fieldNameOrId, object parameters)
    {
      var beginField = _model.BeginField(fieldNameOrId, parameters, true);
      _shouldRender = !string.IsNullOrWhiteSpace(beginField.ToStringOrEmpty());
      if (!_shouldRender)
        return;
      _htmlHelper.ViewContext.Writer.Write(beginField);
    }
  }
}