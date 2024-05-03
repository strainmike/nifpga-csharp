using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace NationalInstruments.NiFpga
{
    public enum DataType
    {
        Bool = 1,
        I8 = 2,
        U8 = 3,
        I16 = 4,
        U16 = 5,
        I32 = 6,
        U32 = 7,
        I64 = 8,
        U64 = 9,
        Sgl = 10,
        Dbl = 11,
        Fxp = 12,
        Cluster = 13
    }

    public static class DataTypeExtensions
    {
        public static Type ToCType(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Bool:
                case DataType.U8:
                case DataType.Fxp:
                case DataType.Cluster:
                    return typeof(byte);
                case DataType.I8:
                    return typeof(sbyte);
                case DataType.I16:
                    return typeof(short);
                case DataType.U16:
                    return typeof(ushort);
                case DataType.I32:
                    return typeof(int);
                case DataType.U32:
                    return typeof(uint);
                case DataType.I64:
                    return typeof(long);
                case DataType.U64:
                    return typeof(ulong);
                case DataType.Sgl:
                    return typeof(float);
                case DataType.Dbl:
                    return typeof(double);
                default:
                    throw new ArgumentException("Invalid DataType");
            }
        }

        public static bool IsSigned(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.I8:
                case DataType.I16:
                case DataType.I32:
                case DataType.I64:
                case DataType.Sgl:
                case DataType.Dbl:
                    return true;
                default:
                    return false;
            }
        }
    }


    public abstract class FpgaType
    {
        public string Name { get; }

        public abstract DataType DataType { get; }

        public abstract int SizeInBits { get; }

        public abstract bool IsCApiType { get; }

        public abstract int NumberOfElements { get; }

        protected FpgaType(string name)
        {
            Name = name ?? string.Empty;
        }

        public abstract dynamic UnpackData(uint[] data, int bitIndex);

        public abstract void PackData(dynamic value, uint[] data, int bitIndex);
    }

    public class FpgaBool : FpgaType
    {
        public FpgaBool(string name) : base(name)
        {
        }

        public override DataType DataType => DataType.Bool;

        public override int SizeInBits => 1;

        public override bool IsCApiType => true;

        public override int NumberOfElements => 1;

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            int uintIndex = bitIndex / 32;
            int bitPosition = bitIndex % 32;
            return ((data[uintIndex] >> bitPosition) & 1) == 1;
        }

        public override void PackData(dynamic dataToPack, uint[] packedData, int bitIndex)
        {
            int uintIndex = bitIndex / 32;
            int bitPosition = bitIndex % 32;

            if ((bool)dataToPack)
            {
                packedData[uintIndex] |= (uint)(1 << bitPosition);
            }
            else
            {
                packedData[uintIndex] &= (uint)~(1 << bitPosition);
            }
        }
    }
    public class FpgaString : FpgaType
    {
        public FpgaString(string name) : base(name)
        {
        }

        public override DataType DataType => DataType.Cluster;

        public override int SizeInBits => 0;

        public override bool IsCApiType => false;

        public override int NumberOfElements => 1;

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return "";
        }

        public override void PackData(dynamic dataToPack, uint[] packedData, int bitIndex)
        {
            // don't pack anything for a string
        }
    }

    public abstract class FpgaNumeric : FpgaType
    {
        public override bool IsCApiType => true;

        public override int NumberOfElements => 1;

        public FpgaNumeric(string name) : base(name)
        {
        }
        public void PackSignedData(long value, uint[] data, int bitIndex)
        {
            int uintIndex = bitIndex / 32;
            int bitOffset = bitIndex % 32;

            long extendedValue = value;
            if (value < 0)
            {
                extendedValue |= ~((1L << SizeInBits) - 1);
            }

            for (int i = 0; i < SizeInBits; i += 32)
            {
                if (uintIndex + i / 32 < data.Length)
                {
                    data[uintIndex + i / 32] &= (uint)~(0xFFFFFFFF << bitOffset);
                    data[uintIndex + i / 32] |= (uint)((extendedValue & 0xFFFFFFFF) << bitOffset);

                    if (bitOffset > 0 && uintIndex + i / 32 + 1 < data.Length)
                    {
                        data[uintIndex + i / 32 + 1] &= (uint)~(0xFFFFFFFF >> (32 - bitOffset));
                        data[uintIndex + i / 32 + 1] |= (uint)(((uint)extendedValue) >> (32 - bitOffset));
                    }

                    extendedValue >>= 32;
                }
            }
        }

        public long UnpackSignedData(uint[] data, int bitIndex)
        {
            int uintIndex = bitIndex / 32;
            int nextUintIndex = uintIndex + 1;
            int bitOffset = bitIndex % 32;
            long value = 0;
            for (int i = 0; i < SizeInBits; i += 32)
            {
                if (uintIndex < data.Length)
                {
                    value |= (long)(data[uintIndex] >> bitOffset) << i;
                    if (bitOffset > 0 && nextUintIndex < data.Length)
                    {
                        value |= (long)(data[nextUintIndex] << (32 - bitOffset)) << i;
                    }
                }

                uintIndex++;
                nextUintIndex++;
            }
            // Sign extend if the value is negative
            if ((value & (1L << (SizeInBits - 1))) != 0)
            {
                value |= ~((1L << SizeInBits) - 1);
            }
            return value;
        }

        public void PackUnsignedData(ulong value, uint[] data, int bitIndex)
        {
            int uintIndex = (data.Length - 1) - bitIndex / 32;
            int bitOffset = bitIndex % 32;

            for (int i = 0; i < SizeInBits; i += 32, uintIndex--)
            {
                if (uintIndex >= 0)
                {
                    data[uintIndex] &= (uint)~(0xFFFFFFFF << bitOffset);
                    data[uintIndex] |= (uint)((value & 0xFFFFFFFF) << bitOffset);

                    if (bitOffset > 0 && uintIndex - 1 >= 0)
                    {
                        data[uintIndex - 1] &= (uint)~(0xFFFFFFFF >> (32 - bitOffset));
                        data[uintIndex - 1] |= (uint)(((uint)value) >> (32 - bitOffset));
                    }

                    value >>= 32;
                }
            }
        }

        public ulong UnpackUnsignedData(uint[] data, int bitIndex)
        {
            int uintIndex = (data.Length - 1) - bitIndex / 32;
            int bitOffset = bitIndex % 32;

            ulong value = 0;
            for (int i = 0; i < SizeInBits; i += 32, uintIndex--)
            {
                if (uintIndex >= 0)
                {
                    value |= (ulong)(data[uintIndex] >> bitOffset) << i;

                    if (bitOffset > 0 && uintIndex - 1 >= 0)
                    {
                        value |= (ulong)(data[uintIndex - 1] << (32 - bitOffset)) << i;
                    }
                }
            }

            return value;
        }
    }

    public class FpgaI8 : FpgaNumeric
    {
        public override DataType DataType => DataType.I8;

        public override int SizeInBits => 8;
        public override bool IsCApiType => true;

        public FpgaI8(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (sbyte)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((byte)value, data, bitIndex);
        }

    }

    public class FpgaI16 : FpgaNumeric
    {
        public override DataType DataType => DataType.I16;

        public override int SizeInBits => 16;
        public override bool IsCApiType => true;

        public FpgaI16(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (short)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((ushort)value, data, bitIndex);
        }
    }

    public class FpgaI32 : FpgaNumeric
    {
        public override DataType DataType => DataType.I32;

        public override int SizeInBits => 32;
        public override bool IsCApiType => true;

        public FpgaI32(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (int)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((uint)value, data, bitIndex);
        }
    }

    public class FpgaI64 : FpgaNumeric
    {
        public override DataType DataType => DataType.I64;

        public override int SizeInBits => 64;
        public override bool IsCApiType => true;

        public FpgaI64(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (long)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((ulong)value, data, bitIndex);
        }
    }

    public class FpgaU8 : FpgaNumeric
    {
        public override DataType DataType => DataType.U8;

        public override int SizeInBits => 8;
        public override bool IsCApiType => true;

        public FpgaU8(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (byte)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((byte)value, data, bitIndex);
        }
    }

    public class FpgaU16 : FpgaNumeric
    {
        public override DataType DataType => DataType.U16;

        public override int SizeInBits => 16;
        public override bool IsCApiType => true;

        public FpgaU16(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (ushort)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((ushort)value, data, bitIndex);
        }
    }


    public class FpgaU32 : FpgaNumeric
    {
        public override DataType DataType => DataType.U32;

        public override int SizeInBits => 32;
        public override bool IsCApiType => true;

        public FpgaU32(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (uint)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((uint)value, data, bitIndex);
        }
    }

    public class FpgaU64 : FpgaNumeric
    {
        public override DataType DataType => DataType.U64;

        public override int SizeInBits => 64;
        public override bool IsCApiType => true;

        public FpgaU64(string name) : base(name)
        {
        }

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            return (ulong)UnpackUnsignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackUnsignedData((ulong)value, data, bitIndex);
        }
    }

    public class FpgaArray : FpgaType
    {

        private FpgaType _subType;
        private int _size;
        private int _sizeInBits;

        public FpgaArray(string name, XElement typeXml) : base(name)
        {
            // Assuming BaseType constructor takes a string name
            var typeElement = typeXml.Element("Type");
            if (typeElement != null)
            {
                var subTypeElement = typeElement.Elements().FirstOrDefault();
                if (subTypeElement != null)
                {
                    _subType = Bitfile.ParseType(subTypeElement);
                }
            }
            _size = int.Parse(typeXml.Element("Size").Value);
            _sizeInBits = _subType.SizeInBits * _size;
        }

        public override int NumberOfElements => _size;

        public override int SizeInBits => _sizeInBits;

        public override bool IsCApiType => _subType.IsCApiType;

        public override DataType DataType => _subType.DataType;


        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            var results = new dynamic[_size]; // TODO try something like     Array results = Array.CreateInstance(_subType.DataType, _size);
            for (int i = 0; i < _size; i++)
            {
                results[i] = _subType.UnpackData(data, bitIndex);
                bitIndex += _subType.SizeInBits;
            }
            Array.Reverse(results);
            return results;
        }

        public override void PackData(dynamic dataToPack, uint[] packedData, int bitIndex)
        {
            Array.Reverse(dataToPack);
            for (int i = 0; i < _size; i++)
            {
                _subType.PackData(dataToPack[i], packedData, bitIndex);
                bitIndex += _subType.SizeInBits;
            }
        }
    }

    public class FpgaCluster : FpgaType
    {
        private List<FpgaType> _children;
        private int _sizeInBits;

        public FpgaCluster(string name, XElement typeXml) : base(name)
        {
            var memberTypes = typeXml.Element("TypeList");
            if (memberTypes != null)
            {
                _children = new List<FpgaType>();
                var names = new HashSet<string>();
                foreach (var child in memberTypes.Elements())
                {
                    var childType = Bitfile.ParseType(child);
                    if (names.Contains(childType.Name))
                    {
                        throw new Exception($"Cluster: '{Name}', contains multiple members with the name: '{childType.Name}'");
                    }
                    names.Add(childType.Name);
                    _children.Add(childType);
                }
                _sizeInBits = _children.Sum(child => child.SizeInBits);
            }
            else
            {
                var overflowEnabledXml = typeXml.Element("IncludeOverflowStatus");
                bool overflowEnabled = overflowEnabledXml != null && overflowEnabledXml.Value.ToLower() == "true";
                var typeName = typeXml.Name.LocalName;
                if (typeName == "FXP" && overflowEnabled)
                {
                    _children = new List<FpgaType>
                    {
                        new FpgaFixedPoint(name, typeXml),
                        new FpgaBool("OverflowStatus"),
                    };
                    _sizeInBits = _children.Sum(child => child.SizeInBits);
                }
                else
                {
                    throw new Exception($"Unsupported type: {typeName}");
                }
            }
        }

        public override DataType DataType => DataType.Cluster;

        public override int SizeInBits => _sizeInBits;

        public override bool IsCApiType => false;

        public override int NumberOfElements => 1;

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            var result = new OrderedDictionary();
            foreach (var child in Enumerable.Reverse(_children))
            {
                result[child.Name] = child.UnpackData(data, bitIndex);
                bitIndex += child.SizeInBits;
            }
            return result;
        }

        public override void PackData(dynamic dataToPack, uint[] packedData, int bitIndex)
        {
            foreach (var child in Enumerable.Reverse(_children))
            {
                child.PackData(dataToPack[child.Name], packedData, bitIndex);
                bitIndex += child.SizeInBits;
            }
        }
    }

    public class FixedPointTypeInfo
    {
        public int IntegerWordLength { get; set; }
        public int WordLength { get; set; }
        public bool IsSigned { get; set; }
    }

    public class FixedPoint
    {
        public static double CalculateFxpDeltaDouble(FixedPointTypeInfo typeInfo)
        {
            int exponent = typeInfo.IntegerWordLength - typeInfo.WordLength;
            return 2.2204460492503131e-16 * Math.Pow(2, exponent + 53 - 1);
        }

        public static double ConvertFromFxpToDouble(FixedPointTypeInfo typeInfo, ulong data)
        {
            double delta = CalculateFxpDeltaDouble(typeInfo);
            return FxpToFloatingPoint(typeInfo, delta, data);
        }

        private static double FxpToFloatingPoint(FixedPointTypeInfo typeInfo, double delta, ulong data)
        {
            ulong wordLengthMask = typeInfo.WordLength != 64
                ? (1UL << (int)typeInfo.WordLength) - 1 : 0xFFFFFFFFFFFFFFFFUL;
            data &= wordLengthMask;
            if (typeInfo.IsSigned)
            {
                ulong signedMask = 1UL << (int)(typeInfo.WordLength - 1);
                if ((data & signedMask) != 0)
                {
                    long signedData = (long)(data ^ wordLengthMask);
                    signedData = (signedData + 1) * -1;
                    return delta * signedData;
                }
            }
            return delta * data;
        }

        public static ulong ConvertFromDoubleToFixedPoint(FixedPointTypeInfo typeInfo, double data)
        {
            double delta = CalculateFxpDeltaDouble(typeInfo);
            return FloatingPointToFxp(typeInfo, delta, data);
        }

        private static ulong FloatingPointToFxp(FixedPointTypeInfo typeInfo, double delta, double data)
        {
            ulong wordLengthMask = typeInfo.WordLength != 64
                ? (1UL << typeInfo.WordLength) - 1 : 0xFFFFFFFFFFFFFFFFUL;
            if (data < 0)
            {
                if (typeInfo.IsSigned)
                {
                    long fxpRepresentation = (long)(data / delta);
                    fxpRepresentation ^= (long)wordLengthMask;
                    fxpRepresentation += 1;
                    fxpRepresentation *= -1;
                    if ((fxpRepresentation & (long)wordLengthMask) == fxpRepresentation)
                    {
                        return (ulong)fxpRepresentation;
                    }
                    else /* minimum */
                    {
                        return (ulong)((-1L * (1L << (typeInfo.WordLength - 1))) & (long)wordLengthMask);
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                ulong fxpRepresentation = (ulong)(data / delta);

                if ((fxpRepresentation & wordLengthMask) == fxpRepresentation)
                {
                    return fxpRepresentation;
                }
                else /* maximum */
                {
                    ulong magnitude = (ulong)(typeInfo.WordLength - (typeInfo.IsSigned ? 1 : 0));
                    return (1UL << (int)magnitude) - 1;
                }
            }
        }
    }

    public class FpgaFixedPoint : FpgaNumeric
    {
        private FixedPointTypeInfo typeInfo;
        private int _sizeInBits;

        public FpgaFixedPoint(string name, XElement typeXml) : base(name)
        {
            this.typeInfo = new FixedPointTypeInfo();
            var signedTag = typeXml.Element("Signed");
            this.typeInfo.IsSigned = signedTag.Value.ToLower() == "true";
            this.typeInfo.WordLength = int.Parse(typeXml.Element("WordLength").Value);
            this.typeInfo.IntegerWordLength = int.Parse(typeXml.Element("IntegerWordLength").Value);

            this._sizeInBits = this.typeInfo.WordLength;

        }

        public override DataType DataType => DataType.Fxp;

        public override int SizeInBits => this.typeInfo.WordLength;

        public override bool IsCApiType => true;

        public override int NumberOfElements => 1;

        public override dynamic UnpackData(uint[] data, int bitIndex)
        {
            ulong rawData = UnpackUnsignedData(data, bitIndex);
            return FixedPoint.ConvertFromFxpToDouble(typeInfo, rawData);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            var dataToPack = FixedPoint.ConvertFromDoubleToFixedPoint(typeInfo, value);
            PackUnsignedData(dataToPack, data, bitIndex);
        }
    }

    public class FpgaRegister
    {
        public string Name { get; }
        public uint Offset { get; }
        public bool IsIndicator { get; }
        public bool AccessMayTimeout { get; }
        public bool Internal { get; }
        public FpgaType Type { get; }
        public int NumElements { get; }

        public FpgaRegister(XElement regXml)
        {
            Name = regXml.Element("Name").Value;
            Offset = uint.Parse(regXml.Element("Offset").Value);
            IsIndicator = regXml.Element("Indicator").Value.ToLower() == "true";
            AccessMayTimeout = regXml.Element("AccessMayTimeout").Value.ToLower() == "true";
            Internal = regXml.Element("Internal").Value.ToLower() == "true";
            var datatype = regXml.Element("Datatype");
            Type = Bitfile.ParseType(datatype.Elements().First());
            NumElements = Type.NumberOfElements;
        }

        public int Length()
        {
            return NumElements;
        }


        public override string ToString()
        {
            return $"Register '{Name}'\n\tType: {Type.DataType}\n\tNum Elements: {Length()}\n\tOffset: {Offset}\n";
        }

    }


    public class FpgaFifo
    {
        public string Name { get; }
        public uint Number { get; }
        public bool Direction { get; }
        public FpgaType Type { get; }

        public FpgaFifo(XElement channelXml)
        {
            Name = channelXml.Attribute("name").Value;
            Number = uint.Parse(channelXml.Element("Number").Value);
            var dataTypeXml = channelXml.Element("DataType");
            if (dataTypeXml.Element("SubType") != null)
            {
                Type = Bitfile.ParseType(dataTypeXml);
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

    }

    public class Bitfile
    {
        private string? _filepath;
        private string _signature;
        public uint BaseAddressOnDevice;
        public List<FpgaRegister> Registers;
        public List<FpgaFifo> Fifos;


        public Bitfile(string filepath, bool parseContents = false)
        {
            XElement tree;
            if (parseContents)
            {
                _filepath = null;
                tree = XElement.Parse(filepath);
            }
            else
            {
                _filepath = Path.GetFullPath(filepath);
                tree = XElement.Load(_filepath);
            }

            _signature = tree.Element("SignatureRegister").Value.ToUpper();

            var project = tree.Element("Project");
            var nifpga = project.Element("CompilationResultsTree")
                                .Element("CompilationResults")
                                .Element("NiFpga");
            BaseAddressOnDevice = uint.Parse(nifpga.Element("BaseAddressOnDevice").Value);

            Registers = new List<FpgaRegister>();
            foreach (var regXml in tree.Element("VI").Element("RegisterList").Elements())
            {
                try
                {
                    var reg = new FpgaRegister(regXml);
                    Registers.Add(reg);
                }
                catch (UnsupportedTypeError e)
                {
                    Console.WriteLine($"Skipping Register: {regXml.Element("Name").Value}, {e.Message}");
                }
                //catch (ClusterMustContainUniqueNames e)
                //{
                //    Console.WriteLine($"Skipping Register: {regXml.Element("Name").Value}, {e.Message}");
                //}
            }
            Fifos = new List<FpgaFifo>();
            foreach (var channelXml in nifpga.Element("DmaChannelAllocationList").Elements())
            {
                try
                {
                    var fifo = new FpgaFifo(channelXml);
                    Fifos.Add(fifo);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Skipping FIFO: {channelXml.Element("Name").Value}, {e.Message}");
                }
            }
        }

        public static FpgaType ParseType(XElement typeXml)
        {
            XElement type = typeXml.Element("SubType");
            string typeName;
            string name;

            if (type != null)
            {
                typeName = type.Value;
                name = "";
            }
            else
            {
                typeName = typeXml.Name.LocalName;
                name = typeXml.Element("Name").Value;
            }
            typeName = typeName.Replace("Enum", "");

            if (typeName == "FXP")
            {
                var overflowEnabledXml = typeXml.Element("IncludeOverflowStatus");
                bool overflowEnabled = overflowEnabledXml != null && overflowEnabledXml.Value.ToLower() == "true";
                if (overflowEnabled)
                {
                    return new FpgaCluster(name, typeXml);
                }
                else
                {
                    return new FpgaFixedPoint(name, typeXml);
                }
            }
            switch (typeName)
            {
                case "Boolean":
                    return new FpgaBool(name);
                case "Cluster":
                    return new FpgaCluster(name, typeXml);
                case "Array":
                    return new FpgaArray(name, typeXml);
                //case "SGL":
                //case "DBL":
                //    return new FpgaFloat(name, typeName);
                case "String":
                    // Strings are not supported on the FPGA, but show up in error clusters
                    return new FpgaString(name);
                case "I8":
                    return new FpgaI8(name);
                case "I16":
                    return new FpgaI16(name);
                case "I32":
                    return new FpgaI32(name);
                case "I64":
                    return new FpgaI64(name);
                case "U8":
                    return new FpgaU8(name);
                case "U16":
                    return new FpgaU16(name);
                case "U32":
                    return new FpgaU32(name);
                case "U64":
                    return new FpgaU64(name);
                default:
                    throw new UnsupportedTypeError($"The FPGA Interface C# API does not yet support {typeName}");
            }
        }
    }

    [Serializable]
    internal class UnsupportedTypeError : Exception
    {
        public UnsupportedTypeError()
        {
        }

        public UnsupportedTypeError(string? message) : base(message)
        {
        }

        public UnsupportedTypeError(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UnsupportedTypeError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}