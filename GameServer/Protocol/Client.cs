// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: GameServer/Protocol/client.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Protocol {

  /// <summary>Holder for reflection information generated from GameServer/Protocol/client.proto</summary>
  public static partial class ClientReflection {

    #region Descriptor
    /// <summary>File descriptor for GameServer/Protocol/client.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ClientReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiBHYW1lU2VydmVyL1Byb3RvY29sL2NsaWVudC5wcm90bxIIcHJvdG9jb2wi",
            "XAoFU0hlYWQSIAoFbXNnaWQYASABKA4yES5wcm90b2NvbC5FT3Bjb2RlEg4K",
            "BnJlcXNlcRgCIAEoDRIhCgdlcnJjb2RlGAMgASgOMhAucHJvdG9jb2wuRUVy",
            "cm5vIiMKB1Rlc3RSZXESDAoEbmFtZRgBIAEoCRIKCgJpZBgCIAEoDSIjCgdU",
            "ZXN0UnNwEgwKBG5hbWUYASABKAkSCgoCaWQYAiABKA0qgAEKB0VPcGNvZGUS",
            "EwoPRU9QQ09ERV9JTlZBSUxEEAASGQoVRU9QQ09ERV9IRUFSVEJFQVRfUkVR",
            "EAESGQoVRU9QQ09ERV9IRUFSVEJFQVRfUlNQEAISFAoQRU9QQ09ERV9UZXN0",
            "X1JFURADEhQKEEVPUENPREVfVGVzdF9SU1AQBCobCgZFRXJybm8SEQoNRUNT",
            "RVJSTk9fU1VDQxAAYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Protocol.EOpcode), typeof(global::Protocol.EErrno), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Protocol.SHead), global::Protocol.SHead.Parser, new[]{ "Msgid", "Reqseq", "Errcode" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protocol.TestReq), global::Protocol.TestReq.Parser, new[]{ "Name", "Id" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Protocol.TestRsp), global::Protocol.TestRsp.Parser, new[]{ "Name", "Id" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum EOpcode {
    /// <summary>
    /// 无效协议
    /// </summary>
    [pbr::OriginalName("EOPCODE_INVAILD")] Invaild = 0,
    /// <summary>
    /// 心跳请求
    /// </summary>
    [pbr::OriginalName("EOPCODE_HEARTBEAT_REQ")] HeartbeatReq = 1,
    /// <summary>
    /// 心跳应答
    /// </summary>
    [pbr::OriginalName("EOPCODE_HEARTBEAT_RSP")] HeartbeatRsp = 2,
    [pbr::OriginalName("EOPCODE_Test_REQ")] TestReq = 3,
    [pbr::OriginalName("EOPCODE_Test_RSP")] TestRsp = 4,
  }

  public enum EErrno {
    /// <summary>
    /// 操作成功
    /// </summary>
    [pbr::OriginalName("ECSERRNO_SUCC")] EcserrnoSucc = 0,
  }

  #endregion

  #region Messages
  public sealed partial class SHead : pb::IMessage<SHead>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SHead> _parser = new pb::MessageParser<SHead>(() => new SHead());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<SHead> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protocol.ClientReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SHead() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SHead(SHead other) : this() {
      msgid_ = other.msgid_;
      reqseq_ = other.reqseq_;
      errcode_ = other.errcode_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SHead Clone() {
      return new SHead(this);
    }

    /// <summary>Field number for the "msgid" field.</summary>
    public const int MsgidFieldNumber = 1;
    private global::Protocol.EOpcode msgid_ = global::Protocol.EOpcode.Invaild;
    /// <summary>
    /// 消息ID
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Protocol.EOpcode Msgid {
      get { return msgid_; }
      set {
        msgid_ = value;
      }
    }

    /// <summary>Field number for the "reqseq" field.</summary>
    public const int ReqseqFieldNumber = 2;
    private uint reqseq_;
    /// <summary>
    /// 请求序号
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint Reqseq {
      get { return reqseq_; }
      set {
        reqseq_ = value;
      }
    }

    /// <summary>Field number for the "errcode" field.</summary>
    public const int ErrcodeFieldNumber = 3;
    private global::Protocol.EErrno errcode_ = global::Protocol.EErrno.EcserrnoSucc;
    /// <summary>
    /// 错误码(应答消息有效，请求消息和通知消息无效)
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Protocol.EErrno Errcode {
      get { return errcode_; }
      set {
        errcode_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as SHead);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(SHead other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Msgid != other.Msgid) return false;
      if (Reqseq != other.Reqseq) return false;
      if (Errcode != other.Errcode) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Msgid != global::Protocol.EOpcode.Invaild) hash ^= Msgid.GetHashCode();
      if (Reqseq != 0) hash ^= Reqseq.GetHashCode();
      if (Errcode != global::Protocol.EErrno.EcserrnoSucc) hash ^= Errcode.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Msgid != global::Protocol.EOpcode.Invaild) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Msgid);
      }
      if (Reqseq != 0) {
        output.WriteRawTag(16);
        output.WriteUInt32(Reqseq);
      }
      if (Errcode != global::Protocol.EErrno.EcserrnoSucc) {
        output.WriteRawTag(24);
        output.WriteEnum((int) Errcode);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Msgid != global::Protocol.EOpcode.Invaild) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Msgid);
      }
      if (Reqseq != 0) {
        output.WriteRawTag(16);
        output.WriteUInt32(Reqseq);
      }
      if (Errcode != global::Protocol.EErrno.EcserrnoSucc) {
        output.WriteRawTag(24);
        output.WriteEnum((int) Errcode);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Msgid != global::Protocol.EOpcode.Invaild) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Msgid);
      }
      if (Reqseq != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(Reqseq);
      }
      if (Errcode != global::Protocol.EErrno.EcserrnoSucc) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Errcode);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(SHead other) {
      if (other == null) {
        return;
      }
      if (other.Msgid != global::Protocol.EOpcode.Invaild) {
        Msgid = other.Msgid;
      }
      if (other.Reqseq != 0) {
        Reqseq = other.Reqseq;
      }
      if (other.Errcode != global::Protocol.EErrno.EcserrnoSucc) {
        Errcode = other.Errcode;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Msgid = (global::Protocol.EOpcode) input.ReadEnum();
            break;
          }
          case 16: {
            Reqseq = input.ReadUInt32();
            break;
          }
          case 24: {
            Errcode = (global::Protocol.EErrno) input.ReadEnum();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            Msgid = (global::Protocol.EOpcode) input.ReadEnum();
            break;
          }
          case 16: {
            Reqseq = input.ReadUInt32();
            break;
          }
          case 24: {
            Errcode = (global::Protocol.EErrno) input.ReadEnum();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class TestReq : pb::IMessage<TestReq>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TestReq> _parser = new pb::MessageParser<TestReq>(() => new TestReq());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TestReq> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protocol.ClientReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestReq() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestReq(TestReq other) : this() {
      name_ = other.name_;
      id_ = other.id_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestReq Clone() {
      return new TestReq(this);
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 2;
    private uint id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TestReq);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TestReq other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Id != other.Id) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Id != 0) hash ^= Id.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Id != 0) {
        output.WriteRawTag(16);
        output.WriteUInt32(Id);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Id != 0) {
        output.WriteRawTag(16);
        output.WriteUInt32(Id);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Id != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(Id);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TestReq other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            Id = input.ReadUInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            Id = input.ReadUInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class TestRsp : pb::IMessage<TestRsp>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TestRsp> _parser = new pb::MessageParser<TestRsp>(() => new TestRsp());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TestRsp> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Protocol.ClientReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestRsp() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestRsp(TestRsp other) : this() {
      name_ = other.name_;
      id_ = other.id_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestRsp Clone() {
      return new TestRsp(this);
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 1;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 2;
    private uint id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TestRsp);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TestRsp other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Name != other.Name) return false;
      if (Id != other.Id) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Id != 0) hash ^= Id.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Id != 0) {
        output.WriteRawTag(16);
        output.WriteUInt32(Id);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Name.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Name);
      }
      if (Id != 0) {
        output.WriteRawTag(16);
        output.WriteUInt32(Id);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Id != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(Id);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TestRsp other) {
      if (other == null) {
        return;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            Id = input.ReadUInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            Name = input.ReadString();
            break;
          }
          case 16: {
            Id = input.ReadUInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
