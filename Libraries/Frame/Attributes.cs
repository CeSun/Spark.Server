using System;
using System.Collections.Generic;
using System.Text;

namespace Frame
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DispacherAttribute: Attribute
    {
        Enum msgId;
        public DispacherAttribute(ValueType MsgId)
        {
        }

    }

}
