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
using System.Security.Permissions; //SecurityAction
using System.Collections; //IEnumerator
using System.Web.UI; //ParseChildren

namespace Memba.WebControls
{
    /// <summary>
    /// ListItem class
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [ParseChildren(false)] //we serialize ourselves
    public class ImageListItem : IEquatable<ImageListItem>
    {
        #region Private Members
        private string _Key;
        private string _ImageUrl;
        private string _Text;
        private string _Tooltip;
        private string _Tag;
        #endregion

        #region Property Accessors
        public string Key
        {
            get { return _Key; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                _Key = value;
            }
        }
        public string ImageUrl
        {
            get { return _ImageUrl; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                _ImageUrl = value;
            }
        }
        public string Text
        {
            get { return _Text; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                _Text = value;
            }
        }
        public string Tooltip
        {
            get { return _Tooltip; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                _Tooltip = value;
            }
        }
        public string Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Parameterless constructor for JSON serialization
        /// </summary>
        //Parameterless constructor exposed to System.Web.Extensions.dll for JSON serialization through StrongNameIdentityPermissionAttribute
        //use sn.exe -Tp System.Web.Extensions.dll to read the public key
        //[System.Security.Permissions.StrongNameIdentityPermissionAttribute(System.Security.Permissions.SecurityAction.Demand, PublicKey =
        //   "0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
        //Unfortunately, this does not work and the constructor really has to be public
        public ImageListItem()
        {
            //Dummy values that need to be changed in case someone uses it
            _Key = Guid.Empty.ToString();
            _ImageUrl = "spacer.gif";
            _Text = "text";
            _Tooltip = "tooltip";
        }
        /// <summary>
        /// Other public constructors
        /// </summary>
        /// <param name="key"></param>
        /// <param name="imageUrl"></param>
        /// <param name="text"></param>
        public ImageListItem(string key, string imageUrl, string text) : this(key, imageUrl, text, text, null) { }
        public ImageListItem(string key, string imageUrl, string text, string tooltip) : this(key, imageUrl, text, tooltip, null) { }
        public ImageListItem(string key, string imageUrl, string text, string tooltip, string tag)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (String.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException("imageUrl");
            if (String.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");
            if (String.IsNullOrEmpty(tooltip))
                throw new ArgumentNullException("tooltip");
            //Tag can be null

            _Key = key;
            _ImageUrl = imageUrl;
            _Text = text;
            _Tooltip = tooltip;
            _Tag = tag;
        }
        #endregion

        #region IEquatable<ImageListItem> Members
        public bool Equals(ImageListItem other)
        {
            return ((this.Key == other.Key)
                || ((this.Text == other.Text) && (this.Tag == other.Tag)));
        }
        #endregion
    }
    /// <summary>
    /// ListItemCollection class
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [ParseChildren(false)] //we serialize ourselves
    public class ImageListItemCollection : ICollection<ImageListItem>, IList<ImageListItem>
    {
        private List<ImageListItem> _ImageListItems;

        #region Constructor
        public ImageListItemCollection()
        {
            _ImageListItems = new List<ImageListItem>();
        }
        public ImageListItemCollection(IEnumerable<ImageListItem> collection)
        {
            _ImageListItems = new List<ImageListItem>(collection);
        }
        #endregion

        #region ICollection<ListItem> Members
        public void Add(ImageListItem item)
        {
            if (!_ImageListItems.Contains(item))
                _ImageListItems.Add(item);
        }

        public void Clear()
        {
            _ImageListItems.Clear();
        }

        public bool Contains(ImageListItem item)
        {
            return _ImageListItems.Contains(item);
        }

        public void CopyTo(ImageListItem[] array, int arrayIndex)
        {
            _ImageListItems.CopyTo(array, arrayIndex);
        }

        public int Count
        {
	        get { return _ImageListItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ImageListItem item)
        {
            return _ImageListItems.Remove(item);
        }
        #endregion

        #region IEnumerable<ListItem> Members
        public IEnumerator<ImageListItem>  GetEnumerator()
        {
            return _ImageListItems.GetEnumerator() as IEnumerator<ImageListItem>;
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _ImageListItems.GetEnumerator();
        }
        #endregion

        #region IList<ListItem> Members

        public int IndexOf(ImageListItem item)
        {
            return _ImageListItems.IndexOf(item);
        }

        public void Insert(int index, ImageListItem item)
        {
            if (!_ImageListItems.Contains(item))
                _ImageListItems.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _ImageListItems.RemoveAt(index);
        }

        public ImageListItem this[int index]
        {
            get
            {
                return _ImageListItems[index];
            }
            set
            {
                if (!_ImageListItems.Contains(value))
                    _ImageListItems[index] = value;
            }
        }

        #endregion

        #region Other methods
        public void AddRange(IEnumerable<ImageListItem> collection)
        {
            _ImageListItems.AddRange(collection);
        }

        public ImageListItem[] ToArray()
        {
            return _ImageListItems.ToArray();
        }
        #endregion
    }
}
