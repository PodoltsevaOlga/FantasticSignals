using System;
using TMPro;
using UnityEngine;

namespace Entities
{
    [RequireComponent(typeof(Collider2D))]
    public class Sender : MonoBehaviour
    {
        [SerializeField] private int m_Value = 0;
        [SerializeField] private TextMeshPro m_SumText;

        public int Value
        {
            get => m_Value;
            private set => m_Value = value;
        }

        private void Awake()
        {
            m_SumText.text = m_Value.ToString();
        }
    }
}