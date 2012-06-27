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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Globalization;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Provides Information about a certain language
    /// </summary>
    public class LanguageInformation
    {
        /// <summary>
        /// Underlying .NET CultureInfo object
        /// </summary>
        private CultureInfo _cultureInfo;
        /// <summary>
        /// The name of the langauge for the current UI culture. For example,
        /// "English" for US devices and "Anglias" for French devices.
        /// </summary>
        private string _nativeName;

        /// <summary>
        /// The name of the langauge for the current UI culture. For example,
        /// "English" for US devices and "Anglias" for French devices.
        /// </summary>
        public string ShortNativeName
        {
            get { return _nativeName; }
        }

        /// <summary>
        /// I.e. "en" for English
        /// </summary>
        public string TwoLetterISOLanguageName
        {
            get { return _cultureInfo.TwoLetterISOLanguageName; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="language"></param>
        public LanguageInformation(string language)
        {
            _cultureInfo = new CultureInfo(language);
            _nativeName = _cultureInfo.DisplayName;
            // The DisplayName is in the format "Langauge (country/region)" and all we care about is thhe
            // Lanugage part, so trim that toff.
            string[] split = _nativeName.Split(new char[] { '(' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0)
            {
                _nativeName = split[0].Trim();
            }
        }
    }
}
