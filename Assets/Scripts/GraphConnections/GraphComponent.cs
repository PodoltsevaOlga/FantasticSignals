using System.Collections.Generic;
using Controllers;
using Entities;

namespace GraphConnections
{
    public class GraphComponent
    {
        public HashSet<Connector> Nodes { get; private set; }

        public HashSet<Receiver> Receivers { get; private set; }
        public HashSet<Sender> Senders { get; private set; }

        public int Size => Nodes.Count;

        public GraphComponent(Connector connector)
        {
            Initialize();
            
            Nodes.Add(connector);
            connector.SetGraphComponent(this);
        }

        public GraphComponent(IEnumerable<Connector> connectors, IEnumerable<Receiver> receivers, IEnumerable<Sender> senders)
        {
            Initialize();
            
            Nodes.UnionWith(connectors);
            Receivers.UnionWith(receivers);
            Senders.UnionWith(senders);
            
            foreach (var receiver in Receivers)
            {
                foreach (var sender in Senders)
                {
                    receiver.AddSender(sender);
                }

                receiver.GraphComponents.Add(this);
            }

            foreach (var node in Nodes)
            {
                node.SetGraphComponent(this);
            }
        }

        private void Initialize()
        {
            var controller = GameController.Instance;
            Nodes = controller.ConnectorsHashSetPool.GetOrCreateObject();
            Receivers = controller.ReceiversHashSetPool.GetOrCreateObject();
            Senders = controller.SendersHashSetPool.GetOrCreateObject();
        }

        public void UpdateConnector(Connector connector)
        {
            if (connector == null)
            {
                return;
            }

            if (connector.TouchedReceiver != null)
            {
                Receivers.Add(connector.TouchedReceiver);
                connector.TouchedReceiver.GraphComponents.Add(this);
            }
            else if (connector.TouchedSender != null)
            {
                Senders.Add(connector.TouchedSender);
            }
        }

        public void AppendComponent(GraphComponent component)
        {
            if (component == this || component == null)
            {
                return;
            }

            foreach (var node in component.Nodes)
            {
                node.SetGraphComponent(this);
            }
            Nodes.UnionWith(component.Nodes);

            var receivers = new List<Receiver>();
            receivers.AddRange(Receivers);
            var otherComponentReceivers = new List<Receiver>();
            foreach (var receiver in component.Receivers)
            {
                if (Receivers.Add(receiver))
                {
                    otherComponentReceivers.Add(receiver);
                }
            }

            var senders = new List<Sender>();
            senders.AddRange(Senders);
            var otherComponentSenders = new List<Sender>();
            foreach (var sender in component.Senders)
            {
                if (Senders.Add(sender))
                {
                    otherComponentSenders.Add(sender);
                }
            }

            foreach (var receiver in receivers)
            {
                foreach (var sender in otherComponentSenders)
                {
                    receiver.AddSender(sender);
                }
            }

            foreach (var receiver in otherComponentReceivers)
            {
                foreach (var sender in senders)
                {
                    receiver.AddSender(sender);
                }
            }

            foreach (var receiver in Receivers)
            {
                receiver.GraphComponents.Remove(component);
                receiver.GraphComponents.Add(this);
            }

            receivers.Clear();
            otherComponentReceivers.Clear();
            senders.Clear();
            otherComponentSenders.Clear();
            component.Clear();
        }

        public void Clear()
        {
            var controller = GameController.Instance;
            controller.ConnectorsHashSetPool.ReleaseObject(Nodes);
            controller.ReceiversHashSetPool.ReleaseObject(Receivers);
            controller.SendersHashSetPool.ReleaseObject(Senders);
        }
    }
}