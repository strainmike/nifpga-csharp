using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace NationalInstruments.NiFpga
{

    internal static class NativeMethods
    {
        const string DLL_NAME = "NiFpga.dll";


        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NiFpgaDll_Open(
            [MarshalAs(UnmanagedType.LPStr)] string bitfile,
            [MarshalAs(UnmanagedType.LPStr)] string signature,
            [MarshalAs(UnmanagedType.LPStr)] string resource,
            UInt32 attribute,
            out int session);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_Close(int session, UInt32 attribute);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_Run(int session, UInt32 attribute);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_Abort(int session);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_Reset(int session);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_Download(int session);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_GetFpgaViState(int session, out UInt32 state);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadArrayU32(int session, uint indicator, uint[] values, int size);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteArrayU32(int session, uint control, uint[] values, int size);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoU8(
            int session,
            UInt32 fifo,
            byte[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoU16(
            int session,
            UInt32 fifo,
            ushort[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoU32(
            int session,
            UInt32 fifo,
            uint[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoU64(
            int session,
            UInt32 fifo,
            ulong[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoI8(
            int session,
            UInt32 fifo,
            sbyte[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoI16(
            int session,
            UInt32 fifo,
            short[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoI32(
            int session,
            UInt32 fifo,
            int[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ReadFifoI64(
            int session,
            UInt32 fifo,
            long[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoU8(
            int session,
            UInt32 fifo,
            byte[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoU16(
            int session,
            UInt32 fifo,
            ushort[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoU32(
            int session,
            UInt32 fifo,
            uint[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoU64(
            int session,
            UInt32 fifo,
            ulong[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoI8(
            int session,
            UInt32 fifo,
            sbyte[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoI16(
            int session,
            UInt32 fifo,
            short[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoI32(
            int session,
            UInt32 fifo,
            int[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_WriteFifoI64(
            int session,
            UInt32 fifo,
            long[] data,
            nuint count,
            UInt32 timeout,
            out nuint remaining);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_ConfigureFifo2(
            int session,
            UInt32 fifo,
            UIntPtr requestedDepth,
            out UIntPtr actualDepth);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_StartFifo(
            int session,
            UInt32 fifo);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NiFpgaDll_StopFifo(
            int session,
            UInt32 fifo);
    }

    public enum FpgaViState
    {
        NotRunning = 0,
        Invalid = 1,
        Running = 2,
        NaturallyStopped = 3
    }

    public class NiFpgaException : Exception
    {
        public int ErrorCode { get; }

        public NiFpgaException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public NiFpgaException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public static void ThrowIfFatal(Int32 status, string message="")
        {
            if (status < 0)
            {
                throw new NiFpgaException(status, $"Error: {status}: {message}");
            }
        }
    }

    public class Session : IDisposable
    {
        internal int _session;
        private bool _disposed = false;
        private Bitfile _bitfile;
        private Dictionary<string, Register> _internalRegisters;
        public Dictionary<string, Register> Registers;

        public Dictionary<string, Fifo> Fifos;

        public Session(string bitfile, string resource)
        {
            _bitfile = new Bitfile(bitfile);
            uint attribute = (uint)1 << 31;
            var status = NativeMethods.NiFpgaDll_Open(bitfile, null, resource, attribute, out _session);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_Open errored");

            try
            {
                Registers = new Dictionary<string, Register>();
                _internalRegisters = new Dictionary<string, Register>();
                foreach (var bitfileRegister in _bitfile.Registers)
                {
                    var register = new Register(this, bitfileRegister, _bitfile.BaseAddressOnDevice);
                    if (bitfileRegister.Internal)
                    {
                        if (_internalRegisters.ContainsKey(register.Name))
                        {
                            System.Diagnostics.Debug.WriteLine($"Warning: Duplicate internal register name '{register.Name}' found. Skipping this register.");
                            continue;
                        }
                        _internalRegisters.Add(register.Name, register);
                    }
                    else
                    {
                        if (Registers.ContainsKey(register.Name))
                        {
                            System.Diagnostics.Debug.WriteLine($"Warning: Duplicate public register name '{register.Name}' found. Skipping this register.");
                            continue;
                        }
                        Registers.Add(register.Name, register);
                    }
                }
                Fifos = new Dictionary<string, Fifo>();
                foreach (var bitfileFifo in _bitfile.Fifos)
                {
                    var fifo = CreateFifo(bitfileFifo);
                    Fifos.Add(fifo.Name, fifo);
                }
            }
            catch
            {
                NativeMethods.NiFpgaDll_Close(_session, 1);
                throw;
            }
        }

        private Fifo CreateFifo(FpgaFifo bitfileFifo)
        {
            switch (bitfileFifo.Type.DataType)
            {
                case DataType.U8:
                    return new FifoU8(this, bitfileFifo);
                case DataType.U16:
                    return new FifoU16(this, bitfileFifo);
                case DataType.U32:
                    return new FifoU32(this, bitfileFifo);
                case DataType.U64:
                    return new FifoU64(this, bitfileFifo);
                case DataType.I8:
                    return new FifoI8(this, bitfileFifo);
                case DataType.I16:
                    return new FifoI16(this, bitfileFifo);
                case DataType.I32:
                    return new FifoI32(this, bitfileFifo);
                case DataType.I64:
                    return new FifoI64(this, bitfileFifo);
                default:
                    throw new ArgumentException($"Unsupported FIFO data type: {bitfileFifo.Type.DataType}");
            }
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Dispose unmanaged resources.
                NativeMethods.NiFpgaDll_Close(_session, 1);
                _disposed = true;
            }
        }

        ~Session()
        {
            Dispose(false);
        }

        public void download()
        {
            var status = NativeMethods.NiFpgaDll_Download(_session);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_Run errored");
        }

        public void reset()
        {
            var status = NativeMethods.NiFpgaDll_Reset(_session);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_Run errored");
        }

        public void run()
        {
            var status = NativeMethods.NiFpgaDll_Run(_session, 0);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_Run errored");
        }

        public FpgaViState GetFpgaViState()
        {
            UInt32 state;
            var status = NativeMethods.NiFpgaDll_GetFpgaViState(_session, out state);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_GetFpgaViState errored");
            return (FpgaViState)state;
        }
    }

    public class Register
    {
        public String Name;
        private int _transferLen;
        private FpgaRegister _register;
        private Session _session;
        private uint _resource;
        private int _bitShift;

        public int Length => _register.Length();

        public Register(Session session, FpgaRegister bitfileRegister, uint baseAddressOnDevice)
        {
            Name = bitfileRegister.Name;
            _register = bitfileRegister;
            _session = session;
            _transferLen = (int)Math.Ceiling(bitfileRegister.Type.SizeInBits / 32.0);
            _resource = bitfileRegister.Offset + baseAddressOnDevice;
            if (bitfileRegister.AccessMayTimeout)
            {
                _resource = _resource | 0x80000000;
            }
            if (_transferLen > 1)
                _bitShift = _transferLen * 32 - _register.Type.SizeInBits;
            else
                _bitShift = 0;
        }

        public dynamic Read()
        {
            uint[] buf = new uint[_transferLen];
            var status = NativeMethods.NiFpgaDll_ReadArrayU32(_session._session, _register.Offset, buf, _transferLen);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadArrayU32 errored");
            return _register.Type.UnpackData(buf, _bitShift);
        }

        public void Write(dynamic userInput)
        {
            uint[] buf = new uint[_transferLen];
            _register.Type.PackData(userInput, buf, _bitShift);
            var status = NativeMethods.NiFpgaDll_WriteArrayU32(_session._session, _register.Offset, buf, _transferLen);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteArrayU32 errored");
        }
    }

    public abstract class Fifo
    {
        public String Name;
        protected FpgaFifo _fifo;
        protected Session _session;

        public Fifo(Session session, FpgaFifo bitfileFifo)
        {
            Name = bitfileFifo.Name;
            _fifo = bitfileFifo;
            _session = session;
        }

        public void Start()
        {
            var status = NativeMethods.NiFpgaDll_StartFifo(_session._session, _fifo.Number);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_StartFifo errored");
        }

        public void Stop()
        {
            var status = NativeMethods.NiFpgaDll_StopFifo(_session._session, _fifo.Number);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_StopFifo errored");
        }

        public ulong Configure(UIntPtr requestedDepth)
        {
            UIntPtr actualDepth;
            var status = NativeMethods.NiFpgaDll_ConfigureFifo2(_session._session, _fifo.Number, requestedDepth, out actualDepth);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ConfigureFifo2 errored");
            return actualDepth.ToUInt64();
        }

        public FifoReaderWriter<T> ReaderWriter<T>()
        {
            if (this is FifoReaderWriter<T> typedFifo)
            {
                return typedFifo;
            }
            else
            {
                throw new ArgumentException($"FIFO not of the correct type.");
            }
        }
    }

    public interface FifoReaderWriter<T>
    {
        public nuint Write(T data, UInt32 timeout = 5000);

        public T Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000);
    }


    public class FifoU8 : Fifo, FifoReaderWriter<byte[]>
    {
        public FifoU8(Session session, FpgaFifo bitfileFifo)
            : base(session, bitfileFifo)
        {
        }

        public nuint Write(byte[] data, UInt32 timeout = 5000)
        {
            nuint count = (nuint)data.Length;
            nuint remaining;
            Int32 status = NativeMethods.NiFpgaDll_WriteFifoU8(_session._session, _fifo.Number, data, count, timeout, out remaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoU8 errored");
                return remaining;
        }

        public byte[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000)
        {
            byte[] data = new byte[count];
            Int32 status = NativeMethods.NiFpgaDll_ReadFifoU8(_session._session, _fifo.Number, data, count, timeout, out elementsRemaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoU8 errored");
            return data;
        }
    }

    public class FifoU16 : Fifo, FifoReaderWriter<ushort[]>
    {
        public FifoU16(Session session, FpgaFifo bitfileFifo)
            : base(session, bitfileFifo)
        {
        }

        public nuint Write(ushort[] data, UInt32 timeout = 5000)
        {
            nuint count = (nuint)data.Length;
            nuint remaining;

            Int32 status = NativeMethods.NiFpgaDll_WriteFifoU16(_session._session, _fifo.Number, data, count, timeout, out remaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoU16 errored");
            return remaining;
        }

        public ushort[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000)
        {
            ushort[] data = new ushort[count];
            nuint remaining;

            Int32 status = NativeMethods.NiFpgaDll_ReadFifoU16(_session._session, _fifo.Number, data, count, timeout, out remaining);
                elementsRemaining = remaining;
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoU16 errored");

            return data;
        }
    }

    public class FifoU32 : Fifo, FifoReaderWriter<uint[]>
    { 
        public FifoU32(Session session, FpgaFifo bitfileFifo)
            : base(session, bitfileFifo)
        {
        }

        public nuint Write(uint[] data, UInt32 timeout=5000)
        {
            nuint count = (nuint)data.Length;
            nuint remaining;

            Int32 status = NativeMethods.NiFpgaDll_WriteFifoU32(_session._session, _fifo.Number, data, count, timeout, out remaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoU32 errored");
            return remaining;
        }

        public uint[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout=5000)
        {
            uint[] data = new uint[count];
            Int32 status = NativeMethods.NiFpgaDll_ReadFifoU32(_session._session, _fifo.Number, data, count, timeout, out elementsRemaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoU32 errored");
            return data;
        }
    }
public class FifoU64 : Fifo, FifoReaderWriter<ulong[]>
{
    public FifoU64(Session session, FpgaFifo bitfileFifo)
        : base(session, bitfileFifo)
    {
    }

    public nuint Write(ulong[] data, UInt32 timeout = 5000)
    {
        nuint count = (nuint)data.Length;
        nuint remaining;
        Int32 status = NativeMethods.NiFpgaDll_WriteFifoU64(_session._session, _fifo.Number, data, count, timeout, out remaining);
        NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoU64 errored");
        return remaining;
    }

    public ulong[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000)
    {
        ulong[] data = new ulong[count];
        Int32 status = NativeMethods.NiFpgaDll_ReadFifoU64(_session._session, _fifo.Number, data, count, timeout, out elementsRemaining);
        NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoU64 errored");
        return data;
    }
}

    public class FifoI8 : Fifo, FifoReaderWriter<sbyte[]>
    {
        public FifoI8(Session session, FpgaFifo bitfileFifo)
            : base(session, bitfileFifo)
        {
        }

        public nuint Write(sbyte[] data, UInt32 timeout = 5000)
        {
            nuint count = (nuint)data.Length;
            nuint remaining;
            Int32 status = NativeMethods.NiFpgaDll_WriteFifoI8(_session._session, _fifo.Number, data, count, timeout, out remaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoI8 errored");
            return remaining;
        }

        public sbyte[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000)
        {
            sbyte[] data = new sbyte[count];
            Int32 status = NativeMethods.NiFpgaDll_ReadFifoI8(_session._session, _fifo.Number, data, count, timeout, out elementsRemaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoI8 errored");
            return data;
        }
    }

    public class FifoI16 : Fifo, FifoReaderWriter<short[]>
    {
        public FifoI16(Session session, FpgaFifo bitfileFifo)
            : base(session, bitfileFifo)
        {
        }

        public nuint Write(short[] data, UInt32 timeout = 5000)
        {
            nuint count = (nuint)data.Length;
            nuint remaining;
            Int32 status = NativeMethods.NiFpgaDll_WriteFifoI16(_session._session, _fifo.Number, data, count, timeout, out remaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoI16 errored");
            return remaining;
        }

        public short[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000)
        {
            short[] data = new short[count];
            Int32 status = NativeMethods.NiFpgaDll_ReadFifoI16(_session._session, _fifo.Number, data, count, timeout, out elementsRemaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoI16 errored");
            return data;
        }
    }

    public class FifoI32 : Fifo, FifoReaderWriter<int[]>
    {
        public FifoI32(Session session, FpgaFifo bitfileFifo)
            : base(session, bitfileFifo)
        {
        }

        public nuint Write(int[] data, UInt32 timeout = 5000)
        {
            nuint count = (nuint)data.Length;
            nuint remaining;
            Int32 status = NativeMethods.NiFpgaDll_WriteFifoI32(_session._session, _fifo.Number, data, count, timeout, out remaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoI32 errored");
            return remaining;
        }

        public int[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000)
        {
            int[] data = new int[count];
            Int32 status = NativeMethods.NiFpgaDll_ReadFifoI32(_session._session, _fifo.Number, data, count, timeout, out elementsRemaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoI32 errored");
            return data;
        }
    }

    public class FifoI64 : Fifo, FifoReaderWriter<long[]>
    {
        public FifoI64(Session session, FpgaFifo bitfileFifo)
            : base(session, bitfileFifo)
        {
        }

        public nuint Write(long[] data, UInt32 timeout = 5000)
        {
            nuint count = (nuint)data.Length; 
            nuint remaining;
            Int32 status = NativeMethods.NiFpgaDll_WriteFifoI64(_session._session, _fifo.Number, data, count, timeout, out remaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_WriteFifoI64 errored");
            return remaining;
        }

        public long[] Read(nuint count, out nuint elementsRemaining, UInt32 timeout = 5000)
        {
            long[] data = new long[count];
            Int32 status = NativeMethods.NiFpgaDll_ReadFifoI64(_session._session, _fifo.Number, data, count, timeout, out elementsRemaining);
            NiFpgaException.ThrowIfFatal(status, "NiFpgaDll_ReadFifoI64 errored");
            return data;
        }
    }
}