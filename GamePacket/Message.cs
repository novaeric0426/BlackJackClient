// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace GamePacket
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct Message : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_3_3(); }
  public static Message GetRootAsMessage(ByteBuffer _bb) { return GetRootAsMessage(_bb, new Message()); }
  public static Message GetRootAsMessage(ByteBuffer _bb, Message obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Message __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string Data { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetDataBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
  public ArraySegment<byte>? GetDataBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetDataArray() { return __p.__vector_as_array<byte>(4); }
  public GamePacket.CardData? Card { get { int o = __p.__offset(6); return o != 0 ? (GamePacket.CardData?)(new GamePacket.CardData()).__assign(o + __p.bb_pos, __p.bb) : null; } }
  public string Func { get { int o = __p.__offset(8); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetFuncBytes() { return __p.__vector_as_span<byte>(8, 1); }
#else
  public ArraySegment<byte>? GetFuncBytes() { return __p.__vector_as_arraysegment(8); }
#endif
  public byte[] GetFuncArray() { return __p.__vector_as_array<byte>(8); }

  public static void StartMessage(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddData(FlatBufferBuilder builder, StringOffset dataOffset) { builder.AddOffset(0, dataOffset.Value, 0); }
  public static void AddCard(FlatBufferBuilder builder, Offset<GamePacket.CardData> cardOffset) { builder.AddStruct(1, cardOffset.Value, 0); }
  public static void AddFunc(FlatBufferBuilder builder, StringOffset funcOffset) { builder.AddOffset(2, funcOffset.Value, 0); }
  public static Offset<GamePacket.Message> EndMessage(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<GamePacket.Message>(o);
  }
  public static void FinishMessageBuffer(FlatBufferBuilder builder, Offset<GamePacket.Message> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedMessageBuffer(FlatBufferBuilder builder, Offset<GamePacket.Message> offset) { builder.FinishSizePrefixed(offset.Value); }
}


}