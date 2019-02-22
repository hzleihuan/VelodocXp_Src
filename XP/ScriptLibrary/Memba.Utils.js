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
Type.registerNamespace("Memba");

Memba._Utils = function() {
    Sys.Debug.trace("Utils: Constructor");
}

Memba._Utils.prototype = {
  
    //Creates a new guid
    newGuid : function()
    {
        var g = "";
        for(var iteratorX = 0; iteratorX < 32; iteratorX++)
        {
            g += Math.floor(Math.random() * 0xF).toString(0xF) + (iteratorX == 7 || iteratorX == 11 || iteratorX == 15 || iteratorX == 19 ? "-" : "");
        }
        Sys.Debug.trace("Utils: Guid value is " + g);
        return g;
    },
    
    // Make sure the Regex patterns are compatible with Memba.Framework.Business.BOValidation.cs
    isEmailAddress : function(email)
    {
        Sys.Debug.trace("Utils: Validating email address " + email);
        
        var e = Function._validateParams(arguments, [
            {name: "email", type: String}
        ]);
        if (e) throw e;
    
	    var rx  = /^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$/;
	    if (rx.test(email))
	        return true;
	    else
	        return false;
    },
    
    isEmailList : function(list)
    {
    
        Sys.Debug.trace("Utils: Validating email list " + list);
        
        var e = Function._validateParams(arguments, [
            {name: "list", type: String}
        ]);
        if (e) throw e;

        var rx = /\s*[,;\t\r\n\s]\s*/;
        var emailArray = list.split(rx);
        
        if(!(emailArray) || (emailArray.length < 1))
            return false;
            
        for (var iteratorX = 0; iteratorX < emailArray.length; iteratorX++ )
        {
            Sys.Debug.trace("Utils: Validating email from list " + emailArray[iteratorX]);
            if (!this.isEmailAddress(emailArray[iteratorX]))
                return false;
        }
        
        return true;
    },
    
    // Source: http://regexlib.com/Search.aspx?k=UNC <------------------------------------------
    // ^([a-zA-Z]\:|\\\\[^\/\\:*?"<>|]+\\[^\/\\:*?"<>|]+)(\\[^\/\\:*?"<>|]+)+(\.[^\/\\:*?"<>|]+)?$
    // Source http://regexlib.com/REDetails.aspx?regexp_id=345
    // ^(([a-zA-Z]:|\\)\\)?(((\.)|(\.\.)|([^\\/:\*\?"\|<>\. ](([^\\/:\*\?"\|<>\. ])|([^\\/:\*\?"\|<>]*[^\\/:\*\?"\|<>\. ]))?))\\)*[^\\/:\*\?"\|<>\. ](([^\\/:\*\?"\|<>\. ])|([^\\/:\*\?"\|<>]*[^\\/:\*\?"\|<>\. ]))?$
    isUNCPath : function(uncPath)
    {
        Sys.Debug.trace("Utils: Validating UNC path " + uncPath);
        
        var e = Function._validateParams(arguments, [
            {name: "uncPath", type: String}
        ]);
        if (e) throw e;

        var rx = /^([a-zA-Z]\:|\\\\[^\/\\:*?"<>|]+\\[^\/\\:*?"<>|]+)(\\[^\/\\:*?"<>|]+)+(\.[^\/\\:*?"<>|]+)?$/; 
	    if (rx.test(uncPath))
	        return true;
	    else
	        return false;
    },
    
    //Source: http://regexlib.com/REDetails.aspx?regexp_id=1809
    // ^[/]*([^/\\ \:\*\?"\<\>\|\.][^/\\\:\*\?\"\<\>\|]{0,63}/)*[^/\\ \:\*\?"\<\>\|\.][^/\\\:\*\?\"\<\>\|]{0,63}$
    isUnixPath : function(unixPath)
    {
        Sys.Debug.trace("Utils: Validating Unix path " + unixPath);
        
        var e = Function._validateParams(arguments, [
            {name: "unixPath", type: String}
        ]);
        if (e) throw e;

        var rx = /^[\/]*([^\/\\ \:\*\?"\<\>\|\.][^\/\\\:\*\?\"\<\>\|]{0,63}\/)*[^\/\\ \:\*\?"\<\>\|\.][^\/\\\:\*\?\"\<\>\|]{0,63}$/
	    if (rx.test(unixPath))
	        return true;
	    else
	        return false;
    },
    
    getFileNameFromPath : function(path)
    {
    
        Sys.Debug.trace("Utils: get file name for " + path);

        var e = Function._validateParams(arguments, [
            {name: "path", type: String, mayBeNull: true}
        ]);
        if (e) throw e;

        if(!(path) || (path.length == 0) || ((path.indexOf('\\') < 0) && (path.indexOf('/') < 0)))
            return path;
               
        var rx = /[\/|\\]([^\/\\]+)?$/;
        var m = rx.exec(path);
        if (m == null)
            return null;
        
        Sys.Debug.assert(m.length == 2);
        return m[1];
        
    },
    
    removeExtFromFileName : function(filename)
    {
        Sys.Debug.trace("Utils: get file name without extension for " + filename);

        var e = Function._validateParams(arguments, [
            {name: "filename", type: String, mayBeNull: true}
        ]);
        if (e) throw e;
        
        if(!(filename) || (filename.length == 0))
            return filename;
        
        var pos = filename.lastIndexOf('.');
        
        if(pos < 0)
            return filename;
        else if (pos == 0) //This is improbable
            return filename.substr(1, filename.length - 2);
        else
            return filename.substr(0, pos);
    },
    
    removeParamFromUrl : function(param, url)
    {
        var e = Function._validateParams(arguments, [
            {name: "param", type: String},
            {name: "url", type: String}
        ]);
        if (e) throw e;

        Sys.Debug.trace("Removing param " + param + " from Url " + url);
        var _param = param.toLowerCase();
        var _url = url.toLowerCase();
        if ((_url.indexOf('?' + _param + '=') < 0) && (_url.indexOf('&' + _param + '=') < 0))
            return url;
        
        var _pos = _url.indexOf('?');
        var _ret = _url.substr(0, _pos + 1).toLowerCase(); //includes ?
        Sys.Debug.trace("Path is " + _ret);
        var _query = _url.substr(_pos + 1).toLowerCase();
        Sys.Debug.trace("Query is " + _query);
        var _params = _query.split('&');
        for (var iteratorX = 0; iteratorX < _params.length; iteratorX++ )
        {
            if (_params[iteratorX].indexOf(_param + '=') < 0)
            {
                Sys.Debug.trace("Adding param " + _params[iteratorX]);
                _ret += _params[iteratorX] + '&';
            }
        }
        Sys.Debug.trace("Last char is " + _ret.substr(_ret.length-1));       
        if ((_ret.substr(_ret.length-1) == '?') || (_ret.substr(_ret.length-1) == '&'))
            _ret = _ret.substr(0, _ret.length-1)
        return _ret;
    }, 

    addParamToUrl : function(param, value, url)
    {
        var e = Function._validateParams(arguments, [
            {name: "param", type: String},
            {name: "value", type: String},
            {name: "url", type: String}
        ]);
        if (e) throw e;

        Sys.Debug.trace("Adding param " + param + " with value " + value + " from Url " + url);
        var _param = param.toLowerCase();
        var _ret = url.toLowerCase();
        if ((_ret.indexOf('?' + _param + '=') > -1) || (_ret.indexOf('&' + _param + '=') > -1))
        {
            _ret = this.removeParamFromUrl(_param, _ret);
            Sys.Debug.trace(_param + " is already in url, removed");
        }
        var _pos = _ret.indexOf('?');
        if (_pos < 0)
            _ret += '?';
        else
        {
            if ((_ret.substr(_ret.length-1) != '?') && (_ret.substr(_ret.length-1) != '&'))
                _ret = _ret + '&'
        }
        return _ret + _param + '=' + escape(value);
    }, 
    
    //html encoding: see escapeHTML in prototype.js framework
    encode : function(value)
    {
        var e = Function._validateParams(arguments, [
            {name: "value", type: String}
        ]);
        if (e) throw e;

        Sys.Debug.trace("Encoding string " + value);
        var _div = document.createElement('div');
        var _text = document.createTextNode(value);
        _div.appendChild(_text);
        return _div.innerHTML;
    }
}

Memba._Utils.registerClass('Memba._Utils', null);

Memba.Utils = new Memba._Utils();

if (typeof (Sys) != 'undefined')
{
    Sys.Application.notifyScriptLoaded();
}
