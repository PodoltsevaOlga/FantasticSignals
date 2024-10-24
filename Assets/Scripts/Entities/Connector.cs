using System.Collections.Generic;
using Controllers;
using GraphConnections;
using UnityEngine;

namespace Entities
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Connector : MonoBehaviour
    {
        public Sender TouchedSender { get; private set; }
        public Receiver TouchedReceiver { get; private set; }

        public readonly List<Connector> Connectors = new List<Connector>();
        public GraphComponent GraphComponent { get; private set; } = null;

        private Collider2D m_Collider;

        private void Awake()
        {
            m_Collider = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            //триггеры продолжают работать даже на вывыключенных объектах
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            if (collider.gameObject.TryGetComponent<Sender>(out var sender))
            {
                TouchedSender = sender;
                GraphComponent.UpdateConnector(this);
            }
            else if (collider.gameObject.TryGetComponent<Receiver>(out var receiver))
            {
                TouchedReceiver = receiver;
                GraphComponent.UpdateConnector(this);
            }
            else if (collider.gameObject.TryGetComponent<Connector>(out var connector))
            {
                GameController.Instance.ConnectionsConnectorsGraph.AddEdge(this, connector);
            }
        }

        public List<Connector> GetNeighbours()
        {
            return Connectors;
        }

        public void AddNeighbour(Connector node)
        {
            if (!Connectors.Contains(node))
            {
                Connectors.Add(node);
            }
        }

        public void RemoveNeighbour(Connector node)
        {
            Connectors.Remove(node);
        }

        public void RemoveAllNeighbours()
        {
            Connectors.Clear();
        }

        public void SetGraphComponent(GraphComponent component)
        {
            GraphComponent = component;
        }

        public void Clear()
        {
            Connectors.Clear();
            GraphComponent = null;
            TouchedReceiver = null;
            TouchedSender = null;
        }
    }
}
