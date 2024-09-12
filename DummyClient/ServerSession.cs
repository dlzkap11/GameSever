﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DummyClient
{
    class PlayerInfoReq
    {
        public long playerId;
        public string name;


        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);

                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();


        public  void Read(ArraySegment<byte> segement)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segement.Array, segement.Offset, segement.Count);
            
            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset); ?
            count += sizeof(ushort);
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToUInt16(s.Slice(count, s.Length - count));         
            count += sizeof(long);

            //string
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); //길이추출
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            // skill list
            skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); //길이추출
            count += sizeof(ushort);
           
            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);
            }
            
        }

        public  ArraySegment<byte> Write()
        {
            ArraySegment<byte> segement = SendBufferHelper.Open(4096); //openSegement
            
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segement.Array, segement.Offset, segement.Count);


            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegement.Array, openSegement.Offset, openSegement.Count), packet.size);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);



            // string
            // string len [2]
            // byte[]


            //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            //success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            //count += sizeof(ushort);
            //Encoding.Unicode.GetBytes(this.name);
            //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segement.Array, count, nameLen);
            //count += nameLen;

            //string
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segement.Array, segement.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;


            // skill list
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach(SkillInfo skill in skills)
                success &= skill.Write(s, ref count);


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
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "ABCD"};
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 101, level = 1, duration = 3.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 201, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 301, level = 3, duration = 7.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 401, level = 4, duration = 3.0f });

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

