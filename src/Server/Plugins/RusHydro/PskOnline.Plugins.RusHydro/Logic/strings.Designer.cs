﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PskOnline.Server.Plugins.RusHydro.Logic {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PskOnline.Server.Plugins.RusHydro.Logic.strings", typeof(strings).Assembly);
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
        ///   Looks up a localized string similar to день.
        /// </summary>
        internal static string _day {
            get {
                return ResourceManager.GetString("_day", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ночь.
        /// </summary>
        internal static string _night {
            get {
                return ResourceManager.GetString("_night", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Внимание! В настройках ПО не выбрана папка для сохранения сводок! Сводки будут сохраняться в резервной папке {0}.
        /// </summary>
        internal static string BaseFolderForSummaryNotSpecified_Format {
            get {
                return ResourceManager.GetString("BaseFolderForSummaryNotSpecified_Format", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Невозможно подготовить сводку по результатам обследования. Обратитесь к системному администратору..
        /// </summary>
        internal static string CannotRenderSummaryInAnyFormat {
            get {
                return ResourceManager.GetString("CannotRenderSummaryInAnyFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Невозможно сохраниь сводку ни в основную папку, ни в резервные папки. Обратитесь к системному администратору и проверьте права на доступ и наличие свободного места в папке {0}
        ///
        ///Содержимое сводки:
        ///=====================
        ///{1}
        ///.
        /// </summary>
        internal static string CannotSaveSummaryToAnyFolder_Format {
            get {
                return ResourceManager.GetString("CannotSaveSummaryToAnyFolder_Format", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ошибка записи в папку {0}. Используем для сохранения сводки резервную папку {1}.
        /// </summary>
        internal static string ErrorCreateFolder_UseFallback_Format {
            get {
                return ResourceManager.GetString("ErrorCreateFolder_UseFallback_Format", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Критическое.
        /// </summary>
        internal static string LSR_0_Critical {
            get {
                return ResourceManager.GetString("LSR_0_Critical", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Негативное.
        /// </summary>
        internal static string LSR_1_Negative {
            get {
                return ResourceManager.GetString("LSR_1_Negative", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Предельно-допустимое.
        /// </summary>
        internal static string LSR_2_OnTheEdge {
            get {
                return ResourceManager.GetString("LSR_2_OnTheEdge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Допустимое.
        /// </summary>
        internal static string LSR_3_Acceptable {
            get {
                return ResourceManager.GetString("LSR_3_Acceptable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Близкое к оптимальному.
        /// </summary>
        internal static string LSR_4_NearOptimal {
            get {
                return ResourceManager.GetString("LSR_4_NearOptimal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Оптимальное.
        /// </summary>
        internal static string LSR_5_Optimal {
            get {
                return ResourceManager.GetString("LSR_5_Optimal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Состояние.
        /// </summary>
        internal static string LSR_State {
            get {
                return ResourceManager.GetString("LSR_State", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Сводки_ПСК.
        /// </summary>
        internal static string Rushydro_PSA_folder {
            get {
                return ResourceManager.GetString("Rushydro_PSA_folder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to yyyy.MM.dd HH:mm.
        /// </summary>
        internal static string SummaryText_DateFormat {
            get {
                return ResourceManager.GetString("SummaryText_DateFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ФИО:                .
        /// </summary>
        internal static string SummaryText_P0_Format_Name {
            get {
                return ResourceManager.GetString("SummaryText_P0_Format_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Должность:          .
        /// </summary>
        internal static string SummaryText_P1_1_Format_Position {
            get {
                return ResourceManager.GetString("SummaryText_P1_1_Format_Position", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Дата:               .
        /// </summary>
        internal static string SummaryText_P1_Format_Date {
            get {
                return ResourceManager.GetString("SummaryText_P1_Format_Date", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Рабочая станция:    .
        /// </summary>
        internal static string SummaryText_P2_Format_Workstation {
            get {
                return ResourceManager.GetString("SummaryText_P2_Format_Workstation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ВКР     .
        /// </summary>
        internal static string SummaryText_P3_Format_HRV {
            get {
                return ResourceManager.GetString("SummaryText_P3_Format_HRV", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ПЗМР    .
        /// </summary>
        internal static string SummaryText_P4_Format_SVMR {
            get {
                return ResourceManager.GetString("SummaryText_P4_Format_SVMR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Общий результат:    .
        /// </summary>
        internal static string SummaryText_P5_Format_Overall_Summary {
            get {
                return ResourceManager.GetString("SummaryText_P5_Format_Overall_Summary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ---------------------------------------------------------.
        /// </summary>
        internal static string SummaryText_Separator {
            get {
                return ResourceManager.GetString("SummaryText_Separator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ПЗМР: слишком много ошибок!.
        /// </summary>
        internal static string SvmrTooManyErrors {
            get {
                return ResourceManager.GetString("SvmrTooManyErrors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to предсменный контроль завершен, спасибо!.
        /// </summary>
        internal static string ThankYouAssessmentFinished {
            get {
                return ResourceManager.GetString("ThankYouAssessmentFinished", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Неизвестные ошибки!.
        /// </summary>
        internal static string UnknownError {
            get {
                return ResourceManager.GetString("UnknownError", resourceCulture);
            }
        }
    }
}
