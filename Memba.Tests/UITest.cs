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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WatiN.Core; //Requires referencing the WatiN assembly available at http://watin.sourceforge.net

namespace Memba.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UITest
    {
        #region Private Members
        private static string MESSAGE = "Hello\nPlease find attached a brochure.\nBest regards,\nJoe";
        #endregion

        #region Constructor
        public UITest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        #endregion

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Text Fixtures
        [TestMethod]
        public void UIDropTest()
        {
            // create a new Internet Explorer instance pointing to the ASP.NET Development Server
            using (IE ie = new IE(Constants.DevWebSite + "drop.aspx"))
            {
                // Maximize the IE window
                ie.ShowWindow(NativeMethods.WindowShowStyle.Maximize);
                
                // Fill in the form
                ie.TextField(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SenderTextBox")).TypeText(Constants.Sender);
                ie.SelectList(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_RecipientDropDownList")).Select(Constants.Recipient);
                ie.TextField(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MessageTextBox")).TypeText(MESSAGE);
                WatiN.Core.FileUpload fUp;
                fUp = ie.FileUpload(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MultiUpload_Input_0"));
                fUp.Set(Constants.File0);
                fUp = ie.FileUpload(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MultiUpload_Input_1"));
                fUp.Set(Constants.File1);
                fUp = ie.FileUpload(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MultiUpload_Input_2"));
                fUp.Set(Constants.File2);
                ie.CheckBox(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_AcceptTermsCheckBox")).Checked = true;
                
                //Click send and wait until end of post
                if (ie.Button(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SendButton")).Enabled)
                {
                    ie.Button(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SendButton")).ClickNoWait();
                    SimpleTimer t = new SimpleTimer(60 * 60);
                    Span s;
                    do
                    {
                        s = ie.Span(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_FinalReportLabel"));
                        if (s.Exists)
                        {
                            goto EXIT;
                        }
                        System.Threading.Thread.Sleep(1000);
                    } while (!t.Elapsed);

                    throw new WatiN.Core.Exceptions.TimeoutException(string.Format("waiting {0} seconds for element to show up.", 60 * 60));

                EXIT:
                    Div d = ie.Div(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SendFinalPanel"));
                    Assert.AreEqual(true, ((d.Exists) && (d.Links.Length > 0) && d.Links[0].Url.StartsWith(Constants.DevWebSite, StringComparison.InvariantCultureIgnoreCase)));
                }
                else
                {
                    Assert.IsTrue(false, "Send button is disabled because of incorrect data");
                }
            }
        }

        [TestMethod]
        public void UISendTest()
        {
            // create a new Internet Explorer instance pointing to the ASP.NET Development Server
            using (IE ie = new IE(Constants.DevWebSite + "send.aspx"))
            {
                // Maximize the IE window
                ie.ShowWindow(NativeMethods.WindowShowStyle.Maximize);

                // Fill in the form
                ie.SelectList(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SenderDropDownList")).Select(Constants.Sender);
                ie.TextField(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_RecipientTextBox")).TypeText(Constants.Recipient);
                ie.TextField(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MessageTextBox")).TypeText(MESSAGE);
                WatiN.Core.FileUpload fUp;
                fUp = ie.FileUpload(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MultiUpload_Input_0"));
                fUp.Set(Constants.File0);
                fUp = ie.FileUpload(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MultiUpload_Input_1"));
                fUp.Set(Constants.File1);
                fUp = ie.FileUpload(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_MultiUpload_Input_2"));
                fUp.Set(Constants.File2);
                ie.TextField(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SecurityCodeTextBox")).TypeText(Constants.SecurityCode);
                //Trigger the key up event to execute the web service and allow some time for callback
                ie.TextField(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SecurityCodeTextBox")).KeyUp();
                System.Threading.Thread.Sleep(1000);

                //Click send and wait until end of post
                if (ie.Button(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SendButton")).Enabled)
                {
                    ie.Button(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SendButton")).ClickNoWait();
                    SimpleTimer t = new SimpleTimer(60 * 60);
                    Span s;
                    do
                    {
                        s = ie.Span(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_FinalReportLabel"));
                        if (s.Exists)
                        {
                            goto EXIT;
                        }
                        System.Threading.Thread.Sleep(1000);
                    } while (!t.Elapsed);

                    throw new WatiN.Core.Exceptions.TimeoutException(string.Format("waiting {0} seconds for element to show up.", 60 * 60));

                EXIT:
                    Div d = ie.Div(Find.ById("ctl00_PageContentPlaceHolder_QuickMessageControl_SendFinalPanel"));
                    Assert.AreEqual(true, ((d.Exists) && (d.Links.Length > 0) && d.Links[0].Url.StartsWith(Constants.DevWebSite, StringComparison.InvariantCultureIgnoreCase)));
                }
                else
                {
                    Assert.IsTrue(false, "Send button is disabled because of incorrect data");
                }
            }
        }
        #endregion
    }
}
