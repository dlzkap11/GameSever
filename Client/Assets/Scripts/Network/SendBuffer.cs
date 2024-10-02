using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; }); //영역 전개

        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }
    


    public class SendBuffer
    {
        // Clean의 개념이 없음 다른 누군가가 이전 데이터를 사용하고 있을 수 있기 때문)(queue)
        // [u] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ]
        byte[] _buffer;
        int _usedSize = 0; //writePos

        public int FreeSize { get { return _buffer.Length - _usedSize; } } //남은 공간

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }


        public ArraySegment<byte> Open(int reserveSize) //예약공간 모든 공간을 다 쓰지 않을 수 있음
        {
            if (reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }


        public ArraySegment<byte> Close(int usedSize) // 실제 데이터가 사용된 공간
        {
            ArraySegment<byte> segement = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
           
            return segement;
        }


    }
}
