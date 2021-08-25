using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public class ResponseException<ErrorCodeType>: Exception
    {
        public ErrorCodeType errorcode { get; private set; }
        public ResponseException(ErrorCodeType errorCodeType)
        {
            errorcode = errorCodeType;
        }
    }
}
