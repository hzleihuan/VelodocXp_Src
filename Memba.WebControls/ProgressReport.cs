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

[assembly: WebResource("Memba.WebControls.JavaScript.ProgressReport.js", "text/javascript")]

namespace Memba.WebControls
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [DefaultProperty("DefaultText")]
    [Themeable(true)]
    //[Designer("...")]
    //[ParseChildren(true)]
    //[PersistChildren(false)]
    public class ProgressReport : Control, IScriptControl
    {
        private const int DEFAULT_TIMEOUT = 3000;
        private const int DEFAULT_INTERVAL = 3000;

        #region Private Members
        private string _DefaultText;
        private string _TextFormat;
        private string _HandlerUrl;
        private int _Interval;
        private int _Timeout;
        private string _CssClass;
        private string _BarCssClass;
        private string _FillerCssClass;
        private string _TextCssClass;
        private Unit _Width;
        private Unit _Height;
        //private Unit _BarHeight;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ProgressReport()
            : base()
        {
            _Interval = DEFAULT_INTERVAL;
            _Timeout = DEFAULT_TIMEOUT;
            //_BarHeight = new Unit("10px");
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// The text displayed before the handler is called
        /// </summary>
        [Category("Appearance")]
        [Description("The text displayed before the handler is called")]
        [Themeable(false)]
        [Localizable(true)]
        [DefaultValue("")]
        public string DefaultText
        {
            get { return _DefaultText; }
            set { _DefaultText = value; }
        }
        /// <summary>
        /// A composite format string displayed below the progress bar
        /// </summary>
        [Category("Appearance")]
        [Description("A composite format string displayed below the progress bar")]
        [Themeable(false)]
        [Localizable(true)]
        [DefaultValue("")]
        public string TextFormat
        {
            get { return _TextFormat; }
            set { _TextFormat = value; }
        }
        /// <summary>
        /// Url of the handler returning upload progress status
        /// </summary>
        [Category("Behavior")]
        [Description("Url of the handler returning upload progress status")]
        [DefaultValue("")]
        [Editor(typeof(UrlEditor), typeof(UITypeEditor))]
        public string HandlerUrl
        {
            get { return _HandlerUrl; }
            set { _HandlerUrl = value.Trim(); }
        }
        /// <summary>
        /// Interval at which recurrently calling the handler
        /// </summary>
        [Category("Behavior")]
        [Description("Interval at which recurrently calling the handler")]
        [DefaultValue(ProgressReport.DEFAULT_INTERVAL)]
        public int Interval
        {
            get { return _Interval; }
            set { _Interval = value; }
        }
        /// <summary>
        /// Interval at which recurrently calling the handler
        /// </summary>
        [Category("Behavior")]
        [Description("Timeout of the handler call")]
        [DefaultValue(ProgressReport.DEFAULT_TIMEOUT)]
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }
        /// <summary>
        /// CSS class applied to the container div
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the container div")]
        [DefaultValue("")]
        public string CssClass
        {
            get { return _CssClass; }
            set { _CssClass = value.Trim(); }
        }
        /// <summary>
        /// CSS class applied to the outer div of the progress bar
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the outer div of the progress bar")]
        [DefaultValue("")]
        public string BarCssClass
        {
            get { return _BarCssClass; }
            set { _BarCssClass = value.Trim(); }
        }
        /// <summary>
        /// CSS class applied to the inner div of the progress bar
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the inner div of the progress bar")]
        [DefaultValue("")]
        public string FillerCssClass
        {
            get { return _FillerCssClass; }
            set { _FillerCssClass = value.Trim(); }
        }
        /// <summary>
        /// CSS class applied to the text displayed below the progress bar
        /// </summary>
        [Category("Appearance")]
        [Description("CSS class applied to the text displayed below the progress bar")]
        [DefaultValue("")]
        public string TextCssClass
        {
            get { return _TextCssClass; }
            set { _TextCssClass = value.Trim(); }
        }
        /// <summary>
        /// Width of the upload progress control
        /// </summary>
        [Category("Layout")]
        [Description("Width of the upload progress control")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Width
        {
            get { return _Width; }
            set { _Width = value; }
        }
        /// <summary>
        /// Height of the upload progress control
        /// </summary>
        [Category("Layout")]
        [Description("Height of the upload progress control")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Height
        {
            get { return _Height; }
            set { _Height = value; }
        }
        /// <summary>
        /// Height of the progress bar
        /// </summary>
        //[Category("Layout")]
        //[Description("Height of the progress bar")]
        //[DefaultValue(typeof(Unit), "10px")]
        //public Unit BarHeight
        //{
        //    get { return _BarHeight; }
        //    set { _BarHeight = value; }
        //}
        /// <summary>
        /// 
        /// </summary>
        [Browsable(true)]
        public override bool EnableTheming
        {
            get { return base.EnableTheming; }
            set { base.EnableTheming = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [Browsable(true)]
        public override string SkinID
        {
            get { return base.SkinID; }
            set { base.SkinID = value; }
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
            <div id="X"> 
                <div id="X_Bar">
                    <div id="X_Filler" style="width:0%"></div>
                </div>
                <div id="X_Text">bla bla</div>  
            </div> 
            */

            if (this.DesignMode || this.Visible)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
                if (!String.IsNullOrEmpty(_CssClass))
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, _CssClass);
                if (!_Width.IsEmpty)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, _Width.ToString());
                if (!_Height.IsEmpty)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, _Height.ToString());
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                //Progress bar
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Bar");
                if (!String.IsNullOrEmpty(_BarCssClass))
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, _BarCssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Filler");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "0%");
                if (!String.IsNullOrEmpty(_FillerCssClass))
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, _FillerCssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.RenderEndTag(); //div
                writer.RenderEndTag(); //div

                //Text
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID + "_Text");
                if (!String.IsNullOrEmpty(_TextCssClass))
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, _TextCssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(_DefaultText);
                writer.RenderEndTag(); //div

                writer.RenderEndTag(); //div
            }

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
                    "Memba.WebControls.JavaScript.ProgressReport.js"));
        }
        /// <summary>
        /// Protected GetScriptDescriptors which should be overriden n derived classes
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor objScriptControlDescriptor = new ScriptControlDescriptor("Memba.WebControls.ProgressReport", ClientID);
            objScriptControlDescriptor.AddProperty("defaultText", this.DefaultText);
            objScriptControlDescriptor.AddProperty("textFormat", this.TextFormat);
            objScriptControlDescriptor.AddProperty("handlerUrl", (String.IsNullOrEmpty(this.HandlerUrl) ? null : this.ResolveClientUrl(this.HandlerUrl)));
            objScriptControlDescriptor.AddProperty("interval", this.Interval);
            objScriptControlDescriptor.AddProperty("timeout", this.Timeout);
            objScriptControlDescriptor.AddProperty("cssClass", this.CssClass);
            objScriptControlDescriptor.AddProperty("barCssClass", this.BarCssClass);
            objScriptControlDescriptor.AddProperty("fillerCssClass", this.FillerCssClass);
            objScriptControlDescriptor.AddProperty("textCssClass", this.TextCssClass);

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
