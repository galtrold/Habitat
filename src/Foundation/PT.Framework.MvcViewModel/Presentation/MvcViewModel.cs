using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Newtonsoft.Json;
using PT.Framework.ViewModelResolver.Infrastructure;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Mvc.Presentation;
using Sitecore.Syndication;
using Sitecore.Web.UI.WebControls;
using Version = Sitecore.Data.Version;

namespace PT.Framework.ViewModelResolver.Presentation
{
    public class MvcViewModel<T>
    {
        public MvcViewModel()
        {
            
        } 

        public MvcViewModel(Item dataItem)
        {
            DataItem = dataItem;
        }

        public virtual void Initialize(Item dataItem)
        {
            
        }

        [JsonIgnore]
        public string ItemId => DataItem != null ? DataItem.ID.ToString() : "";

        [JsonIgnore]
        public string ItemUrl => DataItem != null ? LinkManager.GetItemUrl(DataItem).Replace(" ", "%20") : "WARN-dataitem-not-set";

        [JsonIgnore]
        public string ItemName => DataItem != null ? DataItem.DisplayName : "WARN-dataitem-not-set";

        [JsonIgnore]
        public Item DataItem { get; set; }

        public HtmlString Field(Expression<Func<T, object>> expression)
        {
            //{CA74BF65-4AF1-41DF-B004-474E705E5728}
            if (DataItem == null)
            {
                return new HtmlString("");
            }
            var propId = GetPropertyId(expression);
            var field = FieldRenderer.Render(DataItem, propId.ToString());
            if (field != null)
            {
                return new HtmlString(field);
            }
            return new HtmlString(string.Empty);
        }

        public TP GetItemReference<TP>(Expression<Func<T, object>> expression) where TP : MvcViewModel<TP>, new()
        {
            if (DataItem == null)
                return new TP();

            var propId = GetPropertyId(expression);
            ReferenceField dropDownSelectedItem = DataItem.Fields[propId];
            if (dropDownSelectedItem == null)
            {
                return new TP();
            }
            
            var targetItem = dropDownSelectedItem.TargetItem;
            return new TP { DataItem = targetItem};
        }

        public Sitecore.Data.Fields.LinkField LinkField(Expression<Func<T, object>> expression)
        {
            if (DataItem == null)
            {
                //TODO fix scenarie hvor item ikke eksisterer
                Log.Error("Item not found", this);
                return null;
            }
            var propId = GetPropertyId(expression);
            LinkField linkField = DataItem.Fields[propId];
            return linkField;
        }
        public string LinkField(Expression<Func<T, object>> expression, bool giveMeOnlyUrl)
        {
            if (DataItem == null)
            {
                //TODO fix scenarie hvor item ikke eksisterer
                Log.Error("Item not found", this);
                return "";
            }
            var propId = GetPropertyId(expression);
            LinkField linkField = DataItem.Fields[propId];

            if (linkField == null)
                return "";

            if (linkField.IsInternal && linkField.TargetItem != null)
            {
                if (linkField.TargetItem == null)
                    return "";
                return LinkManager.GetItemUrl(linkField.TargetItem);
            }

            return linkField.Url;
        }

        public string FileField(Expression<Func<T, object>> expression)
        {
            if (DataItem == null)
            {
                //TODO fix scenarie hvor item ikke eksisterer
                Log.Error("Item not found", this);
                return null;
            }
            var propId = GetPropertyId(expression);
            FileField fileField = DataItem.Fields[propId];
            if (fileField.MediaItem != null)
            {
                return Sitecore.Resources.Media.MediaManager.GetMediaUrl(fileField.MediaItem);
            }

            return string.Empty;
        }

        public string GetImageUrl(Expression<Func<T, object>> expression)
        {
            if (DataItem == null)
            {
                //TODO fix scenarie hvor item ikke eksisterer
                Log.Error("Item not found", this);
                return null;
            }
            var propId = GetPropertyId(expression);

            var imageUrl = string.Empty;
            ImageField imageField = DataItem.Fields[propId];

            if (imageField?.MediaItem == null) return imageUrl;

            var image = new MediaItem(imageField.MediaItem);

            imageUrl = Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(image));

            return imageUrl;
        }

        public IEnumerable<TK> List<TK>(Expression<Func<T, object>>  expression) where TK : MvcViewModel<TK>, new()
        {
            if(DataItem == null)
                return new List<TK>();

            var propId = GetPropertyId(expression);

            var listField = DataItem.Fields[propId];

            var database = Sitecore.Context.Database;

            var listItemIds = listField.Value.Split('|');

            if(string.IsNullOrWhiteSpace(listField.Value) || listItemIds.Length < 1)
                return new List<TK>();

            var listItems  = listItemIds.Select(p => new TK{DataItem = database.GetItem(p)});

            return listItems;
        }

        protected ID GetPropertyId(Expression<Func<T, object>> expression)
        {
            string propName = "";
            if (expression.Body is MemberExpression)
                propName = ((MemberExpression)expression.Body).Member.Name;

            var fullyQualifiedName = typeof(T).FullName;

            var key = string.Format("{0}.{1}", fullyQualifiedName, propName);


            ID itemId   ;
            if (MappingTable.Instance.Map.TryGetValue(key, out itemId))
                return itemId;

            Log.Error(string.Format("Kunne ikke finde item id for '{0}'", key), this);
            return new ID("");
        }

        
    }
}