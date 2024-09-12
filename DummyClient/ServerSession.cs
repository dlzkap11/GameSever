using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DummyClient
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

       
        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);


    }

    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset); ?
            count += 2;
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += 2;

            this.playerId = BitConverter.ToUInt16(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count));
            
            count += 8;
            
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096); //openSegement
            
            ushort count = 0;
            bool success = true;

            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count), packet.size);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }


    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    class ServerSession : Session
    {
        /* // unsafe를 사용해 포인터구현
        static unsafe void ToBytes(byte[] array, int offset, ulong value)
        {
            fixed(byte* ptr = &array[offset])
                *(ulong*)ptr = value;
        }
        */



        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };

            // 보낸다
            //for (int i = 0; i < 5; i++)
            {
                /*
                byte[] size = BitConverter.GetBytes(packet.size); 
                byte[] packetId = BitConverter.GetBytes(packet.packetId);
                byte[] playerId = BitConverter.GetBytes(packet.playerId);               
                Array.Copy(size, 0, openSegement.Array, openSegement.Offset + count, 2);
                count += 2;
                Array.Copy(packetId, 0, openSegement.Array, openSegement.Offset + count, 2);
                count += 2;
                Array.Copy(playerId, 0, openSegement.Array, openSegement.Offset + count, 8);
                count += 8;
                */
                ArraySegment<byte> s = packet.Write();
                if (s != null)
                    Send(s);
                
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}

