using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Main : MonoBehaviour
{
    Dictionary<int, GameObject> blocks;
    public Camera mCamera;
    public GameObject cube;
    public Transform mapParent;
    public float unitLength = 1;
    public int unitRowNum = 11; //should be odd
    public int unitColumnNum = 21; // should be larger or equal to row num; odd too
    public Vector2Int destination;
    public NavMeshSurface navMesh;
    public float cameraMoveSpeed = 2;
    BitArray bitArray;
    GameObject blockTemplate;
    GameObject agentTemplate1, agentTemplate2;
    // Use this for initialization
	
	TestLine
	
    void Start()
    {
        blocks = new Dictionary<int, GameObject>();
        bitArray = new BitArray(unitRowNum * unitColumnNum);
        mCamera.fieldOfView = unitLength * unitRowNum / 2 * 2;
        cube.transform.localScale = new Vector3(unitLength * unitColumnNum, 1, unitLength * unitRowNum);
        navMesh.BuildNavMesh();
        blockTemplate = Resources.Load("BlockWithNavmeshVolume") as GameObject;
        agentTemplate1 = Resources.Load("Agent") as GameObject;
        agentTemplate2 = Resources.Load("AgentBig") as GameObject;
        //ground height 0.5
        destinationWorld = Index2World(destination.x, destination.y, 0.5f);
        agentSet = new HashSet<NavMeshAgent>();
        deleteSet = new HashSet<NavMeshAgent>();
    }

    int columnIndex, rowIndex, bitArrayIndex;
    Vector3 destinationWorld;
    HashSet<NavMeshAgent> agentSet;
    HashSet<NavMeshAgent> deleteSet;
    // Update is called once per frame
    void Update()
    {
        foreach (var agent in agentSet)
        {
            if (agent.remainingDistance < agent.stoppingDistance && agent.hasPath)
            {
                Debug.Log(agent.name + " remaining distance = " + agent.remainingDistance + " destination = " + agent.destination + " has path " + agent.hasPath); 
                deleteSet.Add(agent);
            }
        }

        foreach (var agent in deleteSet)
        {
            agentSet.Remove(agent);
            Destroy(agent.gameObject);
        }
        deleteSet.Clear();
        if (Input.GetKey(KeyCode.W))
        {
            mCamera.transform.position += new Vector3(0, 0, cameraMoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            mCamera.transform.position += new Vector3(-cameraMoveSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            mCamera.transform.position += new Vector3(0, 0, -cameraMoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            mCamera.transform.position += new Vector3(cameraMoveSpeed * Time.deltaTime, 0, 0);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 worldMousePosition = mCamera.ScreenToWorldPoint(Input.mousePosition);
            worldMousePosition.y = 0.5f;

            
            GameObject obj = Instantiate(Random.value > 0.8f ? agentTemplate2 : agentTemplate1, worldMousePosition, Quaternion.Euler(Vector3.zero)) as GameObject;
            NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
            agent.SetDestination(destinationWorld);
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agentSet.Add(agent);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldMousePosition = mCamera.ScreenToWorldPoint(Input.mousePosition);
            World2Index(ref worldMousePosition, out columnIndex, out rowIndex);
            bitArrayIndex = rowIndex * unitRowNum + columnIndex;
            if (columnIndex != -1 && rowIndex != -1)
            {
                if (bitArray.Get(bitArrayIndex))
                {

                    Destroy(blocks[bitArrayIndex]);
                    blocks.Remove(bitArrayIndex);
                    bitArray.Set(bitArrayIndex, false);
                }
                else
                {
                    GameObject obj = Instantiate(blockTemplate) as GameObject;
                    obj.transform.parent = mapParent;
                    obj.transform.position = Index2World(columnIndex, rowIndex, 0.5f);
                    blocks.Add(bitArrayIndex, obj);
                    bitArray.Set(rowIndex * unitRowNum + columnIndex, true);
                }
                navMesh.BuildNavMesh();
            }
            else
            {
                Debug.Log("invalid click");
            }

        }
    }

    Vector3 Index2World(int columnIndex, int rowIndex, float y)
    {
        return new Vector3(columnIndex * unitLength + unitLength / 2 - unitLength * unitColumnNum / 2, y, rowIndex * unitLength + unitLength / 2 - unitLength * unitRowNum / 2);
    }

    void World2Index(ref Vector3 worldPos, out int columnIndex, out int rowIndex)
    {
        int x = (int)((worldPos.x + unitLength * unitColumnNum / 2) / unitLength);
        int z = (int)((worldPos.z + unitLength * unitRowNum / 2) / unitLength);
        if (x < 0 || x >= unitColumnNum || z < 0 || z >= unitRowNum)
        {
            rowIndex = -1;
            columnIndex = -1;
        }
        else
        {
            columnIndex = x;
            rowIndex = z;
            Debug.LogFormat("result column index {0} row index {1}", columnIndex, rowIndex);
        }
    }

    void OnDrawGizmos()
    {
        for (int r = 0; r <= unitRowNum; r++)
        {
            Vector3 startPoint = new Vector3(-unitColumnNum * unitLength / 2, 5, r * unitLength - unitRowNum * unitLength / 2);
            Vector3 endPoint = new Vector3(+unitColumnNum * unitLength / 2, 5, r * unitLength - unitRowNum * unitLength / 2);
            Gizmos.DrawLine(startPoint, endPoint);
        }
        for (int c = 0; c <= unitColumnNum; c++)
        {
            Vector3 startPoint = new Vector3(c * unitLength - unitColumnNum * unitLength / 2 , 5, -unitRowNum * unitLength / 2);
            Vector3 endPoint = new Vector3(c * unitLength - unitColumnNum * unitLength / 2 , 5, +unitRowNum * unitLength / 2);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPoint, endPoint);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Index2World(destination.x, destination.y, 5f), 0.25f);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 40, 80, 20), "Produce"))
        {
            Vector3 basePosition = Index2World(5, 5, 0.5f);
            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    GameObject obj = Instantiate(agentTemplate1, basePosition + new Vector3(i * 0.2f, 0, j * 0.2f), Quaternion.Euler(Vector3.zero)) as GameObject;
                    obj.name = string.Format("Agent({0})({1})", i, j);
                    NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
                    agent.SetDestination(destinationWorld);
                    agentSet.Add(agent);
                }
            }

        }
    }
}
