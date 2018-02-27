/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using System.Runtime.Serialization;

namespace Huddle.BotWebApp.Dialogs
{
    [Serializable]
    public class SignTimeoutException : TimeoutException
    {
        public SignTimeoutException() { }

        public SignTimeoutException(string message) : base(message) { }

        public SignTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected SignTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ActionCancelledException : Exception
    {
        public ActionCancelledException() { }

        public ActionCancelledException(string message) : base(message) { }

        public ActionCancelledException(string message, Exception inner) : base(message, inner) { }

        protected ActionCancelledException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
