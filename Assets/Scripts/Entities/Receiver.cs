using System.Collections.Generic;
using GraphConnections;
using TMPro;
using UnityEngine;

namespace Entities
{
    [RequireComponent(typeof(Collider2D))]
    public class Receiver : MonoBehaviour
    {
        [SerializeField] private TextMeshPro m_SumText;
        
        private readonly HashSet<Sender> m_ConnectedSenders = new HashSet<Sender>();

        public HashSet<Sender> Senders => m_ConnectedSenders;
        
        //можно заменить на коннекторы, но тогда проверок будет чуть больше
        public readonly HashSet<GraphComponent> GraphComponents = new HashSet<GraphComponent>();
        public int Sum { get; private set; } = 0;

        public bool AddSender(Sender sender)
        {
            if (HaveSender(sender))
                return false;

            m_ConnectedSenders.Add(sender);
            OnSumChanged(Sum + sender.Value);
            return true;
        }

        public bool HaveSender(Sender sender)
        {
            return m_ConnectedSenders.Contains(sender);
        }

        public bool RemoveSender(Sender sender)
        {
            if (!HaveSender(sender))
                return false;

            m_ConnectedSenders.Remove(sender);
            OnSumChanged(Sum - sender.Value);
            return true;
        }

        public void RemoveAllSenders()
        {
            m_ConnectedSenders.Clear();
            OnSumChanged(0);
        }

        public void RecalculateSum()
        {
            int sum = 0;
            foreach (var sender in m_ConnectedSenders)
            {
                sum += sender.Value;
            }

            OnSumChanged(sum);
        }

        private void OnSumChanged(int newValue)
        {
            Sum = newValue;
            if (m_SumText != null)
            {
                m_SumText.text = Sum.ToString();
            }
        }
    }
}