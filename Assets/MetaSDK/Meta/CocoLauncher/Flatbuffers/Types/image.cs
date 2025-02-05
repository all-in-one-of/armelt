// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace meta.types
{

using global::System;
using global::FlatBuffers;

public struct ImageType : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static ImageType GetRootAsImageType(ByteBuffer _bb) { return GetRootAsImageType(_bb, new ImageType()); }
  public static ImageType GetRootAsImageType(ByteBuffer _bb, ImageType obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public ImageType __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public meta.types.BufferHeader? Header { get { int o = __p.__offset(4); return o != 0 ? (meta.types.BufferHeader?)(new meta.types.BufferHeader()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  public int Rows { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Cols { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Type { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Channels { get { int o = __p.__offset(12); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public byte Data(int j) { int o = __p.__offset(14); return o != 0 ? __p.bb.Get(__p.__vector(o) + j * 1) : (byte)0; }
  public int DataLength { get { int o = __p.__offset(14); return o != 0 ? __p.__vector_len(o) : 0; } }
  public ArraySegment<byte>? GetDataBytes() { return __p.__vector_as_arraysegment(14); }

  public static Offset<ImageType> CreateImageType(FlatBufferBuilder builder,
      Offset<meta.types.BufferHeader> headerOffset = default(Offset<meta.types.BufferHeader>),
      int rows = 0,
      int cols = 0,
      int type = 0,
      int channels = 0,
      VectorOffset dataOffset = default(VectorOffset)) {
    builder.StartObject(6);
    ImageType.AddData(builder, dataOffset);
    ImageType.AddChannels(builder, channels);
    ImageType.AddType(builder, type);
    ImageType.AddCols(builder, cols);
    ImageType.AddRows(builder, rows);
    ImageType.AddHeader(builder, headerOffset);
    return ImageType.EndImageType(builder);
  }

  public static void StartImageType(FlatBufferBuilder builder) { builder.StartObject(6); }
  public static void AddHeader(FlatBufferBuilder builder, Offset<meta.types.BufferHeader> headerOffset) { builder.AddOffset(0, headerOffset.Value, 0); }
  public static void AddRows(FlatBufferBuilder builder, int rows) { builder.AddInt(1, rows, 0); }
  public static void AddCols(FlatBufferBuilder builder, int cols) { builder.AddInt(2, cols, 0); }
  public static void AddType(FlatBufferBuilder builder, int type) { builder.AddInt(3, type, 0); }
  public static void AddChannels(FlatBufferBuilder builder, int channels) { builder.AddInt(4, channels, 0); }
  public static void AddData(FlatBufferBuilder builder, VectorOffset dataOffset) { builder.AddOffset(5, dataOffset.Value, 0); }
  public static VectorOffset CreateDataVector(FlatBufferBuilder builder, byte[] data) { builder.StartVector(1, data.Length, 1); for (int i = data.Length - 1; i >= 0; i--) builder.AddByte(data[i]); return builder.EndVector(); }
  public static void StartDataVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(1, numElems, 1); }
  public static Offset<ImageType> EndImageType(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ImageType>(o);
  }
  public static void FinishImageTypeBuffer(FlatBufferBuilder builder, Offset<ImageType> offset) { builder.Finish(offset.Value); }
};


}
