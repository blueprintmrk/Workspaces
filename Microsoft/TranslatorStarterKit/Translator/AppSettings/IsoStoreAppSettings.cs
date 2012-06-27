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
using System.IO.IsolatedStorage;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Base class to manage saving and restoring settings in the Isolated Storage.
    /// The IsolatedStorageSettings.ApplicationSettings object is automatically
    /// saved and loaded by Silverlight when the application exits or loads.
    /// Since it is a dictionary, this class adds some helper functions to allow
    /// your code to more easily access the dictionary in a consistent fashion.
    /// 
    /// Typical usage of this class is to derive your Application specific settings
    /// and then use properties to access these methods, i.e.
    /// 
    /// public string Name
    /// {
    ///     get { return base.GetValueOrDefault<string>("Name", "Bill"); }
    ///     set { base.AddOrUpdateValue( "Name", value); }
    /// }
    /// 
    /// </summary>
    public class IsoStoreAppSettings
    {
        /// <summary>
        /// The Silverlight Isolated Storage Settings object, which will be lazily
        /// initlialized once the property is accessed
        /// </summary>
        private IsolatedStorageSettings _isolatedstore;

        /// <summary>
        /// The Silverlight Isolated Storage Settings object, which will be lazily
        /// initlialized once the property is accessed
        /// </summary>
        private IsolatedStorageSettings IsolatedSettings
        {
            get
            {
                if (_isolatedstore == null)
                {
                    _isolatedstore = IsolatedStorageSettings.ApplicationSettings;
                }
                return _isolatedstore;
            }
        }

        /// <summary>
        /// Sets a value in the App Settings, overwriting an existing setting if
        /// applicable
        /// </summary>
        /// <param name="key">Keyname of the setting</param>
        /// <param name="value">Value of the setting</param>
        /// <returns>True if the value was changed</returns>
        public bool AddOrUpdateValue( string key, Object value)
        {
            bool valueChanged = false;
            if (IsolatedSettings.Contains(key))
            {
                if (IsolatedSettings[key] != value)
                {
                    // set the value if it is different than what is in settings
                    IsolatedSettings[key] = value;
                    valueChanged = true;
                }
        	}
            else
            {
                IsolatedSettings.Add(key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Retrieves a value from the App Settings. If the value does not
        /// exist in the settings, the defaultValue is returned.
        /// </summary>
        /// <typeparam name="T">Type desired</typeparam>
        /// <param name="Key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>The value, or the default value if not found</returns>
        public T GetValueOrDefault<T>(string key, T defaultValue)
        {
            T value;
            if (IsolatedSettings.Contains(key))
            {
                value = (T)IsolatedSettings[key];
            }
            else
            {
                value = defaultValue;
            }

            return value; 
        }

        /// <summary>
        /// Explicit Save.  Not needed as Save will automatically be called at appInstance exit
        /// </summary>
        public void Save()
        {
            IsolatedSettings.Save();
        }
    }
}
