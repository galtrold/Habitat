using System;
using System.Linq.Expressions;
using PT.Framework.ViewModelResolver.Infrastructure;
using Sitecore.Data;

namespace PT.Framework.ViewModelResolver.Presentation
{
    public class MappedViewModelConfiguration<T>
    {
        public void SetClassPropertyFieldId(Expression<Func<T, object>> expression, ID fieldId)
        {
            var fullyQualifiedClassName = typeof(T).FullName;
            var propertyName = GetPropertyNameFromExpressionService.Get(expression);
            MappingTable.Instance.Map.Add(string.Format("{0}.{1}", fullyQualifiedClassName, propertyName), fieldId);
        }
    }
}