using System;

namespace Boxofon.Web.Helpers
{
    public static class ExceptionHelpers
    {
        public static void ThrowIfNull(this object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}