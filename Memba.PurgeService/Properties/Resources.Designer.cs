﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1434
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Memba.PurgeService.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Memba.PurgeService.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Some values cannot be accessed in configuration file..
        /// </summary>
        internal static string PurgeService_BadAppSettings {
            get {
                return ResourceManager.GetString("PurgeService_BadAppSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The storage directory defined in configuration file does not exist..
        /// </summary>
        internal static string PurgeService_BadDirectory {
            get {
                return ResourceManager.GetString("PurgeService_BadDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete file &quot;{0}&quot;..
        /// </summary>
        internal static string PurgeService_DeleteFail {
            get {
                return ResourceManager.GetString("PurgeService_DeleteFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} files deleted..
        /// </summary>
        internal static string PurgeService_DeleteSuccess {
            get {
                return ResourceManager.GetString("PurgeService_DeleteSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Memba Velodoc XP Purge Service.
        /// </summary>
        internal static string PurgeService_DisplayName {
            get {
                return ResourceManager.GetString("PurgeService_DisplayName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Automatically removes old files from the Velodoc storage directory..
        /// </summary>
        internal static string PurgeService_InstallerDescription {
            get {
                return ResourceManager.GetString("PurgeService_InstallerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Purge service started..
        /// </summary>
        internal static string PurgeService_Started {
            get {
                return ResourceManager.GetString("PurgeService_Started", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Purge service stopped..
        /// </summary>
        internal static string PurgeService_Stopped {
            get {
                return ResourceManager.GetString("PurgeService_Stopped", resourceCulture);
            }
        }
    }
}
