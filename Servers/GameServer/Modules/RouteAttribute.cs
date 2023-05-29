using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;
namespace Frame.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RouteAttribute : Attribute
{
    [SetsRequiredMembers]
    public RouteAttribute(Protocol.MsgId ReqMsgId, Protocol.MsgId RspMsgId)
    {
        {
            this.ReqMsgId = ReqMsgId;
            this.RspMsgId = RspMsgId;
        }
    }

    public required Protocol.MsgId ReqMsgId { get; set; } 
    public required Protocol.MsgId RspMsgId { get; set; } 

}