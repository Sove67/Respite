using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Radial_Part : MonoBehaviour
{
    public RectTransform container;
    public GameObject icon;
    public int id { get; private set; }

    public class OrderClockwise : IComparer<Vector3>
    {
        Vector3 center;

        public OrderClockwise(Vector3 center)
        { this.center = center; }

        public int Compare(Vector3 a, Vector3 b)
        {
            Vector3 one = a - center;
            Vector3 two = b - center;
            float angle1 = (Mathf.Atan2(one.x, one.y) + (2 * Mathf.PI)) % (2 * Mathf.PI);
            float angle2 = (Mathf.Atan2(two.x, two.y) + (2 * Mathf.PI)) % (2 * Mathf.PI);
            float difference = angle1 * Mathf.Rad2Deg - angle2 * Mathf.Rad2Deg;
            return (int)difference;
        }
    }

    

    public void Create(int id, float innerRadius, float outerRadius, float scale, int partCount, float spacing, int index, Material material, Sprite sprite)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertecies = new List<Vector3>();
        List<int> triangles = new List<int>();

        float angle = 360 / partCount;
        
        // Assign the vertecies for that mark
        vertecies.Add(Quaternion.Euler(0, 0, -(angle * index + spacing / 2)) * new Vector3(0, innerRadius, 0));
        vertecies.Add(Quaternion.Euler(0, 0, -(angle * (index + 1) - spacing / 2)) * new Vector3(0, innerRadius, 0));
        vertecies.Add(Quaternion.Euler(0, 0, -(angle * index + spacing / 2)) * new Vector3(0, outerRadius, 0));
        vertecies.Add(Quaternion.Euler(0, 0, -(angle * (index + 1) - spacing / 2)) * new Vector3(0, outerRadius, 0));

        Vector3 midpoint = Vector3.zero;
        for (int i = 0; i < vertecies.Count; i++)
        {
            Vector3 point = vertecies[i];
            midpoint += point;
            vertecies[i] = new Vector3(point.x * scale, point.y * scale, point.z);
        }
        midpoint /= vertecies.Count;

        vertecies.Sort(new OrderClockwise(midpoint));

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(0);

        mesh.vertices = vertecies.ToArray();
        mesh.triangles = triangles.ToArray();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().material = material;

        icon.GetComponent<SpriteRenderer>().sprite = sprite;

        float width = outerRadius - innerRadius;
        container.transform.localScale = new Vector3(width * .75f, width * .75f);
        container.anchoredPosition = midpoint * 2;
    }
}
