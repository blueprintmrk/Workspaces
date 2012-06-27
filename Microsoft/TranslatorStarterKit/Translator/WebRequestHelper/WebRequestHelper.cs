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
using System.IO;
using System.Net;

namespace Microsoft.Phone.Applications.Translator
{
    /// <summary>
    /// Stateless Helper class to handle an asynchronous web request
    /// </summary>
    public static class WebRequestHelper
    {
        /// <summary>
        /// Supported types of data response
        /// </summary>
        public enum DataResponseTypes
        {
            /// <summary>
            /// A string response (such as XML)
            /// </summary>
            String,
            /// <summary>
            /// A response of byte(s)
            /// </summary>
            Byte,
        }

        /// <summary>
        /// Metadata sent on the request
        /// </summary>
        public class RequestInfo
        {
            /// <summary>
            /// the HttpWebRequest
            /// </summary>
            public HttpWebRequest Request;
            /// <summary>
            /// Supported types of data response
            /// </summary>
            public DataResponseTypes DataResponseType;
            /// <summary>
            /// Action to call when the HttpWebRequest has been sent
            /// </summary>
            public Action Sent;
            /// <summary>
            /// Action to call when the HttpWebReqeust returns with a string
            /// </summary>
            public Action<string> ReceivedString;
            /// <summary>
            /// Action to call when the HttpWebReqeust returns with byte(s)
            /// </summary>
            public Action<byte[]> ReceivedBytes;
            /// <summary>
            /// Action to call when the HttpWebReqeust fails
            /// </summary>
            public Action<string> Failed;
            /// <summary>
            /// Set to true if the request was cancelled
            /// </summary>
            public bool Cancelled = false;

            /// <summary>
            /// Starts the call
            /// </summary>
            public void BeginGetResponse()
            {
                Request.BeginGetResponse(new AsyncCallback(OnGetResponseCompleted), this);
            }
        }

        /// <summary>
        /// Sends an async HttpWebRequest with the uri to request a string response
        /// </summary>
        /// <param name="uriString">URI</param>
        /// <param name="sent">Action to call when the URI is sent</param>
        /// <param name="received">Action to call when the string is received</param>
        /// <param name="failed">Action to call when the request fails</param>
        /// <returns>A RequestInfo object that can be used to track/cancel the request</returns>
        public static RequestInfo SendStringRequest(
            string uriString,
            Action sent,
            Action<string> received,
            Action<string> failed
        )
        {
            RequestInfo requestInfo =
                CreateRequestInfo(
                    uriString,
                    sent,
                    failed
                    );

            requestInfo.DataResponseType = DataResponseTypes.String;
            requestInfo.ReceivedString = received;
            requestInfo.BeginGetResponse();
            requestInfo.Sent();

            return requestInfo;
        }

        /// <summary>
        /// Sends an async HttpWebRequest with the uri to request a binary (byte(s)) response
        /// </summary>
        /// <param name="uriString">URI</param>
        /// <param name="sent">Action to call when the URI is sent</param>
        /// <param name="received">Action to call when the byte(s) is received</param>
        /// <param name="failed">Action to call when the request fails</param>
        /// <returns>A RequestInfo object that can be used to track/cancel the request</returns>
        public static RequestInfo SendByteRequest(
            string uriString,
            Action sent,
            Action<byte[]> received,
            Action<string> failed
        )
        {
            RequestInfo requestInfo =
                CreateRequestInfo(
                    uriString,
                    sent,
                    failed
                    );

            requestInfo.DataResponseType = DataResponseTypes.Byte;
            requestInfo.ReceivedBytes = received;
            requestInfo.BeginGetResponse();
            requestInfo.Sent();

            return requestInfo;
        }

        /// <summary>
        /// Builds the metadata, and the actions that are common to all data types
        /// </summary>
        /// <param name="uriString">Uri</param>
        /// <param name="sent">Sent action</param>
        /// <param name="failed">Failed action</param>
        /// <returns>A RequestInfo object that can be used to track/cancel the request</returns>
        private static RequestInfo CreateRequestInfo(
            string uriString,
            Action sent,
            Action<string> failed
            )
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(uriString));
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.Request = request;
            requestInfo.DataResponseType = DataResponseTypes.String;
            requestInfo.Sent = sent;
            requestInfo.Failed = failed;

            return requestInfo;
        }

        /// <summary>
        /// Called when the request returns asynchrounously
        /// </summary>
        /// <param name="ar"></param>
        private static void OnGetResponseCompleted(IAsyncResult ar)
        {
            RequestInfo requestInfo = (RequestInfo)ar.AsyncState;

            try
            {
                //
                // Fetch the metaData
                //
                HttpWebResponse response = (HttpWebResponse)requestInfo.Request.EndGetResponse(ar);

                if (requestInfo.Cancelled != true)
                {
                    switch (requestInfo.DataResponseType)
                    {
                        case DataResponseTypes.String:

                            using (StreamReader stringReader = new StreamReader(response.GetResponseStream()))
                            {
                                requestInfo.ReceivedString(stringReader.ReadToEnd());
                            }
                            break;

                        case DataResponseTypes.Byte:

                            Stream byteReader = response.GetResponseStream();
                            byte[] bytes = new byte[byteReader.Length];
                            long count = byteReader.Read(bytes, 0, (int)byteReader.Length);
                            requestInfo.ReceivedBytes(bytes);
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                requestInfo.Failed(ex.Message);
            }
        }
    }


}
