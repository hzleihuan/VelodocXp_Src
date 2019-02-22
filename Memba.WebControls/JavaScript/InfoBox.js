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
//InfoBoxType Enum
//--------------------------------------------
Memba.WebControls.InfoBoxType = function() {
}
Memba.WebControls.InfoBoxType.prototype = {
    Error : 0,
    Information : 1,
    OK : 2,
    Warning : 3
}
Memba.WebControls.InfoBoxType.registerEnum('Memba.WebControls.InfoBoxType', false);

//--------------------------------------------
//InfoBox Control
//--------------------------------------------
Memba.WebControls.InfoBox = function(element)
{
    Sys.Debug.trace("InfoBox: constructor");
    
    this._Text = null;
    this._ImageUrl = null;
    this._Type = null;
    this._CssClass = null;
    this._TextCssClass = null;
    this._ImageCssClass = null;
    
    this._Table = null;
    this._Image = null;
    this._Label = null;
    
    this._ErrorUrl = null;
    this._InformationUrl = null;
    this._OkUrl = null;
    this._WarningUrl = null;
    
    Memba.WebControls.InfoBox.initializeBase(this, [element]);
}

Memba.WebControls.InfoBox.prototype =
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
            if (this.get_isInitialized() && (this._Label))
                this._Label.innerHTML = value;
            if (this.get_isInitialized() && (this._Image))
                this._Image.alt = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('text');
        }
    },
    
    get_imageUrl : function()
    {
        return this._ImageUrl;
    },
 
    set_imageUrl : function(value)
    {
        if (this._ImageUrl != value)
        {
            this._ImageUrl = value;
            if (this.get_isInitialized() && (this._Image))
                this._Image.src = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('imageUrl');
        }
    },

    get_type : function()
    {
        return this._Type;
    },
 
    set_type : function(value)
    {
        if (this._Type != value)
        {
            this._Type = value;
            if (this.get_isInitialized() && (this._Image))
            {
                if ((this._ImageUrl == null) || (this._ImageUrl.trim() == ''))
                {
                    switch(value)
                    {
                        case Memba.WebControls.InfoBoxType.Error:                            
                            this._Image.src = this._ErrorUrl;
                            break; 
                        case Memba.WebControls.InfoBoxType.Information:
                            this._Image.src = this._InformationUrl;
                            break; 
                        case Memba.WebControls.InfoBoxType.OK:
                            this._Image.src = this._OkUrl;
                            break; 
                        case Memba.WebControls.InfoBoxType.Warning:
                            this._Image.src = this._WarningUrl;
                            break; 
                        default:
                            Sys.Debug.assert(typeof value == "number");
                    }
                }
            }
            // Raise the propertyChanged event
            this.raisePropertyChanged('type');
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
            if (this.get_isInitialized() && (this._Table))
                this._Table.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('cssClass');
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
            this._TextCssClass = value;
            if (this.get_isInitialized() && (this._Label))
                this._Label.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('textCssClass');
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
            this._ImageCssClass = value;
            if (this.get_isInitialized() && (this._Image))
                this._Image.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('imageCssClass');
        }
    },

    get_errorUrl : function()
    {
        return this._ErrorUrl;
    },

    set_errorUrl : function(value)
    {
        this._ErrorUrl = value;
    },
    
    get_informationUrl : function()
    {
        return this._InformationUrl;
    },

    set_informationUrl : function(value)
    {
        this._InformationUrl = value;
    },
    
    get_okUrl : function()
    {
        return this._OkUrl;
    },

    set_okUrl : function(value)
    {
        this._OkUrl = value;
    },
    
    get_warningUrl : function()
    {
        return this._WarningUrl;
    },

    set_warningUrl : function(value)
    {
        this._WarningUrl = value;
    },
    
    setAll : function(type, text)
    {
        this.set_type(type);
        this.set_text(text);        
    },
    
    setTemp : function(type, text, duration)
    {
        if (typeof duration == "undefined")
            duration = 3000;
        var originalType = this.get_type();
        var originalText = this.get_text();
        this.set_type(type);
        this.set_text(text);
        setTimeout('$find("' + this.get_id() + '").setAll(' + originalType + ', "' + originalText + '");', duration);
    },
    
    initialize : function()
    {
        Sys.Debug.trace("InfoBox: initialization");
        
        Memba.WebControls.InfoBox.callBaseMethod(this, 'initialize');
        
        this._Table = this.get_element();
        this._Table.className = this._CssClass;
        this._Label = $get(this.get_id() + "_Label");
        this._Label.innerHTML = this._Text;
        this._Label.className = this._TextCssClass;
        this._Image = $get(this.get_id() + "_Image");
        if ((this._ImageUrl != null) && (this._ImageUrl.trim() != ''))
            this._Image.src = this._ImageUrl;
        else
        {
            switch(this._Type)
            {
                case Memba.WebControls.InfoBoxType.Error:
                    this._Image.src = this._ErrorUrl;
                    break; 
                case Memba.WebControls.InfoBoxType.Information:
                    this._Image.src = this._InformationUrl;
                    break; 
                case Memba.WebControls.InfoBoxType.OK:
                    this._Image.src = this._OkUrl;
                    break; 
                case Memba.WebControls.InfoBoxType.Warning:
                    this._Image.src = this._WarningUrl;
                    break; 
                default:
            }
        }
        this._Image.alt = this._Text;
        this._Image.className = this._ImageCssClass;      
    },
    
    dispose : function()
    {
        Sys.Debug.trace("InfoBox: disposal");
        
        Memba.WebControls.InfoBox.callBaseMethod(this, 'dispose');
    }
}

Memba.WebControls.InfoBox.registerClass('Memba.WebControls.InfoBox', Sys.UI.Control);

if (typeof (Sys) != 'undefined')
{
    Sys.Application.notifyScriptLoaded();
}

