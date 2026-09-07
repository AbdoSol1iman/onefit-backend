using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public string ErrorCode { get; }

        public NotFoundException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
