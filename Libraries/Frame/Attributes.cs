using System;
using System.Collections.Generic;
using System.Text;

namespace Frame
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ControllerAttribute: Attribute
    {
        int msgId;
        public ControllerAttribute(int MsgId)
        {
            this.msgId = MsgId;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DispatcherAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class FilterAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DispatchMethodAttribute : Attribute
    {
    }
}
