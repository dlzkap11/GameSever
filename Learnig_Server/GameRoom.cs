using ServerCore;

namespace Learnig_Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SesssionId;
            packet.chat = $"{chat}I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            //N^2 흠.. 패킷모아보내기..
            foreach (ClientSession s in _sessions)
                s.Send(segment);

        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }


    }
}
