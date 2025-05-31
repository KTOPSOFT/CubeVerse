using System;
using System.Runtime.ExceptionServices;

namespace InsaneScatterbrain.Extensions
{
    public static class ExceptionExtensions
    {
        public static void Rethrow(this Exception e)
        {
            ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
        }
    }
}