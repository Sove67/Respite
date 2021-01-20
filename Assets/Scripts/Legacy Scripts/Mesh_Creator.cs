using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mesh_Creator : MonoBehaviour
{
    // Variables
    // Script Link
    public Grid_Generator Grid_Generator;

    // Mesh Rendering
    private readonly List<MeshContainer> meshList = new List<MeshContainer> { };
    public GameObject meshPrefab;
    public Material baseMaterial;

    // Classes
    public class MeshContainer
    {
        public GameObject gameObject { get; set; }
        public List<Vector3> vertecies { get; set; }
        public List<int> triangles { get; set; }
        public MeshContainer(GameObject gameObject, List<Vector3> vertecies, List<int> triangles)
        {
            this.gameObject = gameObject;
            this.vertecies = vertecies;
            this.triangles = triangles;
        }
    }

    public class VertexToggle
    {
        public bool topLeft { get; set; }
        public bool topRight { get; set; }
        public bool bottomLeft { get; set; }
        public bool bottomRight { get; set; }
        public VertexToggle(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
        }
    }

    // Functions
    public void CreateMeshes()
    {
        for (int i = 0; i < Grid_Generator.sectionCount; i++)
        {
            // Create an empty mesh and assign it to "newMesh"
            MeshContainer newMesh;
            meshList.Add(new MeshContainer(Instantiate(meshPrefab, this.transform), new List<Vector3> { }, new List<int> { }));
            newMesh = meshList[meshList.Count - 1];
            newMesh.gameObject.name = ("Mesh (" + i + ")");

            //Fill the mesh
            newMesh = AssignValues(i, newMesh);
            SendMesh(newMesh);
        }
    }

    public MeshContainer AssignValues(int id, MeshContainer newMesh)
    {
        List<Vector3Int> startingSectors = new List<Vector3Int> { };

        // for each section, check connections and ID. if connected and the ID matches
        for (int y = 0; y < Grid_Generator.gridDimensions.y; y++)
        {
            for (int x = 0; x < Grid_Generator.gridDimensions.x; x++)
            {
                if (Grid_Generator.sectorGrid[x, y].id == id && !Grid_Generator.sectorGrid[x, y].crawled)
                {
                    int length = 0;
                    while (x + length < Grid_Generator.gridDimensions.x && Grid_Generator.sectorGrid[x + length, y].id == id)
                    {
                        Grid_Generator.sectorGrid[x + length, y].crawled = true;
                        length++;
                    }
                    length--;

                    startingSectors.Add(new Vector3Int(x, y, length));

                    // 2 3
                    // 0 1
                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[x, y].vertexPosition[0].x, 0, Grid_Generator.sectorGrid[x, y].vertexPosition[0].y));
                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[x + length, y].vertexPosition[1].x, 0, Grid_Generator.sectorGrid[x + length, y].vertexPosition[1].y));
                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[x, y].vertexPosition[2].x, 0, Grid_Generator.sectorGrid[x, y].vertexPosition[2].y));
                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[x + length, y].vertexPosition[3].x, 0, Grid_Generator.sectorGrid[x + length, y].vertexPosition[3].y));
                }
            }
        }

        for (int i = 0; i < startingSectors.Count; i++)
        {
            List<int> indexes = StreetCrawler(i, id, startingSectors);

            if (indexes.Count >= 2)
            {
                for (int a = 0; a < indexes.Count; a += 2)
                {
                    int start = indexes[a];
                    int end = indexes[a + 1];

                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[start, startingSectors[i].y].vertexPosition[2].x, 0, Grid_Generator.sectorGrid[start, startingSectors[i].y].vertexPosition[2].y + Grid_Generator.streetWidth));
                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[end, startingSectors[i].y].vertexPosition[3].x, 0, Grid_Generator.sectorGrid[end, startingSectors[i].y].vertexPosition[3].y + Grid_Generator.streetWidth));
                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[start, startingSectors[i].y].vertexPosition[2].x, 0, Grid_Generator.sectorGrid[start, startingSectors[i].y].vertexPosition[2].y));
                    newMesh.vertecies.Add(new Vector3(Grid_Generator.sectorGrid[end, startingSectors[i].y].vertexPosition[3].x, 0, Grid_Generator.sectorGrid[end, startingSectors[i].y].vertexPosition[3].y));
                }
            }
        }

        for (int i = 0; i < newMesh.vertecies.Count; i += 4)
        {
            //   3       2 3
            // 0 1   +   0

            newMesh.triangles.Add(i + 0);
            newMesh.triangles.Add(i + 3);
            newMesh.triangles.Add(i + 1);

            newMesh.triangles.Add(i + 2);
            newMesh.triangles.Add(i + 3);
            newMesh.triangles.Add(i + 0);
        }

        return (newMesh);
    }

    public List<int> StreetCrawler(int i, int id, List<Vector3Int> startingSectors)
    {
        bool marking = false;
        int end = 0;
        List<int> indexes = new List<int> { };
        for (int x = startingSectors[i].x; x < Grid_Generator.gridDimensions.x; x++)
        {
            if (startingSectors[i].y + 1 < Grid_Generator.gridDimensions.y)
            {
                if (Grid_Generator.sectorGrid[x, startingSectors[i].y].id == id && Grid_Generator.sectorGrid[x, startingSectors[i].y + 1].id == id && !marking)
                {
                    marking = true;
                    indexes.Add(x);
                }

                if (marking && (Grid_Generator.sectorGrid[x, startingSectors[i].y].id != id || Grid_Generator.sectorGrid[x, startingSectors[i].y + 1].id != id))
                {
                    marking = false;
                    indexes.Add(x - 1);
                }
                end = x;
            }
        }
        if (marking)
        { indexes.Add(end); }

        return (indexes);
    }

    public void SendMesh(MeshContainer newMesh)
    {
        Mesh mesh = new Mesh
        {
            vertices = newMesh.vertecies.ToArray(),
            triangles = newMesh.triangles.ToArray()
        };

        newMesh.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        newMesh.gameObject.GetComponent<MeshRenderer>().material = baseMaterial;
        newMesh.gameObject.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, .5f, .5f, .25f, .75f, 1f, 1f);
    }
}
