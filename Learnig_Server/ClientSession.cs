using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Learnig_Server
{
    public enum PacketID
    {
        PlayerInfoReq = 1,
        Test = 2,

    }

    class PlayerInfoReq
    {
        public byte testByte;
        public long playerId;
        public string name;

        public class Skill
        {
            public int id;
            public short level;
            public float duration;

            public class Attribute
            {
                public int att;

                public void Read(ReadOnlySpan<byte> s, ref ushort count)
                {

                    this.att = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                    count += sizeof(int);

                }

                public bool Write(Span<byte> s, ref ushort count)
                {
                    bool success = true;

                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.att);
                    count += sizeof(int);

                    return success;
                }


            }

            public List<Attribute> attributes = new List<Attribute>();


            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {

                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);


                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);


                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);


                attributes.Clear();
                ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); //길이추출
                count += sizeof(ushort);

                for (int i = 0; i < attributeLen; i++)
                {
                    Attribute attribute = new Attribute();
                    attribute.Read(s, ref count);
                    attributes.Add(attribute);
                }

            }

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);


                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(short);


                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);


                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)attributes.Count);
                count += sizeof(ushort);
                foreach (Attribute attribute in attributes)
                    success &= attribute.Write(s, ref count);

                return success;
            }


        }

        public List<Skill> skills = new List<Skill>();


        public void Read(ArraySegment<byte> segement)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segement.Array, segement.Offset, segement.Count);

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset); ?
            count += sizeof(ushort);
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            this.testByte = (byte)segement.Array[segement.Offset + count];
            count += sizeof(byte);


            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);


            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); //길이추출
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;


            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); //길이추출
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);
                skills.Add(skill);
            }

        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segement = SendBufferHelper.Open(4096); //openSegement          
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segement.Array, segement.Offset, segement.Count);

            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count), packet.size);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);

            segement.Array[segement.Offset + count] = (byte)this.testByte;
            count += sizeof(byte);


            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);


            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segement.Array, segement.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;


            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (Skill skill in skills)
                success &= skill.Write(s, ref count);


            success &= BitConverter.TryWriteBytes(s, count); //최종 카운트
            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
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

                        foreach(PlayerInfoReq.Skill skill in p.skills)
                        {
                            Console.WriteLine($"Skill ({skill.id} {skill.level} {skill.duration})");
                        }

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