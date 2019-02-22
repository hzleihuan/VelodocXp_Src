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
//MultiUpload2EventArgs
//--------------------------------------------
Memba.WebControls.MultiUpload2EventArgs = function(id, value) {

    Sys.Debug.trace("MultiUpload2EventArgs: constructor");

    var e = Function._validateParams(arguments, [
        {name: "id", type: String},
        {name: "value", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    this._id = id;
    this._value = value;
}

Memba.WebControls.MultiUpload2EventArgs.prototype = {

    get_id: function() {
        return this._id;
    },

    get_value: function() {
        return this._value;
    }
}

Memba.WebControls.MultiUpload2EventArgs.registerClass('Memba.WebControls.MultiUpload2EventArgs', null);

//--------------------------------------------
//MultiUpload2 Control
//--------------------------------------------
Memba.WebControls.MultiUpload2 = function(element)
{
    Sys.Debug.trace("MultiUpload2: constructor");
    
    this._Text = null;
    this._CssClass = null;
    this._HoverCssClass = null;
    
    this._LinkSpan = null;
    this._InputsDiv = null;
    this._CurrentInput = null;
    this._CurrentIdNum = null;
    this._Count = null;
    
    Memba.WebControls.MultiUpload2.initializeBase(this, [element]);
}

Memba.WebControls.MultiUpload2.prototype =
{
    get_text : function()
    {
        return this._Text;
    },

    set_text : function(value)
    {
        if (this._Text != value)
        {
            this._Text = value;
            if (this.get_isInitialized() && (this._LinkSpan))
                this._LinkSpan.innerHTML = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('text');
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
            this._CssClass = value;
            if (this.get_isInitialized() && (this._LinkSpan))
                this._LinkSpan.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('cssClass');
        }
    },
    
    get_hoverCssClass : function()
    {
        return this._HoverCssClass;
    },
 
    set_hoverCssClass : function(value)
    {
        if (this._HoverCssClass != value)
        {
            this._HoverCssClass = value;
            //if (this.get_isInitialized() && (this._LinkSpan))
            //    this._LinkSpan.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('hoverCssClass');
        }
    },

    get_count : function()
    {
        return this._Count;
    },

    removeInput : function(id)
    {
        Sys.Debug.trace("MultiUpload2: removing input id " + id); 
        var input = $get(id);
        if((input) && (this._InputsDiv) && (this._CurrentInput)
            && (input.id != this._CurrentInput.id) && (input.parentNode.id == this._InputsDiv.id))  
        {
            this._InputsDiv.removeChild(input);
            this._Count--;
        }
        Sys.Debug.trace("MultiUpload2: " + this._Count + " files in multi-upload control");
    },
    
    clear : function()
    {
        Sys.Debug.trace("MultiUpload2: clear"); 
        while((this._InputsDiv.childNodes) && (this._InputsDiv.childNodes.length > 0))
        {
            this._InputsDiv.removeChild(this._InputsDiv.childNodes[0]);
            this._Count--;
        }
        Sys.Debug.trace("MultiUpload2: " + this._Count + " files in multi-upload control");
    },
    
    // browse event
    add_browse: function(handler) {
        /// <summary>Adds a event handler for the browse event.</summary>
        /// <param name="handler" type="Function">The handler to add to the event.</param>
        this.get_events().addHandler("browse", handler);
    },
    remove_browse: function(handler) {
        /// <summary>Removes a event handler for the browse event.</summary>
        /// <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("browse", handler);
    },
    
    initialize : function()
    {
        Sys.Debug.trace("MultiUpload2: initialization");
        
        Memba.WebControls.MultiUpload2.callBaseMethod(this, 'initialize');
        
        var controlEl = this.get_element();
        
        this._LinkSpan = $get(this.get_id() + "_Link");
        this._LinkSpan.className = this._CssClass;
        
        this._InputsDiv = $get(this.get_id() + "_Inputs");
        this._CurrentInput = this.get_element().childNodes[1];
        this._CurrentIdNum = 0;
        this._Count = 0;
        
        //add mouse over/out handlers
        var onmouseover = Function.createDelegate(this, function() { 
            this._LinkSpan.className = this._HoverCssClass;
        });
        $addHandler(this._CurrentInput, "mouseover", onmouseover);

        var onmouseout = Function.createDelegate(this, function() { 
            this._LinkSpan.className = this._CssClass;
        });
        $addHandler(this._CurrentInput, "mouseout", onmouseout);
                            
        //add click handler        
        var onchange = Function.createDelegate(this, function() {
            
            Sys.Debug.trace("MultiUpload2: Opening file dialog");
            
            var clickedId = this._CurrentInput.id;
            var clickedValue = this._CurrentInput.value;
            Sys.Debug.trace("MultiUpload2: current input id is " + clickedId);
            Sys.Debug.trace("MultiUpload2: value is " + clickedValue);
            
            //Note: Each time a user clicks the control, this delegate is called twice:
            //First time on clicking the control, before the open file dialog is displayed
            //Second time on clicking Open or Cancel in the open file dialog
            //Note that the first time value is empty, like when clicking cancel
            //Thus teh following test 
            if (clickedValue != '')
            {
                //Create new file input element
                this._CurrentIdNum++;
                this._Count++;
                //We do not use cloneNode because Netscape copies the value.
                //Neither newInput.name nor newInput.setAttribute("name", ...) work in IE
                //The name should be added in createElement as in the code provided at
                //http://msdn2.microsoft.com/en-us/library/ms534184.aspx
                //See also: http://www.thunderguy.com/semicolon/2005/05/23/setting-the-name-attribute-in-internet-explorer/
                if (Sys.Browser.agent == Sys.Browser.InternetExplorer)
                    newInput = document.createElement('<input name="X"></input>');
                else
                    newInput = document.createElement("input");
                newInput.id = this.get_id() + "_Input_" + this._CurrentIdNum;
                newInput.name = this.get_id() + "_Input_" + this._CurrentIdNum;
                newInput.type = "file";
                newInput.size = "1";
                Sys.Debug.trace("MultiUpload2: new input name is " + newInput.name);
                newInput.style.cssText = this._CurrentInput.style.cssText;
                Sys.Debug.trace("MultiUpload2: new input style is " + newInput.style.cssText);
                controlEl.insertBefore(newInput, this._CurrentInput);
                               
                //Move the current file input
                var oldInput = this._CurrentInput;
                $clearHandlers(oldInput);
                controlEl.removeChild(oldInput);
                this._InputsDiv.appendChild(oldInput);
                
                //Assign the new input element to the current input
                this._CurrentInput = newInput;           
                
                //add mouse over/out handlers
                var onmouseover = Function.createDelegate(this, function() { 
                    this._LinkSpan.className = this._HoverCssClass;
                });
                $addHandler(this._CurrentInput, "mouseover", onmouseover);

                var onmouseout = Function.createDelegate(this, function() { 
                    this._LinkSpan.className = this._CssClass;
                });
                $addHandler(this._CurrentInput, "mouseout", onmouseout);
                
                //TODO: add onchange handler
                $addHandler(this._CurrentInput, "change", onchange);
                
                //Raise the browse event at the end
                var args = new Memba.WebControls.MultiUpload2EventArgs(oldInput.id, oldInput.value);
                Sys.Debug.trace("MultiUpload2: raising event browse event");
                var eventHandler = this.get_events().getHandler("browse");
                if (eventHandler)
                    eventHandler(this, args);
            }
            
            return true; //So that the open file dialog gets opened
        });
        
        $addHandler(this._CurrentInput, "change", onchange);
    },
    
    dispose : function()
    {
        Sys.Debug.trace("MultiUpload2: disposal");
        
        Memba.WebControls.MultiUpload2.callBaseMethod(this, 'dispose');
    }
}

Memba.WebControls.MultiUpload2.registerClass('Memba.WebControls.MultiUpload2', Sys.UI.Control);

if (typeof (Sys) != 'undefined')
{
    Sys.Application.notifyScriptLoaded();
}
