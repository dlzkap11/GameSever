using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Learnig_Server
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
        public string name;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segement)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segement.Array, segement.Offset, segement.Count);

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset); ?
            count += sizeof(ushort);
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToUInt16(s.Slice(count, s.Length - count));

            count += sizeof(long);


            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segement = SendBufferHelper.Open(4096); //openSegement

            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segement.Array, segement.Offset, segement.Count);


            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count), packet.size);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);



            // string
            // string len [2]
            // byte[]


            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            Encoding.Unicode.GetBytes(this.name);
            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segement.Array, count, nameLen);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s, count); //최종 카운트

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

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected : {endPoint}");

            //Packet packet = new Packet() { size = 100, packetId = 10 };

            // [100]  []  []  []  [10]  []  []  []
            //ArraySegment<byte> openSegement = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegement.Array, openSegement.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegement.Array, openSegement.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);



            //Send(sendBuff);
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"Player InfoReq : {p.playerId} {p.name}");
                    }
                    break;

            }

            Console.WriteLine($"RecvPacket Id : {id}, Size : {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        // 이동패킷 ((3,2)좌표로 이동하고 싶다!)
        // 15 3 2

        /*
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
            return buffer.Count;
        }
        */


        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}