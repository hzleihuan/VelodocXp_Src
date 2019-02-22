/*
 * Copyright © 2005-2008 Memba SA. All rights reserved.
 * 
 * This file is part of Velodoc XP Edition.
 * 
 * Velodoc XP Edition is open-source software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 2 of the License, or (at your option) any later version.
 * 
 * Velodoc XP Edition is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with Velodoc XP Edition.
 * If not, see <http://www.gnu.org/licenses/>.
 * 
 * For more information, please contact Memba at <http://www.memba.com>.
 * You can find more information about Velodoc at <http://www.velodoc.com> and <http://www.velodoc.com/help>.
 * You can try Velodoc online at <http://www.velodoc.net>.
 *
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web; //AspNetHostingPermission, AspNetHostingPermissionLevel
using System.Web.UI; //Control, IScriptControl, WebResource, Themeable
using System.Web.UI.Design; //UrlEditor
using System.Web.UI.WebControls;
using System.Drawing.Design; //UITypeEditor
using System.ComponentModel; //Category, Description, DefaultProperty
using System.Security.Permissions; //SecurityAction
using System.Web.Script.Serialization; //JavaScriptSerializer

//An important question was whether to derive this control from System.Web.UI.WebControls.ListControl
//like System.Web.UI.WebControls.BulletedList or System.Web.UI.WebControls.DropDownList to benefit
//from state management and data binding.
//Actually, state management cannot be read from client side, so we need to use a hidden field to keep track of data.
//Besides the attributes of a ListItem object are not perfectly adapted as they are designed to be rendered to an HtmlTextWriter  

//TODO: Future development: implement selection (single selection of multi-selection)
//TODO: Future development: implement an Edit image button

//See: http://www.asp.net/AJAX/Documentation/Live/tutorials/IScriptControlTutorial1.aspx

[assembly: WebResource("Memba.WebControls.JavaScript.ImageList.js", "text/javascript")]
[assembly: WebResource("Memba.WebControls.Styles.remove.gif", "image/gif")]

namespace Memba.WebControls
{   
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultProperty("Text")]
    [Themeable(true)]
    //[Designer("...")]
    //[ParseChildren(true)]
    //[PersistChildren(false)]
    public class ImageList : Control, IScriptControl
    {
        #region Private Members
        private ImageListItemCollection _ImageListItemCollection;
        private string _RemoveTooltip;
        private string _RemoveImage;
        private string _CssClass;
        private string _ItemCssClass;
        private string _ItemHoverCssClass;
        private string _ImageCssClass;
        private string _TextCssClass;
        private string _RemoveCssClass;
        private int _LinesOfText;
        private bool _Enabled;

        //Not passed to client side
        private Unit _Width;
        private Unit _Height;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ImageList()
            : base()
        {
            _LinesOfText = 0; //0 means no truncation
            _Enabled = true;
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Collection of ListItems
        /// </summary>
        [Browsable(false)]
        [Themeable(false)]
        public ImageListItemCollection ImageListItemCollection
        {
            get
            {
                if (_ImageListItemCollection == null)
                    _ImageListItemCollection = new ImageListItemCollection();
                return _ImageListItemCollection;
            }
        }
        /// <summary>
        /// Tooltip for the remove image button
        /// </summary>
        [Category("Appearance")]
        [Description("Tooltip for the remove image button")]
        [Themeable(false)]
        [Localizable(true)]
        [DefaultValue("")]
        public string RemoveTooltip
        {
            //ViewState persists between postbacks, but the more ViewState added, the more processing and bandwidth required
            get { return _RemoveTooltip; } // ViewState["RemoveTooltip"] == null ? "" : ViewState["RemoveTooltip"].ToString(); }
            set { _RemoveTooltip = value; } // ViewState["RemoveTooltip"] = value; }
        }
        /// <summary>
        /// Url for the remove image button
        /// </summary>
        [Category("Appearance")]
        [Description("Url for the remove image button")]
        [DefaultValue("")]
        [Editor(typeof(UrlEditor), typeof(UITypeEditor))]
        public string RemoveImage
        {
            get { return _RemoveImage; } //ViewState["RemoveImage"] == null ? "" : ViewState["RemoveImage"].ToString(); }
            set { _RemoveImage = value; } //ViewState["RemoveImage"] = value; }
        }
        /// <summary>
        /// CSS class applied to the ImageList container div
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the ImageList container div")]
        [DefaultValue("")]
        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }
        /// <summary>
        /// CSS class applied to an Item div
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to an Item div")]
        [DefaultValue("")]
        public string ItemCssClass
        {
            get { return _ItemCssClass; }
            set { _ItemCssClass = value; }
        }
        /// <summary>
        /// CSS class applied when hovering an Item div
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied when hovering an Item div")]
        [DefaultValue("")]
        public string ItemHoverCssClass
        {
            get { return _ItemHoverCssClass; }
            set { _ItemHoverCssClass = value; }
        }
        /// <summary>
        /// CSS class applied to an Item's image
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to an Item's image")]
        [DefaultValue("")]
        public string ImageCssClass
        {
            get { return _ImageCssClass; }
            set { _ImageCssClass = value; }
        }
        /// <summary>
        /// CSS class applied to an Item's text
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to an Item's text")]
        [DefaultValue("")]
        public string TextCssClass
        {
            get { return _TextCssClass; }
            set { _TextCssClass = value; }
        }
        /// <summary>
        /// CSS class applied to the remove icon
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the remove icon")]
        [DefaultValue("")]
        public string RemoveCssClass
        {
            get { return _RemoveCssClass; }
            set { _RemoveCssClass = value; }
        }
        /// <summary>
        /// Number of characters after which to truncate text (not tooltips)
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the remove icon")]
        [DefaultValue(0)]
        public int LinesOfText
        {
            get { return _LinesOfText; }
            set { _LinesOfText = value; }
        }
        /// <summary>
        /// Whether to enable/disable the mouseover/out and remove events
        /// </summary>
        [Category("Appearance")]
        [Description("Whether to enable/disable the mouseover/out and remove events")]
        [DefaultValue(true)]
        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        /// <summary>
        /// Width of the ListItem control
        /// </summary>
        [Category("Layout")]
        [Description("Width of the ListItem control")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        /// <summary>
        /// Height of the ListItem control
        /// </summary>
        [Category("Layout")]
        [Description("Height of the ListItem control")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Height
        {
            get { return _Height; }
            set { _Height = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Browsable(true)]
        public override bool EnableTheming
        {
            get
            {
                return base.EnableTheming;
            }
            set
            {
                base.EnableTheming = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Browsable(true)]
        public override string SkinID
        {
            get
            {
                return base.SkinID;
            }
            set
            {
                base.SkinID = value;
            }
        }
        #endregion

        #region Control Events
        /// <summary>
        /// OnLoad override that runs only when serving the original page
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnLoad(EventArgs e)
        {
            //Hidden field to track data
            string sHiddenFieldName = this.ClientID + "_HiddenField";
            if (Page.Request.Form[sHiddenFieldName] != null)
            {
                string sValue = Page.Request.Form[sHiddenFieldName];
                _ImageListItemCollection = DeserializeListItems(sValue);
            }
            base.OnLoad(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if (!this.DesignMode)
            {
                // Test for ScriptManager and register if it exists
                ScriptManager objScriptManager = ScriptManager.GetCurrent(Page);

                if (objScriptManager == null)
                    throw new HttpException("A ScriptManager control must exist on the current page.");

                //Register control with ScriptManager
                objScriptManager.RegisterScriptControl(this);
            }

            base.OnPreRender(e); //was on top of the method

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            /*
            <DIV class="cssAttachmentList2" id="divFileboxSelectionList">
	            <DIV class="cssAttachmentListItem2" id="divItemtblFileboxSelectionList9898708b-dc7b-4e6b-b48b-17e473cbb0ad">
		            <IMG title="100mb (100.0 MB)" height="32" alt="100mb (100.0 MB)" src="http://localhost:59668/Hosted/App_Images/32x32/file.gif" width="32" border="0" />
                    <DIV class="cssAttachmentListText2">100mb...</DIV>
                    <IMG class="cssAttachmentListDeleteButton2" title="Remove from selection" style="DISPLAY: none" height="16" alt="Remove from selection" src="http://localhost:59668/Hosted/App_Images/16x16/delete2.gif" width="16" border="0" />
	            </DIV>
	            <DIV class="cssAttachmentListItemHover2" id="divItemtblFileboxSelectionList36927529-1c11-4f65-bef7-ee753b01ddaa">
		            <IMG title="Movies (160.2 KB)" height="32" alt="Movies (160.2 KB)" src="http://localhost:59668/Hosted/App_Images/32x32/ext-zip.gif" width="32" border="0" />
                    <DIV class="cssAttachmentListText2">Movies...</DIV>
                    <IMG class="cssAttachmentListDeleteButton2" title="Remove from selection" height="16" alt="Remove from selection" src="http://localhost:59668/Hosted/App_Images/16x16/delete2.gif" width="16" border="0" />
	            </DIV>
            </DIV>
            */

            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "auto");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative"); //Otherwise overflow auto does not work in IE7
            if (!_Width.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, _Width.ToString());
            if (!_Height.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, _Height.ToString());
            if (!String.IsNullOrEmpty(_CssClass))
                writer.AddAttribute(HtmlTextWriterAttribute.Class, _CssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Inner");
            //writer.RenderBeginTag(HtmlTextWriterTag.Div);

            /*
            if (this.DesignMode) //Only in design mode, otherwise items are created on the client from the values in the hidden field
            {
                //Add Items
                foreach (ImageListItem objImageListItem in _ImageListItemCollection)
                {
                    //Item Div
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Item_" + objImageListItem.Key);
                    if (!String.IsNullOrEmpty(_ItemCssClass))
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, _ItemCssClass);
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);

                    //Item Image
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Image_" + objImageListItem.Key);
                    if (!String.IsNullOrEmpty(_ImageCssClass))
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, _ImageCssClass);
                    else
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, objImageListItem.Tooltip);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, objImageListItem.Tooltip);
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, objImageListItem.ImageUrl);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag(); //Item Image

                    //Item text
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Text_" + objImageListItem.Key);
                    if (!String.IsNullOrEmpty(_TextCssClass))
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, _TextCssClass);
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write(objImageListItem.Text);
                    writer.RenderEndTag(); //Item text

                    //Remove icon
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Remove_" + objImageListItem.Key);
                    if (!String.IsNullOrEmpty(_RemoveCssClass))
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, _RemoveCssClass);
                    else
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, _RemoveTooltip);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, _RemoveTooltip);
                    string sRemoveImage;
                    if (String.IsNullOrEmpty(this.RemoveImage))
                        sRemoveImage = Page.ClientScript.GetWebResourceUrl(typeof(ImageList), "Memba.WebControls.Styles.remove.gif");
                    else
                        sRemoveImage = this.ResolveClientUrl(this.RemoveImage);
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, sRemoveImage);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag(); //Item Image

                    writer.RenderEndTag(); //Item Div
                }
            }
            */ 

            writer.RenderEndTag(); //Div          

            //Add hidden field
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_HiddenField");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ClientID + "_HiddenField");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, SerializeListItems(_ImageListItemCollection));
            writer.RenderBeginTag(HtmlTextWriterTag.Input);

            //writer.RenderEndTag(); //Div          

            //Register the Ajax extension properties of the control
            if (!this.DesignMode)
            {
                ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
            }

            base.Render(writer); //Was on top of method
        }
        #endregion

        #region ListItem Serialization
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listItemCollection"></param>
        /// <returns></returns>
        protected string SerializeListItems(ImageListItemCollection listItemCollection)
        {
            string sRet;
            if ((listItemCollection == null) || (listItemCollection.Count < 1))
                sRet = String.Empty;
            else
            {
                JavaScriptSerializer objSerializer = new JavaScriptSerializer();
                ImageListItem[] arrListItems = listItemCollection.ToArray();
                sRet = objSerializer.Serialize(arrListItems);
            }
            return sRet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected ImageListItemCollection DeserializeListItems(string stream)
        {
            ImageListItemCollection colRet;
            if (String.IsNullOrEmpty(stream))
                colRet = new ImageListItemCollection();
            else
            {
                JavaScriptSerializer objSerializer = new JavaScriptSerializer();
                ImageListItem[] arrListItems = objSerializer.Deserialize<ImageListItem[]>(stream);
                colRet = new ImageListItemCollection(arrListItems);
            }
            return colRet;
        }
        #endregion

        #region IScriptControl interface implementation
        /// <summary>
        /// Protected GetScriptReferences which should be overriden n derived classes
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(
                this.Page.ClientScript.GetWebResourceUrl(
                    this.GetType(),
                    "Memba.WebControls.JavaScript.ImageList.js"));
        }
        /// <summary>
        /// Protected GetScriptDescriptors which should be overriden n derived classes
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor objScriptControlDescriptor = new ScriptControlDescriptor("Memba.WebControls.ImageList", ClientID);
            objScriptControlDescriptor.AddProperty("removeTooltip", this.RemoveTooltip);
            string sRemoveImage;
            if (String.IsNullOrEmpty(this.RemoveImage))
                sRemoveImage = Page.ClientScript.GetWebResourceUrl(typeof(ImageList), "Memba.WebControls.Styles.remove.gif");
            else
                sRemoveImage = this.ResolveClientUrl(this.RemoveImage);
            objScriptControlDescriptor.AddProperty("removeImage", sRemoveImage);
            objScriptControlDescriptor.AddProperty("cssClass", this.CssClass);
            objScriptControlDescriptor.AddProperty("itemCssClass", this.ItemCssClass);
            objScriptControlDescriptor.AddProperty("itemHoverCssClass", this.ItemHoverCssClass);
            objScriptControlDescriptor.AddProperty("imageCssClass", this.ImageCssClass);
            objScriptControlDescriptor.AddProperty("textCssClass", this.TextCssClass);
            objScriptControlDescriptor.AddProperty("removeCssClass", this.RemoveCssClass);
            objScriptControlDescriptor.AddProperty("linesOfText", this.LinesOfText);
            objScriptControlDescriptor.AddProperty("enabled", this.Enabled);

            //No need to maintain the remove image Url client-side considering the above
            //objScriptControlDescriptor.AddProperty("removeUrl", Page.ClientScript.GetWebResourceUrl(typeof(ImageList), "Memba.WebControls.Styles.remove.gif"));

            yield return objScriptControlDescriptor;
        }
        /// <summary>
        /// Public interface for GetScriptReferences
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences()
        {
            return GetScriptReferences();
        }
        /// <summary>
        /// Public interface for GetScriptDescriptors
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors()
        {
            return GetScriptDescriptors();
        }
        #endregion
    }
}
