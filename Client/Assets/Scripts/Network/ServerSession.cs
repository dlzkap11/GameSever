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
    class ServerSession : PacketSession
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

            /*
            // 보낸다
            //for (int i = 0; i < 5; i++)
            {
                
                byte[] size = BitConverter.GetBytes(packet.size); 
                byte[] packetId = BitConverter.GetBytes(packet.packetId);
                byte[] playerId = BitConverter.GetBytes(packet.playerId);               
                Array.Copy(size, 0, open
                
                
                
                .Array, opensegment.Offset + count, 2);
                count += 2;
                Array.Copy(packetId, 0, opensegment.Array, opensegment.Offset + count, 2);
                count += 2;
                Array.Copy(playerId, 0, opensegment.Array, opensegment.Offset + count, 8);
                count += 8;
                           

            }
            */
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer, (s, p) => PacketQueue.Instance.Push(p));
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}

