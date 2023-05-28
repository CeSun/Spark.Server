using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RouteAttribute<T> : Attribute
{
    [SetsRequiredMembers]
    public RouteAttribute(T ReqMsgId, T RspMsgId)
    {
        this.ReqMsgId = ReqMsgId;
        this.RspMsgId = RspMsgId;
    }

    public required T ReqMsgId { get; set; }
    public required T RspMsgId { get; set; }

}
