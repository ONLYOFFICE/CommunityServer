//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.HtmlControls;
//using System.ComponentModel;
//using System.ComponentModel.Design;
//using System.Web.UI.Design;
//using System.Reflection;

//namespace AjaxPro.Web.UI
//{
//    [ToolboxData("<{0}:AjaxAutoComplete runat=server>")]
//    [Designer(typeof(HtmlSelectDesigner))]
//    public class AjaxHtmlSelect : System.Web.UI.WebControls.TextBox
//    {
//        public AjaxHtmlSelect()
//            : base()
//        {
//        }

//        [CategoryAttribute("Misc")]
//        [DescriptionAttribute("")]
//        public string LoadingMessage
//        {
//            get
//            {
//                return m_LoadingMessage;
//            }
//            set
//            {
//                m_LoadingMessage = value;
//            }
//        }

//    }

//    public class HtmlSelectDesigner : ControlDesigner
//    {
//        private DesignerActionListCollection al = null;

//        public override DesignerActionListCollection ActionLists
//        {
//            get
//            {
//                if (al == null)
//                {
//                    al = new DesignerActionListCollection();
//                    al.AddRange(base.ActionLists);

//                    // Add a custom DesignerActionList
//                    al.Add(new HtmlSelectDesignerActionList(this));
//                }
//                return al;

//            }
//        }
//    }

//    public class HtmlSelectDesignerActionList : DesignerActionList
//    {
//        HtmlSelectDesigner m_parent;

//        public HtmlSelectDesignerActionList(HtmlSelectDesigner parent)
//            : base(parent.Component)
//        {
//            m_parent = parent;
//        }

//        public override DesignerActionItemCollection GetSortedActionItems()
//        {
//            // Create list to store designer action items
//            DesignerActionItemCollection actionItems = new DesignerActionItemCollection();

//            // Add Appearance category header text
//            //actionItems.Add(new DesignerActionHeaderItem("Misc"));

//            //// Add Appearance category descriptive label
//            //actionItems.Add(
//            //  new DesignerActionTextItem(
//            //    "Properties that affect how the User looks.",
//            //    "Misc"));

//            actionItems.Add(
//              new DesignerActionPropertyItem(
//                "LoadingMessage",
//                "Loading",
//                GetCategory(this.WebControl, "LoadingMessage"),
//                GetDescription(this.WebControl, "LoadingMessage")));

//            //actionItems.Add(
//            //    new DesignerActionMethodItem(
//            //    this,
//            //    "ClearValues",
//            //    "Clear Values",
//            //    true));

//            return actionItems;
//        }

//        #region Control Proxy Properties

//        public string LoadingMessage
//        {
//            get { return WebControl.LoadingMessage; }
//            set { SetProperty("LoadingMessage", value); }
//        }

//        public void ClearValues()
//        {

//        }

//        #endregion


//        private AjaxHtmlSelect WebControl
//        {
//            get
//            {
//                return (AjaxHtmlSelect)this.Component;
//            }
//        }

//        private string GetCategory(object source, string propertyName)
//        {
//            PropertyInfo property = source.GetType().GetProperty(propertyName);
//            CategoryAttribute attribute = (CategoryAttribute)property.GetCustomAttributes(typeof(CategoryAttribute), false)[0];
//            if (attribute == null)
//                return null;
//            return attribute.Category;
//        }

//        private string GetDescription(object source, string propertyName)
//        {
//            PropertyInfo property = source.GetType().GetProperty(propertyName);
//            DescriptionAttribute attribute = (DescriptionAttribute)property.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
//            if (attribute == null)
//                return null;
//            return attribute.Description;
//        }

//        // Helper method to safely set a component’s property
//        private void SetProperty(string propertyName, object value)
//        {
//            // Get property
//            PropertyDescriptor property = TypeDescriptor.GetProperties(this.WebControl)[propertyName];
//            // Set property value
//            property.SetValue(this.WebControl, value);
//        }
//    }
//}
