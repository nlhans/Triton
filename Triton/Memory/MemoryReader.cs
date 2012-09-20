using System;
using System.Diagnostics;
using System.Text;
using Triton.Memory;


namespace Triton.Memory
{
    public class MemoryReader
    {
        public const uint PROCESS_QUERY_INFORMATION = (0x0400);
        public const uint PROCESS_QUERY_LIMITED_INFORMATION = (0x0100);
        public const uint PROCESS_VM_READ = (0x0010);
        public const uint PROCESS_VM_WRITE = (0x0028);

        /// <summary>	
        /// Process from which to read		
        /// </summary>
        public Process ReadProcess
        {
            get
            {
                return m_ReadProcess;
            }
            set
            {
                m_ReadProcess = value;
            }
        }

        protected Process m_ReadProcess = null;

        protected IntPtr m_hProcess = IntPtr.Zero;

        public void OpenProcess()
        {
            m_hProcess = ProcessMemoryReaderApi.OpenProcess(  PROCESS_VM_READ, 0, (uint)m_ReadProcess.Id);

            if (m_hProcess == IntPtr.Zero)
            {
                Console.WriteLine(ProcessMemoryReaderApi.GetLastError());
            }
        }

        public void CloseHandle()
        {
            if (m_hProcess != null)
            {
                int iRetValue;
                iRetValue = ProcessMemoryReaderApi.CloseHandle(m_hProcess);
                if (iRetValue == 0)
                    throw new Exception("CloseHandle failed");
            }
        }

        public byte[] ReadProcessMemory(IntPtr MemoryAddress, uint bytesToRead)
        {
            byte[] buffer = new byte[bytesToRead];

            IntPtr ptrBytesReaded;
            int bytesReaded = 0;
            ProcessMemoryReaderApi.ReadProcessMemory(m_hProcess, MemoryAddress, buffer, bytesToRead, out ptrBytesReaded);

            bytesReaded = ptrBytesReaded.ToInt32();
            bytesReaded++;
            return buffer;
        }


        public byte ReadByte(IntPtr address)
        {
            return this.ReadProcessMemory(address, 1)[0];
        }

        public byte[] ReadBytes(IntPtr address, uint size)
        {
            return this.ReadProcessMemory(address, size);
        }

        public string ReadString(IntPtr address, uint size)
        {
            int i = 0;
            byte[] bt = this.ReadBytes(address, size);
            for (i = 0; i < bt.Length; i++)
            {
                if (bt[i] == 0)
                    break;
            }

            return Encoding.ASCII.GetString(bt, 0, i);
        }

        public double ReadDouble(IntPtr address)
        {
            return BitConverter.ToDouble(this.ReadProcessMemory(address, 8), 0);
        }

        public int ReadInt32(IntPtr address)
        {
            return BitConverter.ToInt32(this.ReadProcessMemory(address, 4), 0);
        }

        public long ReadInt64(IntPtr address)
        {
            return BitConverter.ToInt64(this.ReadProcessMemory(address, 8), 0);
        }

        public float ReadFloat(IntPtr address)
        {
            return BitConverter.ToSingle(this.ReadProcessMemory(address, 4), 0);
        }

    }
}
