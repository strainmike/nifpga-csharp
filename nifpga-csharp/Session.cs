using System;
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
    }

    public class Session : IDisposable
    {
        internal int _session;
        private bool _disposed = false;
        private Bitfile _bitfile;
        private Dictionary<string, Register> _internalRegisters;
        public Dictionary<string, Register> Registers;

        public Session(string bitfile, string resource)
        {
            _bitfile = new Bitfile(bitfile);
            uint attribute = (uint)1 << 31;
            int result = NativeMethods.NiFpgaDll_Open(bitfile, null, resource, attribute, out _session);
            if (result != 0)
            {
                throw new Exception($"Failed to open session. NiFpgaDll_Open returned: {result}");
            }

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
            }
            catch
            {
                NativeMethods.NiFpgaDll_Close(_session, 0);
                throw;
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
                NativeMethods.NiFpgaDll_Close(_session, 0);
                _disposed = true;
            }
        }

        ~Session()
        {
            Dispose(false);
        }

        public void download()
        {
            int result = NativeMethods.NiFpgaDll_Download(_session);
            if (result != 0)
            {
                throw new Exception("Failed to download");
            }
        }

        public void reset()
        {
            int result = NativeMethods.NiFpgaDll_Reset(_session);
            if (result != 0)
            {
                throw new Exception("Failed to reset");
            }
        }

        public void run()
        {
            int result = NativeMethods.NiFpgaDll_Run(_session, 0);
            if (result != 0)
            {
                throw new Exception("Failed to run");
            }
        }

        public FpgaViState GetFpgaViState()
        {
            UInt32 state;
            int result = NativeMethods.NiFpgaDll_GetFpgaViState(_session, out state);
            if (result != 0)
            {
                throw new Exception("Failed to get state");
            }
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
        }

        public dynamic Read()
        {
            uint[] buf = new uint[_transferLen];
            NativeMethods.NiFpgaDll_ReadArrayU32(_session._session, _register.Offset, buf, _transferLen);
            return _register.Type.UnpackData(buf, 0);
        }

        public void Write(dynamic userInput)
        {
            uint[] buf = new uint[_transferLen];
            _register.Type.PackData(userInput, buf, 0);
            NativeMethods.NiFpgaDll_WriteArrayU32(_session._session, _register.Offset, buf, _transferLen);
        }
    }
}