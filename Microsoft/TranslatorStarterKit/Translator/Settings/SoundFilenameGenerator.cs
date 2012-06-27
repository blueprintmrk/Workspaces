
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
using System.IO.IsolatedStorage;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Generates a unique filename for a langauge.
    /// The format is:
    /// 
    ///    XX\phrNNNNNNNN.wav
    ///    
    /// Where:
    /// 
    ///    XX is the two-letter ISO langauge code
    ///    NNNNNNNNN is a unique number.
    ///    
    ///  Examples:
    ///  
    ///   en\phr1.wav
    ///   en\phr372.wav
    ///   fr\phr39.wav
    ///   
    /// </summary>
    public class SoundFilenameGenerator
    {
        /// <summary>
        /// If this is set to a non-empty value, it will override the generator.
        /// Used by testing code
        /// </summary>
        public static string ForceFileName = String.Empty;

        /// <summary>
        /// Next number for the file
        /// </summary>
        public uint NextFileNumber { get; set; }

        /// <summary>
        /// Generates a filename
        /// </summary>
        /// <param name="lang">The langauge</param>
        /// <returns>A unique filename</returns>
        public string GetNextFilename(string lang)
        {
            if (String.IsNullOrEmpty(ForceFileName) == false)
            {
                return ForceFileName;
            }

            string directory = lang;

            // Ensures the directory exists in ISO
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.DirectoryExists(directory) == false)
                {
                    store.CreateDirectory(directory);
                }
            }
            NextFileNumber++;
            return directory + @"\phr" + NextFileNumber.ToString() + ".wav";
        }
    }
}
