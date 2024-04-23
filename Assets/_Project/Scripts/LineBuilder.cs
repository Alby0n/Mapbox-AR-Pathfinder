using UnityEngine;
using System.Collections.Generic;
//This script, `LineBuilder`, is responsible for dynamically creating a line mesh based on a list of 2D points. 
//Overall, this script provides a simple way to dynamically generate a line mesh based on a list of points, allowing for flexible rendering of lines in a Unity environment.
public class LineBuilder : MonoBehaviour
{
    public List<Vector2> Points;
    public float Width = 1.0f;
    public Material Material;

    private float width => Width / 100;

    //- `Points`: A list of 2D points representing the path of the line.
   //- `Width`: The width of the line.
   //- `Material`: The material used to render the line.


    public struct Corner
    { //The `Corner` struct holds information about the left and right vertices of a corner.
        public Vector2 Left;
        public Vector3 Right;
    }

    public void Start()
    {//   - The `Start` method is called when the script initializes, triggering the construction of the line.
        build();
    }

    static Vector3 Vector2ToVector3(Vector2 v)
    { //- The `Vector2ToVector3` method converts a Vector2 point to a Vector3 with the y-component set to zero.
        return new Vector3(v.x, 0, v.y);
    }

    void OnValidate()
    { //   - The `OnValidate` method is called when the script's values are changed in the Unity Editor. It ensures that the line is rebuilt if changes occur while the game is running.
        if (Application.IsPlaying(this))
        {

            build();
        }
    }

    void OnDisable()
    { //   - The `OnDisable` method is called when the GameObject this script is attached to is disabled. It destroys the previously created line GameObject to avoid memory leaks.
        // if (go != null)
        // {
            go.Destroy();
        // }
    }

    void build()
    { //- The `build` method triggers the construction of the line. In this implementation, it calls the `buildDumb` method.
        buildDumb();
    }

    private GameObject go;
    private void buildDumb()
    { //   - The `buildDumb` method creates a new GameObject to represent the line, adds a MeshFilter and MeshRenderer component to it, and calls `BuildLineMesh` to construct the line mesh.
        if (go != null)
        {
            go.Destroy();
        }

        go = new GameObject("LINE");
        var mesh = go.AddComponent<MeshFilter>().mesh;
        var meshRenderer = go.AddComponent<MeshRenderer>();

        BuildLineMesh(Points, mesh, width);
          // - The `BuildLineMesh` method constructs the line mesh using the provided list of points. It calculates vertices, normals, and indices to create the mesh.
  // - It iterates over the points, calculates the left and right vertices for each segment based on the width, and adds them to the vertices list.
   //- It calculates indices to form triangles between vertices.
   //- It also calculates vertices for corners by finding the center point between adjacent segments and adding triangles to connect them.


        meshRenderer.sharedMaterial = Material;
    }

    public static void BuildLineMesh(List<Vector2> points, Mesh mesh, float width) {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var indices = new List<int>();

        Debug.Assert(points.Count >= 2);

        var ic = 0;
        for (var i = 0; i < points.Count - 1; i += 1)
        {
            var ab = (points[i + 1] - points[i]).normalized;
            var segLen = (points[i + 1] - points[i]).magnitude;
            var tab = new Vector2(-ab.y, ab.x);

            var aLeft2d = points[i] - tab * width;
            var aRight2d = points[i] + tab * width;

            vertices.Add(Vector2ToVector3(aLeft2d));
            vertices.Add(Vector2ToVector3(aRight2d));
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);

            var bLeft2D = aLeft2d + segLen * ab;
            var bRight2D = aRight2d + segLen * ab;

            vertices.Add(Vector2ToVector3(bLeft2D));
            vertices.Add(Vector2ToVector3(bRight2D));
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);

            indices.AddRange(new int[] { ic ,ic+ 1,ic+ 2,ic+ 1,ic+ 3,ic+ 2 });
            ic += 4;
        }

        for (var i = 0; i < points.Count - 2; i+= 1) {
            var firstRight = vertices[4 * i + 3];
            var firstLeft = vertices[4 * i + 2];
            var secondLeft = vertices[4 * (i+1)];
            var secondRight = vertices[4 * (i+1) + 1];

            var center = (firstRight + firstLeft) / 2;

            vertices.AddRange(new Vector3[]{center, firstRight, secondRight});
            vertices.AddRange(new Vector3[]{center, secondLeft, firstLeft});

            for (var k = 0; k < 6;k++) {
                normals.Add(Vector3.up);
            }

            indices.AddRange(new int[]{ ic, ic + 1, ic +2, ic+3, ic+4, ic+5 });
            ic += 6;
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
    }


    private void GetCornerVertices(Vector2 a, Vector2 b, Vector2 c)
    {
        Corner corner = new Corner { };

        Vector2 ab = (b - a).normalized;
        Vector2 tab = new Vector2(-ab.y, ab.x);
    }
}
