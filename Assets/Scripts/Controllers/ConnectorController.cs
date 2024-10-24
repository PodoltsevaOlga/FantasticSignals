using Entities;
using GraphConnections;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class ConnectorController : MonoBehaviour
    {
        private GameObjectsPool m_ConnectorPool;

        [SerializeField] 
        private GameObject m_ConnectorPrefab;
        
        public ConnectorsGraph ConnectionsConnectorsGraph => GameController.Instance.ConnectionsConnectorsGraph;
        
        private void Awake()
        {
            m_ConnectorPool = new GameObjectsPool(m_ConnectorPrefab);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var position2D = new Vector2(position.x, position.y);
                var raycastHit = Physics2D.Raycast(position2D, Vector2.zero, 20.0f);
                if (raycastHit.collider != null)
                {
                    var go = raycastHit.collider.gameObject;
                    var connector = go.GetComponent<Connector>();
                    if (connector != null)
                    {
                        RemoveConnector(connector);
                    }
                }
                else
                {
                    SpawnConnector(position2D);
                }
            }
        }

        private void RemoveConnector(Connector connector)
        {
            ConnectionsConnectorsGraph.RemoveNode(connector);
            connector.Clear();
            
            var go = connector.gameObject;
            go.SetActive(false);
            go.transform.position = new Vector3(1000.0f, 1000.0f, 0.0f);
            m_ConnectorPool.ReleaseObject(go);
        }

        private void SpawnConnector(Vector2 position)
        {
            var go = m_ConnectorPool.GetOrCreateObject();
            go.transform.position = new Vector3(position.x, position.y, 0.0f);
            
            var connector = go.GetComponent<Connector>();
            ConnectionsConnectorsGraph.AddNewNode(connector);
            
            go.SetActive(true);
            
        }
    }
}