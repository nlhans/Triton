using System;

namespace Triton.Memory
{
    public class MemoryWriter : MemoryReader
    {
        public void OpenProcess()
        {
            m_hProcess = ProcessMemoryReaderApi.OpenProcess(PROCESS_VM_WRITE | PROCESS_VM_READ, 0, (uint)m_ReadProcess.Id);

            if (m_hProcess == IntPtr.Zero)
            {
                Console.WriteLine(ProcessMemoryReaderApi.GetLastError());
            }
        }

        public void WriteFloat(IntPtr address, float value)
        {
            this.WriteProcessMemory(address, BitConverter.GetBytes(value));
        }

        public void WriteDouble(IntPtr address, double value)
        {
            this.WriteProcessMemory(address, BitConverter.GetBytes(value));
        }

        public void WriteByte(IntPtr address, byte value)
        {
            this.WriteProcessMemory(address, new byte[1] { value });
        }


        private void WriteProcessMemory(IntPtr address, byte[] bits)
        {
            //Trying to write to the process's memory.
            int BytesWritten;
            IntPtr BaseAddress = new IntPtr(0x49C523);
            byte[] NewVal = { 0x90 };
            ProcessMemoryReaderApi.WriteProcessMemory(m_hProcess, address, bits, (UIntPtr)bits.Length, out BytesWritten);

            if (BytesWritten == 0)
                Console.WriteLine("Failed to write ({0})", ProcessMemoryReaderApi.GetLastError());
        }
    }
}