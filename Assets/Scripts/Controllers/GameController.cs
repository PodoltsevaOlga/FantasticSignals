using Entities;
using GraphConnections;
using Utils;

namespace Controllers
{
    public class GameController
    {
        public readonly ConnectorsGraph ConnectionsConnectorsGraph = new ConnectorsGraph();
        public readonly HashSetPool<Receiver> ReceiversHashSetPool = new HashSetPool<Receiver>();
        public readonly HashSetPool<Sender> SendersHashSetPool = new HashSetPool<Sender>();
        public readonly HashSetPool<Connector> ConnectorsHashSetPool = new HashSetPool<Connector>();

        private static GameController m_Instance = null;

        private GameController()
        {
        }

        public static GameController Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new GameController();
                }
                return m_Instance;
            }
        }
    }
}