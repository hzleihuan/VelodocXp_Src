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
//ProgressReport Control
//--------------------------------------------
Memba.WebControls.ProgressReport = function(element)
{
    Sys.Debug.trace("ProgressReport Constructor");
    
    this._DefaulText = null;
    this._TextFormat = null
    this._HandlerUrl = null;
    this._Interval = null;
    this._Timeout = null;
    this._TimeoutID = null;
    
    this._CssClass = null;
    this._BarCssClass = null;
    this._FillerCssClass = null;
    this._BarCssClass = null;
    
    this._MainDiv = null;
    this._BarDiv = null;
    this._FillerDiv = null;
    this._TextDiv = null;
    
    this._CurrentRequest = null;
    this._IsComplete = null;
    this._IsStarted = false;
           
    Memba.WebControls.ProgressReport.initializeBase(this, [element]);
}

Memba.WebControls.ProgressReport.prototype =
{
    get_defaultText : function()
    {
        return this._DefaultText;
    },

    set_defaultText : function(value)
    {
        if (this._DefaultText != value)
        {
            this._DefaultText = value;
            if (this.get_isInitialized() && (this._TextDiv))
                this._TextDiv.innerHTML = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('defaultText');
        }
    },
    
    get_textFormat : function()
    {
        return this._TextFormat;
    },

    set_textFormat : function(value)
    {
        if (this._TextFormat != value)
        {
            this._TextFormat = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('textFormat');
        }
    },
    
    get_handlerUrl : function()
    {
        return this._HandlerUrl;
    },
 
    set_handlerUrl : function(value)
    {
        if (this._HandlerUrl != value)
        {
            this._HandlerUrl = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('handlerUrl');
        }
    },

    get_interval : function()
    {
        return this._Interval;
    },
 
    set_interval : function(value)
    {
        if (this._Interval != value)
        {
            this._Interval = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('interval');
        }
    },

    get_timeout : function()
    {
        return this._Timeout;
    },
 
    set_timeout : function(value)
    {
        if (this._Timeout != value)
        {
            this._Timeout = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('timeout');
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
            if (this.get_isInitialized() && (this._MainDiv))
                this._MainDiv.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('cssClass');
        }
    },
    
    get_barCssClass : function()
    {
        return this._BarCssClass;
    },

    set_barCssClass : function(value)
    {
        if (this._BarCssClass != value)
        {
            this._BarCssClass = value;
            if (this.get_isInitialized() && (this._BarDiv))
                this._BarDiv.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('barCssClass');
        }
    },
    
    get_fillerCssClass : function()
    {
        return this._FillerCssClass;
    },

    set_fillerCssClass : function(value)
    {
        if (this._FillerCssClass != value)
        {
            this._FillerCssClass = value;
            if (this.get_isInitialized() && (this._FillerDiv))
                this._FillerDiv.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('fillerCssClass');
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
                this._TextDiv.className = value;

            // Raise the propertyChanged event
            this.raisePropertyChanged('textCssClass');
        }
    },
    
    // error event
    add_error: function(handler) {
        /// <summary>Adds a event handler for the error event.</summary>
        /// <param name="handler" type="Function">The handler to add to the event.</param>
        this.get_events().addHandler("error", handler);
    },
    remove_error: function(handler) {
        /// <summary>Removes a event handler for the error event.</summary>
        /// <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("error", handler);
    },

    // complete event
    add_complete: function(handler) {
        /// <summary>Adds a event handler for the complete event.</summary>
        /// <param name="handler" type="Function">The handler to add to the event.</param>
        this.get_events().addHandler("complete", handler);
    },
    remove_complete: function(handler) {
        /// <summary>Removes a event handler for the complete event.</summary>
        /// <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("complete", handler);
    },
    
    start : function()
    {
        Sys.Debug.trace("Starting progress report");
        this._IsStarted = true;
        this._updateProgress();
    },
    
    stop : function()
    {
        Sys.Debug.trace("Stopping progress report");
        this._IsStarted = false;       
        //Abort request in progress 
        if((typeof(this._CurrentRequest) != "undefined") && (this._CurrentRequest != null)
            && (typeof(this._CurrentRequest.get_executor()) != "undefined") && (this._CurrentRequest.get_executor() != null))
            this._CurrentRequest.get_executor().abort();
    },
    
    //XmlHttp call to progress handler
    _updateProgress : function()
    {
        Sys.Debug.trace("Ajax call to update progress status");

        //No progress handler
        if ((typeof(this._HandlerUrl) == "undefined") || (this._HandlerUrl == null) || (this._HandlerUrl == ''))
            return;
        
        //Progress not visible
        if ((this._FillerDiv == null) && (this._TextDiv == null))
            return;
        
        //Progress not started, you need to call start(), not _updateProgress()
        if (!this._IsStarted)
            return;
        
        //Progress is complete
        if (this._IsComplete)
            return;
    
        //Abort request in progress to avoid queuing many identical requests 
        if((typeof(this._CurrentRequest) != "undefined") && (this._CurrentRequest != null)
            && (typeof(this._CurrentRequest.get_executor()) != "undefined") && (this._CurrentRequest.get_executor() != null))
            this._CurrentRequest.get_executor().abort();
        
        Sys.Debug.trace("Calling " + this._HandlerUrl);
        
        //Execute new request
        this._CurrentRequest = new Sys.Net.WebRequest();
        this._CurrentRequest.set_url(this._HandlerUrl);
        this._CurrentRequest.set_httpVerb("GET");
        this._CurrentRequest.set_timeout(this._Timeout);
        this._CurrentRequest.add_completed(Function.createDelegate(this, this._updateProgressCallback));
        this._CurrentRequest.invoke();
    
        //Call again later. We plan the next call here in case any callback does not execute 
        Sys.Debug.trace("Planning next call in " + this._Interval + "ms");
        this._TimeoutID = window.setTimeout(Function.createDelegate(this, this._updateProgress) , this._Interval);
    },
    
    //Asynchronous callback
    _updateProgressCallback : function(executor, eventArgs) 
    {    
        Sys.Debug.trace("Asynchronous callback to update progress status");
        
        //Progress not started
        if (!this._IsStarted)
            return;
        
        if(executor.get_responseAvailable()) 
        {
            Sys.Debug.trace("The callback did respond");
            
            try
            {
                var progressData = Sys.Serialization.JavaScriptSerializer.deserialize(executor.get_responseData());
                if (progressData)
                {
                    if((progressData.ErrorMessage != null) && (progressData.ErrorMessage != ''))
                    {
                        Sys.Debug.trace("There is an error: " + progressData.ErrorMessage);
                        
                        this._IsComplete = true;
                        //Raise the error event
                        var eventHandler = this.get_events().getHandler("error");
                        if (eventHandler)
                            eventHandler(this, progressData.ErrorMessage);
                    }
                    else
                    {
                        Sys.Debug.trace("We are progressing: " + progressData.FillRatio);

                        if (progressData.FillRatio)
                        {
                            var width = progressData.FillRatio;
                            
                            if(this._FillerDiv)
                            {
                                
                                //this._FillerDiv.style.width = width;
                                
                                var _updateFillerDiv = Function.createDelegate(this, function() { 
                                    this._FillerDiv.style.width = width;
                                    Sys.Debug.trace("Filler width is now " + this._FillerDiv.style.width);
                                });

                                window.setTimeout(_updateFillerDiv, 0);
                            }
                        }
                        
                        if((progressData.TextValues) && (progressData.TextValues instanceof Array))
                        {
                            if (progressData.TextValues.length > 0)
                            {
                                if (this._TextDiv)
                                {
                                    //using String.localeFormat does not work, see _toFormattedString below
                                    //var text = String.localeFormat(this._TextFormat, progressData.TextValues);
                                    var text = this._toFormattedString(this._TextFormat, progressData.TextValues);
                                    
                                    //It is probably simpler to do the following but
                                    //1) On one hand, some articles say JavaScript browser implementations are single threaded
                                    //2) On the other hand, other articles argue that setTimeout provides a "multithreaded effect"
                                    //TODO: Probably needs to be reviewed as we get more information.
                                    //this._TextDiv.innerHTML = eval(text);

                                    var _updateTextDiv = Function.createDelegate(this, function() { 
                                        Sys.Debug.trace("Updating text with " + text);
                                        this._TextDiv.innerHTML = text; //eval(text);
                                    });
                                
                                    window.setTimeout(_updateTextDiv, 0);
                                }
                            }
                        }
                        
                        //If Upload is complete
                        if (progressData.IsComplete)
                        {
                            Sys.Debug.trace("We are complete");

                            this._IsComplete = true;

                            this.stop();

                            //Raise the complete event
                            var eventHandler = this.get_events().getHandler("complete");
                            if (eventHandler)
                                eventHandler(this, progressData.FillRatio);
                        }
                    }
                }
            }
            catch(e)
            {
            }
        }
    },

    //This function is derived from String._toFormattedString which I could not get to work as expected when args is an array
    _toFormattedString : function(format, args) {
        /// <param name="format" type="String"></param>
        /// <param name="args" parameterArray="true" mayBeNull="true"></param>
        /// <returns type="String"></returns>
        var e = Function._validateParams(arguments, [
            {name: "format", type: String},
            {name: "args", mayBeNull: true, parameterArray: true}
        ]);
        if (e) throw e;

        //alert(arguments.length) displays 2
        //This is the problem of the String.localFormat() function which works on the basis
        //that arguments[0] = format and arguments[i+1] = args[i]
        //when in fact we have arguments[0] = format and arguments[1] = args.

        var useLocale = true;
        var result = '';

        for (var pos=0;;) {
        
            // Find the next opening or closing brace
            var open = format.indexOf('{', pos);
            var close = format.indexOf('}', pos);
            if ((open < 0) && (close < 0)) {
                // Not found: copy the end of the string and break
                result += format.slice(pos);
                break;
            }
            if ((close > 0) && ((close < open) || (open < 0))) {
                // Closing brace before opening is an error
                if (format.charAt(close + 1) !== '}') {
                    throw Error.argument('format', Sys.Res.stringFormatBraceMismatch);
                }
                result += format.slice(pos, close + 1);
                pos = close + 2;
                continue;
            }

            // Copy the string before the brace
            result += format.slice(pos, open);
            pos = open + 1;

            // Check for double braces (which display as one and are not arguments)
            if (format.charAt(pos) === '{') {
                result += '{';
                pos++;
                continue;
            }

            // at this point we have a valid opening brace, which should be matched by a closing brace.
            if (close < 0) throw Error.argument('format', Sys.Res.stringFormatBraceMismatch);

            // Find the closing brace

            // Get the string between the braces, and split it around the ':' (if any)
            var brace = format.substring(pos, close);
            var colonIndex = brace.indexOf(':');
            var argNumber = parseInt((colonIndex < 0)? brace : brace.substring(0, colonIndex)); // + 1;
            if (isNaN(argNumber)) throw Error.argument('format', Sys.Res.stringFormatInvalid);
            var argFormat = (colonIndex < 0)? '' : brace.substring(colonIndex + 1);

            //alert(argNumber.toString() + " -> " + args[argNumber]);
            var arg = args[argNumber];
            if (typeof(arg) === "undefined" || arg === null) {
                arg = '';
            }

            // If it has a toFormattedString method, call it.  Otherwise, call toString()
            if (arg.toFormattedString) {
                result += arg.toFormattedString(argFormat);
            }
            else if (useLocale && arg.localeFormat) {
                result += arg.localeFormat(argFormat);
            }
            else if (arg.format) {
                result += arg.format(argFormat);
            }
            else
                result += arg.toString();

            pos = close + 1;
        }

        return result;
    },

    initialize : function()
    {
        Sys.Debug.trace("ProgressReport initialization");
        
        Memba.WebControls.ProgressReport.callBaseMethod(this, 'initialize');
        
        this._MainDiv = this.get_element();
        if(this._MainDiv)
            this._MainDiv.className = this._CssClass;
        this._BarDiv = $get(this.get_id() + "_Bar");
        if(this._BarDiv)
            this._BarDiv.className = this._BarCssClass;
        this._FillerDiv = $get(this.get_id() + "_Filler");
        if (this._FillerDiv)
            this._FillerDiv.className = this._FillerCssClass;
        this._TextDiv = $get(this.get_id() + "_Text");
        if (this._TextDiv)
        {
            this._TextDiv.className = this._TextCssClass;
            this._TextDiv.innerHTML = this._DefaultText;
        }
    },
    
    dispose : function()
    {
        Sys.Debug.trace("ProgressReport disposal");
        
        Memba.WebControls.ProgressReport.callBaseMethod(this, 'dispose');
    }
}

Memba.WebControls.ProgressReport.registerClass('Memba.WebControls.ProgressReport', Sys.UI.Control);

if (typeof (Sys) != 'undefined')
{
    Sys.Application.notifyScriptLoaded();
}


