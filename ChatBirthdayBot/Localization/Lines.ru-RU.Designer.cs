﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChatBirthdayBot.Localization {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Lines_ru_RU {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Lines_ru_RU() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ChatBirthdayBot.Localization.Lines.ru-RU", typeof(Lines_ru_RU).Assembly);
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
        ///   Looks up a localized string similar to Ваш день/дата рождения выставлена на {0}. Если это неверно, исправьте её с помощью команды &lt;code&gt;/setbirthday &amp;lt;ДД-ММ-ГГГГ&amp;gt;&lt;/code&gt;. Если вы не хотите указывать год рождения, используйте формат &lt;code&gt;&amp;lt;ДД-ММ&amp;gt;&lt;/code&gt;. Для удаления записи из базы данных используйте команду &lt;code&gt;/removebirthday&lt;/code&gt;..
        /// </summary>
        internal static string BirthdayDate {
            get {
                return ResourceManager.GetString("BirthdayDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ваш день рождения не выставлен. Задать её можно командой &lt;code&gt;/setbirthday &amp;lt;ДД-ММ-ГГГГ&amp;gt;&lt;/code&gt;. Если вы не хотите указывать год, используйте формат &lt;code&gt;&amp;lt;ДД-ММ&amp;gt;&lt;/code&gt;..
        /// </summary>
        internal static string BirthdayNotSet {
            get {
                return ResourceManager.GetString("BirthdayNotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ваш день рождения был удалён..
        /// </summary>
        internal static string BirthdayRemoved {
            get {
                return ResourceManager.GetString("BirthdayRemoved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Вы указали неверную дату (или в неверном формате). Пожалуйста, отправьте команду ещё раз с исправленной датой..
        /// </summary>
        internal static string BirthdaySetFailed {
            get {
                return ResourceManager.GetString("BirthdaySetFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to День рождения успешно добавлен в базу данных!.
        /// </summary>
        internal static string BirthdaySetSuccessfully {
            get {
                return ResourceManager.GetString("BirthdaySetSuccessfully", resourceCulture);
            }
        }
    }
}
