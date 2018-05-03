using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Main : MonoBehaviour
{

    public Camera mCamera;
    public GameObject cube;
    public Transform mapParent;
    public float unitLength = 1;
    public int unitRowNum = 11; //should be odd
    public int unitColumnNum = 21; // should be larger or equal to row num; odd too
    public NavMeshSurface navMesh;

    BitArray bitArray;

    // Use this for initialization
    void Start()
    {
        bitArray = new BitArray(unitRowNum * unitColumnNum);
        mCamera.fieldOfView = unitLength * unitRowNum / 2;
        cube.transform.localScale = new Vector3(unitLength * unitColumnNum, 1, unitLength * unitRowNum);
        navMesh.BuildNavMesh();
    }

    int columnIndex, rowIndex, bitArrayIndex;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldMousePosition = mCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("screen pos ? " + Input.mousePosition);
            Debug.Log(worldMousePosition);
            DetermineIndex(ref worldMousePosition, out columnIndex, out rowIndex);
            bitArrayIndex = rowIndex * unitRowNum + columnIndex;
            if (columnIndex != -1 && rowIndex != -1 && !bitArray.Get(bitArrayIndex))
            {
                BuildOneBlock(mapParent, 
                    new Vector3(
                        columnIndex * unitLength + unitLength /2 - unitLength * unitColumnNum / 2 , 
                        0.9f,
                        rowIndex * unitLength + unitLength / 2 - unitLength * unitRowNum / 2));
                bitArray.Set(rowIndex * unitRowNum + columnIndex, true);
                navMesh.BuildNavMesh();
            }
            else
            {
                Debug.Log("invalid click");
            }

        }
    }

    void BuildOneBlock(Transform parent, Vector3 pos)
    {
        GameObject objTemplate = Resources.Load("BlockWithNavmeshVolume") as GameObject;
        GameObject obj = Instantiate(objTemplate) as GameObject;
        obj.transform.parent = parent;
        obj.transform.position = pos;
    }

    void DetermineIndex(ref Vector3 worldPos, out int columnIndex, out int rowIndex)
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
    }
}
