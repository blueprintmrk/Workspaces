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

using Microsoft.Phone.Applications.Translator.LocalizationStrings;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Provides a way around that "internal" constructor in the auto-gen resources.designer.cs file
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public class Resources 
    {
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Resources() : base()
        {
            Strings = new Strings();
        }

        public Strings Strings { get; internal set; }
    }
}

