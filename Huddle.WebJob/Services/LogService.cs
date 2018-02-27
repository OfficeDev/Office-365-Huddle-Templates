/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;

namespace Huddle.WebJob.Services
{
    public class LogService
    {
        public static void LogOperationStarted(string operation)
        {
            Console.WriteLine($"{operation} started.");
        }

        public static void LogOperationEnded(string operation)
        {
            Console.WriteLine($"{operation} ended.");
        }

        public static void LogInfo(string info)
        {
            Console.WriteLine(info);
        }

        public static void LogError(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}
