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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Logging;
using Microsoft.Phone.Shell;

namespace Microsoft.Phone.Applications.Translator
{
    public class PhoneUtils
    {
        public static object TryGetValue(IDictionary<string, object> state, string key, object defaultValue)
        {
            object returnValue = null;

            if (state.TryGetValue(key, out returnValue) == false)
            {
                returnValue = defaultValue;
            }

            return returnValue;
        }

        public static TValue TryGetValue<TValue>(IDictionary<string, object> state, string key, TValue defaultValue)
        {
            object returnValue;

            if (state.TryGetValue(key, out returnValue) == false)
            {
                returnValue = defaultValue;
            }

            return (TValue)returnValue;
        }
    }
}
