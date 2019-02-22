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
Type.registerNamespace('Memba.WebControls');

//--------------------------------------------
//ImageListItem
//--------------------------------------------
Memba.WebControls.ImageListItem = function(key, imageUrl, text, tooltip, tag) {

    Sys.Debug.trace("ImageListItem: constructor");

    var e = Function._validateParams(arguments, [
        {name: "key", type: String},
        {name: "imageUrl", type: String},
        {name: "text", type: String},
        {name: "tooltip", type: String, mayBeNull: true, optional: true},
        {name: "tag", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    //IMPORTANT: The names of our properties need to match the corresponding names in the C# ImageListItem object
    //for JSON serialization to work both in C# and in JavaScript. This is the reason we have this.Key and not this._key.
    this.Key = key;
    this.ImageUrl = imageUrl;
    this.Text = text;
    if((typeof(tooltip) == "undefined") || (tooltip == null))
        this.Tooltip = text;
    else
        this.Tooltip = tooltip;
    this.Tag = tag;
}

Memba.WebControls.ImageListItem.prototype = {

    get_key: function() {
        return this.Key;
    },

    get_imageUrl: function() {
        return this.ImageUrl;
    },

    get_text: function() {
        return this.Text;
    },

    get_tooltip: function() {
        return this.Tooltip;
    },

    get_tag: function() {
        return this.Tag;
    }
}

Memba.WebControls.ImageListItem.registerClass('Memba.WebControls.ImageListItem', null);

//--------------------------------------------
//ImageList Control
//--------------------------------------------
Memba.WebControls.ImageList = function(element)
{
    Sys.Debug.trace("ImageList: constructor");
       
    this._ImageListItemCollection = null;
    this._RemoveTooltip = null;
    this._RemoveImage = null;
    this._CssClass = null;
    this._ItemCssClass = null;
    this._ItemHoverCssClass = null;   
    this._ImageCssClass = null;
    this._TextCssClass = null;
    this._RemoveCssClass = null;
    this._LinesOfText = null;
    this._Enabled = null;
    
    this._ControlDiv = null; //Maybe replaced with _InnerDiv inside control's external div for positioning requirements
    this._HiddenField = null;
    
    Memba.WebControls.ImageList.initializeBase(this, [element]);
}

Memba.WebControls.ImageList.prototype =
{
    get_items : function()
    {
        Sys.Debug.trace("ImageList: get_items");
        //return Array.clone(this._ImageListItemCollection);
        return this._ImageListItemCollection;
    },
    
    get_item: function(key) {
        
        Sys.Debug.trace("ImageList: get_item");
        
        var e = Function._validateParams(arguments, [
            {name: "key", type: String}
        ]);
        if (e) throw e;
        
        //Make sure we have a valid collection to search
        Sys.Debug.assert(this._ImageListItemCollection instanceof Array, "this._ImageListItemCollection should be an Array");
        
        var item = null;
        
        /*
        IMPORTANT
        We use iteratorX instead of i in all iterations for the following reason
        Take the following code calling get_item:
              items = myFirstImageList.get_items();
              for(i = 0; i < items.length; i++)
              {
                  Sys.debug.trace("value of i is " + i);
                  var item = mySecondImageList.get_item(items[i].get_key());
                  Sys.debug.trace("value of i is " + i);
              }
        
        Now suppose myFirstImageList has 1 item and mySecondImageList has 3 items which are different from the item in myFirstImageList.
        The iteration will start with i = 0 and the first trace will display a value of 0 
        Because items[i] is passed to get_item, get_item receives a reference to i.
        Because get_item uses i as its iterator, it will increament i which seraching for items[i].
        Because mySecondImageList has 3 items all different from items[0], the second trace will display a value of 3
        
        To avoid this we need to use a name for the iterator which has little chance to be used in client code, like iteratorX
        */
        
        for (iteratorX=0; iteratorX<this._ImageListItemCollection.length; iteratorX++)
        {
            if (this._ImageListItemCollection[iteratorX].get_key() == key)
            {
                item = this._ImageListItemCollection[iteratorX];
                break;
            }
        }
        
        return item;
    },
    
    find_item: function(searchedStr) {
        
        Sys.Debug.trace("ImageList: find_item");
        
        var e = Function._validateParams(arguments, [
            {name: "searchedStr", type: String}
        ]);
        if (e) throw e;
        
        //Make sure we have a valid collection to search
        Sys.Debug.assert(this._ImageListItemCollection instanceof Array, "this._ImageListItemCollection should be an Array");
        
        var aRet = new Array();
        
        for (iteratorX=0; iteratorX<this._ImageListItemCollection.length; iteratorX++)
        {
            if ((this._ImageListItemCollection[iteratorX].get_text() == searchedStr)
                || (this._ImageListItemCollection[iteratorX].get_tooltip() == searchedStr)
                || (this._ImageListItemCollection[iteratorX].get_tag() == searchedStr))
            {
                Array.add(aRet, this._ImageListItemCollection[iteratorX]);
            }
        }
        
        return aRet;
    },
    
    add_item: function(item) {

        Sys.Debug.trace("ImageList: add_item");

        var e = Function._validateParams(arguments, [
            {name: "item", type: Memba.WebControls.ImageListItem}
        ]);
        if (e) throw e;

        if (this.get_isInitialized())
        {
            //add to collection
            if (this._add2Collection(item))
            {
                //add to UI
                this._add2UI(item);
                //Raise the add event
                var eventHandler = this.get_events().getHandler("add");
                if (eventHandler)
                    eventHandler(this, item);
            }
        }
    },
    
    _indexOf: function(item) {
        
        Sys.Debug.trace("ImageList: _indexOf");

        var e = Function._validateParams(arguments, [
            {name: "item", type: Memba.WebControls.ImageListItem, mayBeNull: true}
        ]);
        if (e) throw e;

        //Make sure we have a valid collection to search
        Sys.Debug.assert(this._ImageListItemCollection instanceof Array, "this._ImageListItemCollection should be an Array");
        
        var index = -1;
        
        if ((item != null) && (item instanceof Memba.WebControls.ImageListItem))
        { 
            for (iteratorX=0; iteratorX<this._ImageListItemCollection.length; iteratorX++)
            {
                if ((this._ImageListItemCollection[iteratorX].get_key() == item.get_key())
                    || ((this._ImageListItemCollection[iteratorX].get_text() == item.get_text())
                        && (this._ImageListItemCollection[iteratorX].get_tag() == item.get_tag())))
                {
                    index = iteratorX;
                    break;
                }
            }
        }
        
        return index;
    },
    
    _add2Collection: function(item) {

        Sys.Debug.trace("ImageList: _add2Collection");

        var e = Function._validateParams(arguments, [
            {name: "item", type: Memba.WebControls.ImageListItem}
        ]);
        if (e) throw e;

        //Make sure we have a valid collection to add to
        Sys.Debug.assert(this._ImageListItemCollection instanceof Array, "this._ImageListItemCollection should be an Array");
        
        var index = this._indexOf(item);
        var addedRet = false;
        
        if(index < 0)
        {
            Array.add(this._ImageListItemCollection, item);
            this._HiddenField.value = Sys.Serialization.JavaScriptSerializer.serialize(this._ImageListItemCollection);
            Sys.Debug.trace("ImageList: new hidden field value is " + this._HiddenField.value);
            addedRet = true;
        }
        
        return addedRet;
    },
    
    _add2UI: function(item) {

        Sys.Debug.trace("ImageList: _add2UI");

        var e = Function._validateParams(arguments, [
            {name: "item", type: Memba.WebControls.ImageListItem}
        ]);
        if (e) throw e;

        var exists = $get(this.get_id() + "_Item_" + item.get_key());
        if (!exists)
        {            
            //Add DOM elements
            var itemDiv = document.createElement('div');
            itemDiv.setAttribute('id', this.get_id() + "_Item_" + item.get_key());
            if((this._ItemCssClass) && (this._ItemCssClass != ''))
                itemDiv.className = this._ItemCssClass;
            //itemDiv.setAttribute('class', this._ItemCssClass);
            Sys.Debug.trace("ImageList: item div creation for " + itemDiv.id);
            
            var imageCssClass = '';
            if((this._ImageCssClass) && (this._ImageCssClass != ''))
                imageCssClass = ' class="' + this._ImageCssClass + '"'; 
            var textCssClass = '';
            if((this._TextCssClass) && (this._TextCssClass != ''))
                textCssClass = ' class="' + this._TextCssClass + '"'; 
            var removeCssClass = '';
            if((this._RemoveCssClass) && (this._RemoveCssClass != ''))
                removeCssClass = ' class="' + this._RemoveCssClass + '"'; 
            itemDiv.innerHTML =
                '<img src="' + item.get_imageUrl() + '" alt="' + item.get_tooltip() + '" title="' + item.get_tooltip() + '"' + imageCssClass + ' />' +
	            '<div' + textCssClass + '></div>' +
	            '<img src="' + this._RemoveImage + '" alt="' + this._RemoveTooltip + '" title="' + this._RemoveTooltip + '"' + removeCssClass + ' style="display:none; cursor:pointer;" />';          
            Sys.Debug.trace("ImageList: item div content is " + itemDiv.innerHTML);
            this._ControlDiv.appendChild(itemDiv);
            this._truncWithEllipsis(itemDiv.childNodes[1], item.get_text());
            
            if (this._Enabled)
            {
                //add mouse over/out handlers
                var onmouseover = Function.createDelegate(this, function() { 
                    itemDiv.className = this._ItemHoverCssClass;
                    itemDiv.childNodes[2].style.display = "block";
                    //Sys.Debug.trace("ImageList: item div CSS class changed to " + itemDiv.className);
                });
                $addHandler(itemDiv, "mouseover", onmouseover);

                var onmouseout = Function.createDelegate(this, function() { 
                    itemDiv.className = this._ItemCssClass;
                    itemDiv.childNodes[2].style.display = "none";
                    //Sys.Debug.trace("ImageList: item div CSS class changed to " + itemDiv.className);
                });
                $addHandler(itemDiv, "mouseout", onmouseout);
                            
                //add remove Handler
                var remove = Function.createDelegate(this, function() { 
                    var key = itemDiv.id.substr((this.get_id() + "_Item_").length);
                    Sys.Debug.trace("ImageList: remove item with key " + key);
                    this.remove_item(key);
                });
                $addHandler(itemDiv.childNodes[2], "click", remove);
            }           
        }
    },
    
    _buildUI: function() { //Call _clearUI before in most cases
    
        Sys.Debug.trace("ImageList: _clearUI");
        
        //Make sure we have a valid collection to add to
        Sys.Debug.assert(this._ImageListItemCollection instanceof Array, "this._ImageListItemCollection should be an Array");
    
        for(iteratorX=0; iteratorX<this._ImageListItemCollection.length; iteratorX++)
        {
            this._add2UI(this._ImageListItemCollection[iteratorX]);
        }
    },
    
    remove_item: function(key) {
    
        Sys.Debug.trace("ImageList: remove_item");
    
        var e = Function._validateParams(arguments, [
            {name: "key", type: String}
        ]);
        if (e) throw e;

        if (this.get_isInitialized())
        {
            //remove from collection
            var item = this.get_item(key);
            if (this._removeFromCollection(item))
            {    
                //remove from UI
                this._removeFromUI(key);
                //Raise the remove event
                var eventHandler = this.get_events().getHandler("remove");
                if (eventHandler)
                    eventHandler(this, item);
            }
        }
    },
    
    _removeFromCollection: function(item) {
    
        Sys.Debug.trace("ImageList: _removeFromCollection");

        var e = Function._validateParams(arguments, [
            {name: "item", type: Memba.WebControls.ImageListItem}
        ]);
        if (e) throw e;

        //Make sure we have a valid collection to add to
        Sys.Debug.assert(this._ImageListItemCollection instanceof Array, "this._ImageListItemCollection should be an Array");
        
        var removedRet = false;
        
        if((item != null) && (item instanceof Memba.WebControls.ImageListItem))
        {
            var index = this._indexOf(item);
            if (index > -1)
            {
                Array.removeAt(this._ImageListItemCollection, index);
                this._HiddenField.value = Sys.Serialization.JavaScriptSerializer.serialize(this._ImageListItemCollection);
                Sys.Debug.trace("ImageList: new hidden field value: " + this._HiddenField.value);
                removedRet = true;
            }
        }
        
        return removedRet;
    },
    
    _removeFromUI: function(key) {

        Sys.Debug.trace("ImageList: _removeFromUI");

        var e = Function._validateParams(arguments, [
            {name: "key", type: String}
        ]);
        if (e) throw e;
       
        var divItem = $get(this.get_id() + "_Item_" + key);
        $clearHandlers(divItem); //mouse over/out
        Sys.Debug.trace("ImageList: Event handlers removed for " + divItem.id);
        $clearHandlers(divItem.childNodes[2]); //remove image
        Sys.Debug.trace("ImageList: Event handlers removed for remove image");
        this._ControlDiv.removeChild(divItem);
    },
    
    _clearUI : function() {
    
        Sys.Debug.trace("ImageList: _clearUI");
        
        //Make sure we have a valid collection to add to
        Sys.Debug.assert(this._ImageListItemCollection instanceof Array, "this._ImageListItemCollection should be an Array");
    
        for(iteratorX=0; iteratorX<this._ImageListItemCollection.length; iteratorX++)
        {
            this._removeFromUI(this._ImageListItemCollection[iteratorX].get_key());
        }
    },
    
    clear : function() {

        Sys.Debug.trace("ImageList: clear");
        this._clearUI();
        Array.clear(this._ImageListItemCollection);
        this._HiddenField.value = Sys.Serialization.JavaScriptSerializer.serialize(this._ImageListItemCollection);
        Sys.Debug.trace("ImageList: new hidden field value is " + this._HiddenField.value);

    },

    get_count : function()
    {
        if (this._ImageListItemCollection instanceof Array)
            return this._ImageListItemCollection.length;
        else
            return 0;
    },
    
    get_removeTooltip : function()
    {
        return this._RemoveTooltip;
    },

    set_removeTooltip : function(value)
    {
        if (this._RemoveTooltip != value)
        {
            this._RemoveTooltip = value;
            if (this.get_isInitialized())
            {
                //TODO: We could certainly be more clever, but this will do the job in v1
                this._clearUI();
                this._buildUI();
            }

            // Raise the propertyChanged event
            this.raisePropertyChanged("removeTooltip");
        }
    },
    
    get_removeImage : function()
    {
        return this._RemoveImage;
    },
 
    set_removeImage : function(value)
    {
        if (this._RemoveImage != value)
        {
            this._RemoveImage = value;
            if (this.get_isInitialized())
            {
                //TODO: We could certainly be more clever, but this will do the job in v1
                this._clearUI();
                this._buildUI();
            }

            // Raise the propertyChanged event
            this.raisePropertyChanged('removeImage');
        }
    },

    get_cssClass : function()
    {
        return this._CssClass;
    },
 
    set_cssClass : function(value)
    {
        if (this._CssClass != value)
        {
            if (this.get_isInitialized() && (this._ControlDiv))
                this._ControlDiv.className = value;
            this._CssClass = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('cssClass');
        }
    },

    get_itemCssClass : function()
    {
        return this._ItemCssClass;
    },

    set_itemCssClass : function(value)
    {
        if (this._ItemCssClass != value)
        {
            if (this.get_isInitialized() && (this._ControlDiv))
                this._changeTreeClassName(this._ControlDiv, this._ItemCssClass, value);          
            this._ItemCssClass = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('itemCssClass');
        }
    },

    get_itemHoverCssClass : function()
    {
        return this._ItemHoverCssClass;
    },

    set_itemHoverCssClass : function(value)
    {
        if (this._ItemHoverCssClass != value)
        {
            if (this.get_isInitialized() && (this._ControlDiv))
                this._changeTreeClassName(this._ControlDiv, this._ItemHoverCssClass, value);          
            this._ItemHoverCssClass = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('itemHoverCssClass');
        }
    },

    get_imageCssClass : function()
    {
        return this._ImageCssClass;
    },

    set_imageCssClass : function(value)
    {
        if (this._ImageCssClass != value)
        {
            if (this.get_isInitialized() && (this._ControlDiv))
                this._changeTreeClassName(this._ControlDiv, this._ImageCssClass, value);          
            this._ImageCssClass = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('imageCssClass');
        }
    },

    get_textCssClass : function()
    {
        return this._TextCssClass;
    },

    set_textCssClass : function(value)
    {
        if (this._TextCssClass != value)
        {
            if (this.get_isInitialized() && (this._ControlDiv))
                this._changeTreeClassName(this._ControlDiv, this._TextCssClass, value);          
            this._TextCssClass = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('textCssClass');
        }
    },
    
    get_removeCssClass : function()
    {
        return this._RemoveCssClass;
    },

    set_removeCssClass : function(value)
    {
        if (this._RemoveCssClass != value)
        {
            if (this.get_isInitialized() && (this._ControlDiv))
                this._changeTreeClassName(this._ControlDiv, this._RemoveCssClass, value);          
            this._RemoveCssClass = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('removeCssClass');
        }
    },
    
    _changeTreeClassName : function(element, oldClass, newClass)
    {
        Sys.Debug.trace("ImageList: _changeClassName");
        
        var e = Function._validateParams(arguments, [
            {name: "element", domElement: true},
            {name: "oldClass", type: String, mayBeNull: true},
            {name: "newClass", type: String}
        ]);
        if (e) throw e;

        //TODO: we know there is potentially a problem if the name oldClass
        //is used for two types of elements, for example <id>_Image_<key> and <id>_Remove_<key>
        //but this will do the job in v1

        //if(element.className == oldClass)
        //    element.className = newClass;
        
        if(element.childNodes)
        {
            for(iteratorX = 0; iteratorX<element.childNodes.length; iteratorX++)
            {
                if(element.childNodes[iteratorX].className == oldClass)
                    element.childNodes[iteratorX].className = newClass;
            }
        }
    },
    
    get_linesOfText : function()
    {
        return this._LinesOfText;
    },

    set_linesOfText : function(value)
    {
        if (this._LinesOfText != value)
        {
            this._LinesOfText = value;
            if (this.get_isInitialized())
            {
                //TODO: We could certainly be more clever, but this will do the job in v1
                this._clearUI();
                this._buildUI();
            }

            // Raise the propertyChanged event
            this.raisePropertyChanged("linesOfText");
        }
    },
    
    //truncWithEllipsis takes the item text, truncates it to be displayed on this._LinesOfText lines
    //then adds an ellipsis (...) at the end of the last line if needed. 
    _truncWithEllipsis: function(element, text)
    {
        Sys.Debug.trace("ImageList: _truncWithEllipsis");
        
        var e = Function._validateParams(arguments, [
            {name: "element", domElement: true},
            {name: "text", type: String}
        ]);
        if (e) throw e;

        if (this._LinesOfText == 0) //We explicitely tell we do not want truncation
        {
            element.innerHTML = text;
            return;
	    }
	    
	    //TODO: we could improve the function with soft hyphenation on specific characters
	    //especially not to leave orphan characters at the end of a line.
	    var lines = new Array();
	    var pos = 0;    
	    var width = element.offsetWidth;
        Sys.Debug.trace("ImageList: type of offsetWidth is " + typeof (element.style.width)); 
	    
	    if (width == 0) //offsetWidth returns 0 in Netscape, so we do a best guess
        {
            width = parseInt(element.style.width);
            if(isNaN(width))
                width = parseInt(element.parentNode.style.width);
            if(isNaN(width))
            {
                element.innerHTML = text;
                return;
            }    
	    }
	    
        Sys.Debug.trace("ImageList: truncate text to display on " + this._LinesOfText + " lines on a width of " + width);

	    element.innerHTML = '<span id="' + element.id + '_Span" style="white-space:nowrap;">' + text + '</span>';
   	    var tmpSpan = document.getElementById(element.id + "_Span");

	    if(tmpSpan.offsetWidth <= width) //There is no need for truncation
	    {
            element.innerHTML = text;
            return;
	    }
	    
	    var lnum = 0;
	    var tx = text;

  	    while (lnum < this._LinesOfText)
	    {
		    var cnum = 1;
		    tmpSpan.innerHTML = '';

		    while((tmpSpan.offsetWidth < width) && (cnum < tx.length))
		    {
			    tmpSpan.innerHTML = tx.substr(0,cnum) + '...';
			    cnum++; //next character
		    }
            
            if ((lnum > 0) && (tx.substr(0,cnum).length < lines[lnum-1].length)) //less characters than preceding line 
		        lines[lnum] = tx.substr(0,cnum); //otherwise we always add ellipsis at the end	        
		    else
		        lines[lnum] = tx.substr(0,cnum-2); //Play safe, remove two chars
		    pos += lines[lnum].length;
		    tx=text.substr(pos);
		    lnum++; //next line
	    }

	    var displayText = '';
	    if (lnum > 1)
	    {
		    for (l=0; l < lnum-1; l++)
			    displayText += lines[l] + "<br/>";
	    }
	    displayText += lines[lnum-1];
        if(pos < text.length)
	        displayText += '...';
	        
  	    element.innerHTML = displayText;
    },

    get_enabled : function()
    {
        return this._Enabled;
    },

    set_enabled : function(value)
    {
        var e = Function._validateParams(arguments, [
            {name: "value", type: Boolean}
        ]);
        if (e) throw e;
        
        if (this._Enabled != value)
        {
            this._Enabled = value;
            if (this.get_isInitialized())
            {
                //TODO: We could certainly be more clever, but this will do the job in v1
                this._clearUI();
                this._buildUI();
            }

            // Raise the propertyChanged event
            this.raisePropertyChanged("enabled");
        }
    },

    /*
    get_errorUrl : function()
    {
        return this._ErrorUrl;
    },

    set_errorUrl : function(value)
    {
        this._ErrorUrl = value;
    },
    */
       
    // add event
    add_add: function(handler) {
        /// <summary>Adds a event handler for the add event.</summary>
        /// <param name="handler" type="Function">The handler to add to the event.</param>
        this.get_events().addHandler("add", handler);
    },
    remove_add: function(handler) {
        /// <summary>Removes a event handler for the add event.</summary>
        /// <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("add", handler);
    },

    // remove event
    add_remove: function(handler) {
        /// <summary>Adds a event handler for the remove event.</summary>
        /// <param name="handler" type="Function">The handler to add to the event.</param>
        this.get_events().addHandler("remove", handler);
    },
    remove_remove: function(handler) {
        /// <summary>Removes a event handler for the remove event.</summary>
        /// <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("remove", handler);
    },
    
    initialize : function()
    {
        Sys.Debug.trace("ImageList: initialization");
        
        Memba.WebControls.ImageList.callBaseMethod(this, 'initialize');
        
        this._ImageListItemCollection = new Array();
        
        //this._InnerDiv = $get(this.get_id() + "_Inner");
        this._ControlDiv = this.get_element();
        this._ControlDiv.className = this._CssClass;
        
        this._HiddenField = $get(this.get_id() + "_HiddenField");
        Sys.Debug.trace("ImageList: new hidden field value is " + this._HiddenField.value);
        
        if(this._HiddenField.value != '')
        {
            var aSerial = Sys.Serialization.JavaScriptSerializer.deserialize(this._HiddenField.value);
            Sys.Debug.assert(aSerial instanceof Array, "Deserialized hidden field should be an Array");
            
            //We would rather have a typed collection
            for (iteratorX=0; iteratorX<aSerial.length; iteratorX++)
            {           
                var item = new Memba.WebControls.ImageListItem(
                    aSerial[iteratorX].Key,
                    aSerial[iteratorX].ImageUrl,
                    aSerial[iteratorX].Text,
                    aSerial[iteratorX].Tooltip,
                    aSerial[iteratorX].Tag);
                    
                Array.add(this._ImageListItemCollection, item);
                this._add2UI(item);
            }
        }
    },
    
    dispose : function()
    {
        Sys.Debug.trace("ImageList: disposal");
        
        this._clearUI(); //This will clear handlers

        Memba.WebControls.ImageList.callBaseMethod(this, 'dispose');
    }
}

Memba.WebControls.ImageList.registerClass('Memba.WebControls.ImageList', Sys.UI.Control);

if (typeof (Sys) != 'undefined')
{
    Sys.Application.notifyScriptLoaded();
}
