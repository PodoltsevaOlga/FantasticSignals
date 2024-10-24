using System.Collections.Generic;
using System.Linq;
using Controllers;
using Entities;

namespace GraphConnections
{
    public class ConnectorsGraph
    {
        //компоненты связности
        private readonly HashSet<GraphComponent> m_Components = new HashSet<GraphComponent>();

        //добавление пустой ноды без соседей
        //она становится самостоятельной компонентой связности
        public bool AddNewNode(Connector node)
        {
            if (node == null || node.GraphComponent != null)
            {
                return false;
            }

            var component = new GraphComponent(node);
            m_Components.Add(component);
            return true;
        }

        public void AddEdge(Connector node1, Connector node2)
        {
            if (node1.GetNeighbours().Contains(node2))
            {
                return;
            }

            node1.AddNeighbour(node2);
            node2.AddNeighbour(node1);

            var component1 = node1.GraphComponent;
            var component2 = node2.GraphComponent;
            if (component1 == component2)
            {
                return;
            }

            //для небольшой экономии времени добавляем меньшую компоненту в большую
            if (component1.Size < component2.Size)
            {
                (component1, component2) = (component2, component1);
            }
            component1.AppendComponent(component2);

            m_Components.Remove(component2);

        }

        public void RemoveNode(Connector connector)
        {
            if (connector == null || connector.GraphComponent == null)
            {
                return;
            }

            if (connector.GraphComponent.Size == 1)
            {
                m_Components.Remove(connector.GraphComponent);
                connector.SetGraphComponent(null);
            }

            foreach (var node in connector.GetNeighbours())
            {
                node.RemoveNeighbour(connector);
            }
            
            SplitComponents(connector);
            connector.RemoveAllNeighbours();
            connector.SetGraphComponent(null);
        }

        //ребра до переданного коннектора уже удалены
        private void SplitComponents(Connector connector)
        {
            var neighbourNodes = connector.GetNeighbours().ToList();
            if (neighbourNodes.Count == 0)
            {
                return;
            }

            var neighboursCheck = new HashSet<Connector>(neighbourNodes);
            var bfsQueue = new Queue<Connector>();
            bool componentRemainWhole = false;
            
            var controller = GameController.Instance;
            var connectors = controller.ConnectorsHashSetPool.GetOrCreateObject();
            var receivers = controller.ReceiversHashSetPool.GetOrCreateObject();
            var senders = controller.SendersHashSetPool.GetOrCreateObject();
            
            bool receiverConnected = connector.TouchedReceiver == null;
            bool senderConnected = connector.TouchedSender == null;
            var newComponents = new List<GraphComponent>();
            
            m_Components.Remove(connector.GraphComponent);

            for (int i = 0; i < neighbourNodes.Count; ++i)
            {
                var neighbourNode = neighbourNodes[i];

                if (!neighboursCheck.Contains(neighbourNode))
                {
                    continue;
                }
                
                if (connectors.Contains(neighbourNode))
                {
                    continue;
                }
                
                bfsQueue.Enqueue(neighbourNode);
                while (bfsQueue.Count > 0)
                {
                    var current = bfsQueue.Dequeue();
                    foreach (var node in current.GetNeighbours())
                    {
                        if (!connectors.Contains(node))
                        {
                            bfsQueue.Enqueue(node);
                        }
                    }

                    connectors.Add(current);
                    if (current.TouchedReceiver != null)
                    {
                        receivers.Add(current.TouchedReceiver);
                        if (current.TouchedReceiver == connector.TouchedReceiver)
                        {
                            receiverConnected = true;
                        }
                    }

                    if (current.TouchedSender != null)
                    {
                        senders.Add(current.TouchedSender);
                        if (current.TouchedSender == connector.TouchedSender)
                        {
                            senderConnected = true;
                        }
                    }

                    neighboursCheck.Remove(current);
                    if (i == 0 && neighboursCheck.Count == 0)
                    {
                        componentRemainWhole = true;
                    }

                    if (componentRemainWhole && senderConnected && receiverConnected)
                    {
                        break;
                    }
                }
                
                if (componentRemainWhole && senderConnected && receiverConnected)
                {
                    break;
                }

                var newGraphComponent = new GraphComponent(connectors, receivers, senders);
                m_Components.Add(newGraphComponent);
                newComponents.Add(newGraphComponent);
                foreach (var receiver in receivers)
                {
                    receiver.GraphComponents.Remove(connector.GraphComponent);
                }
                
                connectors.Clear();
                receivers.Clear();
                senders.Clear();
            }

            //после удаления коннектора компонента связности осталась целой.
            //все, что нужно пересчитать - отсоединился ли какой-то ресивер или сендер
            if (componentRemainWhole)
            {
                RecalculateComponentWithoutConnector(connector, receiverConnected, senderConnected);
                m_Components.Add(connector.GraphComponent);
            }
            else
            {
                foreach (var receiver in connector.GraphComponent.Receivers)
                {
                    receiver.GraphComponents.Remove(connector.GraphComponent);
                }
                
                foreach (var receiver in connector.GraphComponent.Receivers)
                {
                    //те, которые не вошли ни в одну компоненту связнсти
                    if (receiver.GraphComponents.Count == 0)
                    {
                        receiver.RemoveAllSenders();
                    }
                }
                
                foreach (var newComponent in newComponents)
                {
                    foreach (var receiver in newComponent.Receivers)
                    {
                        CheckReceiverConnections(receiver, newComponent, connector.GraphComponent);
                    }
                }
               
            }
            
            controller.ConnectorsHashSetPool.ReleaseObject(connectors);
            controller.ReceiversHashSetPool.ReleaseObject(receivers);
            controller.SendersHashSetPool.ReleaseObject(senders);
        }

        private void CheckReceiverConnections(Receiver receiver, GraphComponent graphComponent = null, GraphComponent excludingComponent = null)
        {
            if (receiver.Senders.Count == 0)
            {
                return;
            }
            
            var oldSenders = new HashSet<Sender>(receiver.Senders);
            foreach (var sender in oldSenders)
            {
                bool senderConnectedToReceiver = false;
                if (graphComponent != null && graphComponent.Senders.Contains(sender))
                {
                    continue;
                }

                if (excludingComponent != null && !excludingComponent.Senders.Contains(sender))
                {
                    continue;
                }

                foreach (var component in receiver.GraphComponents)
                {
                    if (component.Senders.Contains(sender))
                    {
                        senderConnectedToReceiver = true;
                        break;
                    }
                }

                if (!senderConnectedToReceiver)
                {
                    receiver.RemoveSender(sender);
                }
            }
        }

        private void CheckReceiversConnectionToSender(Sender sender, GraphComponent graphComponent)
        {
            foreach (var receiver in graphComponent.Receivers)
            {
                bool senderConnectedToReceiver = false;
                foreach (var component in receiver.GraphComponents)
                {
                    if (component.Senders.Contains(sender))
                    {
                        senderConnectedToReceiver = true;
                        break;
                    }
                }
                
                if (!senderConnectedToReceiver)
                {
                    receiver.RemoveSender(sender);
                }
            }
            
        }

        private void RecalculateComponentWithoutConnector(Connector connector, bool receiverConnected,
            bool senderConnected)
        {
            var graphComponent = connector.GraphComponent;

            //отсоединился сендер
            if (!senderConnected)
            {
                graphComponent.Senders.Remove(connector.TouchedSender);
                CheckReceiversConnectionToSender(connector.TouchedSender, graphComponent);
            }

            //отсоединился ресивер
            if (!receiverConnected)
            {
                graphComponent.Receivers.Remove(connector.TouchedReceiver);
                connector.TouchedReceiver.GraphComponents.Remove(graphComponent);
                if (connector.TouchedReceiver.GraphComponents.Count == 0)
                {
                    connector.TouchedReceiver.RemoveAllSenders();
                }
                else
                {
                    CheckReceiverConnections(connector.TouchedReceiver, null, graphComponent);
                }
            }

            graphComponent.Nodes.Remove(connector);

        }
    }
}