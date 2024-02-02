using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
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
        protected string Name { get; }

        public abstract DataType DataType { get; }

        public abstract int SizeInBits { get; }

        public abstract bool IsCApiType { get; }

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
            int bitOffset = bitIndex % 32;

            long value = 0;
            for (int i = 0; i < SizeInBits; i += 32)
            {
                if (uintIndex + i / 32 < data.Length)
                {
                    value |= (long)(data[uintIndex + i / 32] >> bitOffset) << i;

                    if (bitOffset > 0 && uintIndex + i / 32 + 1 < data.Length)
                    {
                        value |= (long)(data[uintIndex + i / 32 + 1] << (32 - bitOffset)) << i;
                    }
                }
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
            int uintIndex = bitIndex / 32;
            int bitOffset = bitIndex % 32;

            for (int i = 0; i < SizeInBits; i += 32, uintIndex++)
            {
                if (uintIndex < data.Length)
                {
                    data[uintIndex] &= (uint)~(0xFFFFFFFF << bitOffset);
                    data[uintIndex] |= (uint)((value & 0xFFFFFFFF) << bitOffset);

                    if (bitOffset > 0 && uintIndex + 1 < data.Length)
                    {
                        data[uintIndex + 1] &= (uint)~(0xFFFFFFFF >> (32 - bitOffset));
                        data[uintIndex + 1] |= (uint)(((uint)value) >> (32 - bitOffset));
                    }

                    value >>= 32;
                }
            }
        }

        public ulong UnpackUnsignedData(uint[] data, int bitIndex)
        {
            int uintIndex = bitIndex / 32;
            int bitOffset = bitIndex % 32;

            ulong value = 0;
            for (int i = 0; i < SizeInBits; i += 32, uintIndex++)
            {
                if (uintIndex < data.Length)
                {
                    value |= (ulong)(data[uintIndex] >> bitOffset) << i;

                    if (bitOffset > 0 && uintIndex + 1 < data.Length)
                    {
                        value |= (ulong)(data[uintIndex + 1] << (32 - bitOffset)) << i;
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
            return (sbyte)UnpackSignedData(data, bitIndex) ;
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackSignedData((sbyte)value, data, bitIndex);
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
            return (short)UnpackSignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackSignedData((short)value, data, bitIndex);
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
            return (int)UnpackSignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackSignedData((int)value, data, bitIndex);
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
            return (long)UnpackSignedData(data, bitIndex);
        }

        public override void PackData(dynamic value, uint[] data, int bitIndex)
        {
            PackSignedData((long)value, data, bitIndex);
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
            //NumElements = IsArray() ? Type.Size : 1;
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

    public class Bitfile
    {
        private string? _filepath;
        private string _signature;
        public uint BaseAddressOnDevice;
        public List<FpgaRegister> Registers;

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
            switch (typeName)
            {
                case "Boolean":
                    return new FpgaBool(name);
                //case "Cluster":
                //    return new FpgaCluster(name, typeXml);
                //case "FXP":
                //    return new FpgaFXP(name, typeXml);
                //case "Array":
                //    return new FpgaArray(name, typeXml);
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