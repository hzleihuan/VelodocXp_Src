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

[assembly: WebResource("Memba.WebControls.JavaScript.MultiUpload.js", "text/javascript")]

namespace Memba.WebControls
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultProperty("Text")]
    [Themeable(true)]
    //[Designer("...")]
    //[ParseChildren(true)]
    //[PersistChildren(false)]
    public class MultiUpload : Control, IScriptControl
    {
        #region Private Members
        private string _CssClass;
        private Unit _Height;
        private Unit _Width;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public MultiUpload()
            : base()
        {
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Text displayed in the MultiUpload Div
        /// </summary>
        [Category("Appearance")]
        [Description("Text displayed in the MultiUpload Div")]
        [Themeable(false)]
        [Localizable(true)]
        [DefaultValue("")]
        public string Text
        {
            get { return ViewState["Text"] == null ? "" : ViewState["Text"].ToString(); }
            set { ViewState["Text"] = value; }
        }
        /// <summary>
        /// CSS class applied to the MultiUpload Div
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the MultiUpload Div")]
        [DefaultValue("")]
        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value; }
        }
        /// <summary>
        /// Width of the MultiUpload Div
        /// </summary>
        [Category("Layout")]
        [Description("Width of the MultiUpload Div")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        /// <summary>
        /// Height of the MultiUpload Div
        /// </summary>
        [Category("Layout")]
        [Description("Height of the MultiUpload Div")]
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
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
            if (!String.IsNullOrEmpty(_CssClass))
                writer.AddAttribute(HtmlTextWriterAttribute.Class, _CssClass);
            if (!_Width.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, _Width.ToString());
            if (!_Height.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, _Height.ToString());
            //writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(this.Text);
            writer.RenderEndTag(); //A

            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Inputs");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            //but Safari is supposed to prefer the following to display:none
            //writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
            //writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "-1000px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Input_0");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ClientID + "_Input_0");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "file");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag(); //Input

            writer.RenderEndTag(); //Div

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
                    "Memba.WebControls.JavaScript.MultiUpload.js"));
        }
        /// <summary>
        /// Protected GetScriptDescriptors which should be overriden n derived classes
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor objScriptControlDescriptor = new ScriptControlDescriptor("Memba.WebControls.MultiUpload", ClientID);
            objScriptControlDescriptor.AddProperty("text", this.Text);
            objScriptControlDescriptor.AddProperty("cssClass", this.CssClass);

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
