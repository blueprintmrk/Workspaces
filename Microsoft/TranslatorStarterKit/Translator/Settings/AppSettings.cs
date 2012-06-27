// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
using System.Globalization;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Top level data structure for the application settings
    /// </summary>
    public class AppSettings : IsoStoreAppSettings
    {
        const string cFromLangaugeCode = "FromLangaugeCode";
        const string cToLangaugeCode = "ToLangaugeCode";

        /// <summary>
        /// Two-letter ISO name for "from" langauge
        /// </summary>
        public string FromLangaugeCode
        {
            get
            {
                return base.GetValueOrDefault<string>(
                    cFromLangaugeCode, 
                    CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                    );
            }
            set
            {
                base.AddOrUpdateValue(cFromLangaugeCode, value);
            }
        }

        /// <summary>
        /// Two-letter ISO name for "to" langauge
        /// </summary>
        public string ToLangaugeCode
        {
            get
            {
                return base.GetValueOrDefault<string>(
                    cToLangaugeCode,
                    App.Model.FindDifferentLanguage(App.Model.FromLanguage).TwoLetterISOLanguageName
                    );
            }
            set
            {
                base.AddOrUpdateValue(cToLangaugeCode, value);
            }
        }
    }
}
