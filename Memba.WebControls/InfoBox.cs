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


//See: http://www.asp.net/AJAX/Documentation/Live/tutorials/IScriptControlTutorial1.aspx

[assembly: WebResource("Memba.WebControls.JavaScript.InfoBox.js", "text/javascript")]
[assembly: WebResource("Memba.WebControls.Styles.error.gif", "image/gif")]
[assembly: WebResource("Memba.WebControls.Styles.information.gif", "image/gif")]
[assembly: WebResource("Memba.WebControls.Styles.ok.gif", "image/gif")]
[assembly: WebResource("Memba.WebControls.Styles.warning.gif", "image/gif")]

namespace Memba.WebControls
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultProperty("Text")]
    [Themeable(true)]
    //[Designer("...")]
    //[ParseChildren(true)]
    //[PersistChildren(false)]
    public class InfoBox : Control, IScriptControl
    {
        #region Private Members
        private string _CssClass;
        private string _TextCssClass;
        private string _ImageCssClass;
        private Unit _Width;
        private Unit _Height;
        private Unit _ImageColWidth;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public InfoBox()
            : base()
        {
            _ImageColWidth = new Unit("35px");
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Text displayed in the InfoBox
        /// </summary>
        [Category("Appearance")]
        [Description("Text displayed in the InfoBox")]
        [Themeable(false)]
        [Localizable(true)]
        [DefaultValue("")]
        public string Text
        {
            get { return ViewState["Text"] == null ? "" : ViewState["Text"].ToString(); }
            set { ViewState["Text"] = value; }
        }
        /// <summary>
        /// Url of the image displayed in the InfoBox
        /// </summary>
        [Category("Appearance")]
        [Description("Url of the image displayed in the InfoBox")]
        [DefaultValue("")]
        [Editor(typeof(UrlEditor), typeof(UITypeEditor))]
        public string ImageUrl
        {
            get { return ViewState["ImageUrl"] == null ? "" : ViewState["ImageUrl"].ToString(); }
            set { ViewState["ImageUrl"] = value; }
        }
        /// <summary>
        /// Defines image displayed when no Url is specified
        /// </summary>
        [Category("Appearance")]
        [Description("Defines image displayed when no ImageUrl is specified")]
        [DefaultValue(InfoBoxType.Information)]
        public InfoBoxType Type
        {
            get { return ViewState["Type"] == null ? InfoBoxType.Information : (InfoBoxType)Enum.Parse(typeof(InfoBoxType), ViewState["Type"].ToString()); }
            set { ViewState["Type"] = value; }
        }
        /// <summary>
        /// CSS class applied to the InfoBox table container
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the InfoBox table container")]
        [DefaultValue("")]
        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }
        /// <summary>
        /// CSS class applied to the InfoBox text
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the InfoBox text")]
        [DefaultValue("")]
        public string TextCssClass
        {
            get { return _TextCssClass; }
            set { _TextCssClass = value; }
        }
        /// <summary>
        /// CSS class applied to the InfoBox image
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the InfoBox image")]
        [DefaultValue("")]
        public string ImageCssClass
        {
            get { return _ImageCssClass; }
            set { _ImageCssClass = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("Layout")]
        [Description("Width of the InfoBox table container")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        /// <summary>
        /// Height of the InfoBox table container
        /// </summary>
        [Category("Layout")]
        [Description("Height of the InfoBox table container")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Height
        {
            get { return _Height; }
            set { _Height = value; }
        }
        /// <summary>
        /// Width of the table column containg the image
        /// </summary>
        [Category("Layout")]
        [Description("Width of the table column containg the image")]
        [DefaultValue(typeof(Unit), "35px")]
        public Unit ImageColWidth
        {
            get { return _ImageColWidth; }
            set { _ImageColWidth = value; }
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
            <table border="0" cellpadding="5px" cellspacing="3px" style="width: 100%" class="cssInfoBoxTable">
                <tr>
                    <td style="width: 35px" valign="top"><img id="ctl00_InfoBox_InfoBoxImage" class="cssInfoBoxImage" src="/Hosted/App_Images/24x24/information.gif" alt="..." style="border-width:0px;" /></td>
                    <td valign="top"><span id="ctl00_InfoBox_InfoBoxLabel" class="cssInfoBoxLabel">...</span></td>
                </tr>
            </table>
            */

            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (!String.IsNullOrEmpty(_CssClass))
                writer.AddAttribute(HtmlTextWriterAttribute.Class, _CssClass);
            if (!_Width.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, _Width.ToString());
            if (!_Height.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, _Height.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, _ImageColWidth.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Image");
            if (!String.IsNullOrEmpty(_ImageCssClass))
                writer.AddAttribute(HtmlTextWriterAttribute.Class, _ImageCssClass);
            if (!String.IsNullOrEmpty(this.ImageUrl))
                writer.AddAttribute(HtmlTextWriterAttribute.Src, base.ResolveClientUrl(this.ImageUrl));
            else
            {
                switch (this.Type)
                {
                    case InfoBoxType.Error:
                        writer.AddAttribute(HtmlTextWriterAttribute.Src,
                            Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.error.gif"));
                        break;
                    case InfoBoxType.Information:
                        writer.AddAttribute(HtmlTextWriterAttribute.Src,
                            Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.information.gif"));
                        break;
                    case InfoBoxType.OK:
                        writer.AddAttribute(HtmlTextWriterAttribute.Src,
                            Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.ok.gif"));
                        break;
                    case InfoBoxType.Warning:
                        writer.AddAttribute(HtmlTextWriterAttribute.Src,
                            Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.warning.gif"));
                        break;
                    default:
                        break;
                }
            }
            //writer.AddAttribute(HtmlTextWriterAttribute.Width, "24px");
            //writer.AddAttribute(HtmlTextWriterAttribute.Height, "24px");
            //writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.Text);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag(); //img
            writer.RenderEndTag(); //td
            writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Label");
            if (!String.IsNullOrEmpty(_TextCssClass))
                writer.AddAttribute(HtmlTextWriterAttribute.Class, _TextCssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(this.Text);
            writer.RenderEndTag(); //span
            writer.RenderEndTag(); //td
            writer.RenderEndTag(); //tr
            writer.RenderEndTag(); //table

            //Register the Ajax extension properties of the control
            if (!this.DesignMode)
            {
                ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
            }

            base.Render(writer); //Was on top of method
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
                    "Memba.WebControls.JavaScript.InfoBox.js"));
        }
        /// <summary>
        /// Protected GetScriptDescriptors which should be overriden n derived classes
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor objScriptControlDescriptor = new ScriptControlDescriptor("Memba.WebControls.InfoBox", ClientID);
            objScriptControlDescriptor.AddProperty("text", this.Text);
            objScriptControlDescriptor.AddProperty("imageUrl", base.ResolveClientUrl(this.ImageUrl));
            objScriptControlDescriptor.AddProperty("type", this.Type);
            objScriptControlDescriptor.AddProperty("cssClass", this.CssClass);
            objScriptControlDescriptor.AddProperty("textCssClass", this.TextCssClass);
            objScriptControlDescriptor.AddProperty("imageCssClass", this.ImageCssClass);

            objScriptControlDescriptor.AddProperty("errorUrl", Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.error.gif"));
            objScriptControlDescriptor.AddProperty("informationUrl", Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.information.gif"));
            objScriptControlDescriptor.AddProperty("okUrl", Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.ok.gif"));
            objScriptControlDescriptor.AddProperty("warningUrl", Page.ClientScript.GetWebResourceUrl(typeof(InfoBox), "Memba.WebControls.Styles.warning.gif"));
            
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
