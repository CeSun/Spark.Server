using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RouteAttribute<T> : Attribute
{
    public RouteAttribute(T ReqMsgId, T RspMsgId)
    {

    }

}
