using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

/// <summary>
/// Testing class to visualize and see how the delaunay triangulation works. 
/// 
/// Shout out to Madsbangh on github for providing an awesome package 'EasyButtons' found here:
/// https://github.com/madsbangh/EasyButtons
/// </summary>
public class DelaunayTriangulationTesting : MonoBehaviour
{

    [Header("Generation")]
    [SerializeField] private int _pointsToGenerate;
    [SerializeField] private float _maxSize;

    [Header("Mesh")]
    [SerializeField] private GameObject _objectWithMeshFilter;
    [SerializeField] private float _perlinSize;
    [SerializeField] private float _perlinNoiseScale;

    private VoronoiDiagram _voronoi = new VoronoiDiagram();
    private DelaunayTriangulation _triangulation;
    private List<Vector2> _points;
#if UNITY_EDITOR
    //This is only ever used to flag if we draw the handles for circumcircle, so lets also wrap it in unity editor.
    private bool _doDrawCircumcircle;
#endif

    [Button("Toggle Circumcircle")]
    public void ToggleCircumcircle()
    {
        _doDrawCircumcircle = !_doDrawCircumcircle;
#if UNITY_EDITOR
        UnityEditor.EditorWindow view = UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>();
        view.Repaint();
#endif
    }

    [Button]
    public void Generate()
    {
        if (_triangulation == null)
            _triangulation = new DelaunayTriangulation();

        //Create new list of points
        _points = new List<Vector2>();

        //Give each point a random position based off max size
        for (int i = 0; i < _pointsToGenerate; i++)
            _points.Add(new Vector2(Random.Range(0, _maxSize), Random.Range(0, _maxSize)));

        //Triangulate the points
        _triangulation.Calculate(_points);

        //Lets repaint scene so it updates immediately
#if UNITY_EDITOR
        UnityEditor.EditorWindow view = UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>();
        view.Repaint();
#endif
        _voronoi.SetTris(_triangulation.CalculatedTriangles);
    }

    /// <summary>
    /// Generate the mesh based off of the triangulated points from delaunay triangulation
    /// </summary>
    [Button]
    public void GenerateMesh()
    {
        if (_triangulation == null || _points == null)
        {
            Debug.LogError("Please generate the triangulation before attempting to generate the mesh.");
            return;
        }

        Mesh mesh;

        //Lets try and get the mesh
        if (_objectWithMeshFilter.TryGetComponent(out MeshFilter meshFilter))
        {
            mesh = meshFilter.mesh;
            mesh.Clear();
        }
        else
        {
            Debug.LogError("There is no object with mesh filter to store the mesh in.");
            return;
        }

        //Calculate the verts of the mesh
        int[] verts = new int[_triangulation.CalculatedTriangles.Count * 3];

        for (int i = 0; i < _triangulation.CalculatedTriangles.Count; i++)
        {
            verts[i * 3] = _triangulation.CalculatedTriangles[i].Index1;
            verts[i * 3 + 1] = _triangulation.CalculatedTriangles[i].Index2;
            verts[i * 3 + 2] = _triangulation.CalculatedTriangles[i].Index3;

        }

        //Take the points and turn them into a vector3 while also applying some perlin noise to them for height.
        mesh.vertices = ToVector3Array(_points.ToArray());
        mesh.triangles = verts;
        mesh.uv = _points.ToArray();

    }

    public Vector3[] ToVector3Array(Vector2[] array)
    {
        return System.Array.ConvertAll(array, ToVector3WithPerlin);
    }

    private Vector3 ToVector3WithPerlin(Vector2 point)
    {
        return new Vector3(point.x, Mathf.PerlinNoise(point.x / 10.0f, point.y / 10.0f) * _perlinNoiseScale, point.y);
    }

    private void OnValidate()
    {
        //Lets validate here to ensure we have at minimum 3 points
        if (_pointsToGenerate < 3)
            _pointsToGenerate = 3;
    }

    /// <summary>
    /// Draw all the gizmos so we can see what is going on
    /// </summary>
    public void OnDrawGizmos()
    {
        //Lets first null check and ensure there will be no errors or null references
        if (_triangulation == null || _triangulation.CalculatedTriangles == null || _points == null)
            return;

        _voronoi.OnDrawGizmos();

        //Loop through all points and draw them
        for (int i = 0; i < _points.Count; i++)
        {
            Gizmos.DrawSphere(new Vector3(_points[i].x, 0, _points[i].y), .5f);
        }

        //If we have calculated triangles, lets draw them
        if (_triangulation.CalculatedTriangles != null)
            foreach (Triangle tri in _triangulation.CalculatedTriangles)
                DrawTriangle(tri);

    }

    /// <summary>
    /// Draw a triangle from Delaunay's triangulation
    /// </summary>
    /// <param name="triangle"></param>
    public void DrawTriangle(Triangle triangle)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(triangle.Point1.x, 0, triangle.Point1.y), 1f);
        Gizmos.DrawSphere(new Vector3(triangle.Point2.x, 0, triangle.Point2.y), 1f);
        Gizmos.DrawSphere(new Vector3(triangle.Point3.x, 0, triangle.Point3.y), 1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(triangle.Point1.x, 0, triangle.Point1.y), new Vector3(triangle.Point2.x, 0, triangle.Point2.y));
        Gizmos.DrawLine(new Vector3(triangle.Point3.x, 0, triangle.Point3.y), new Vector3(triangle.Point2.x, 0, triangle.Point2.y));
        Gizmos.DrawLine(new Vector3(triangle.Point1.x, 0, triangle.Point1.y), new Vector3(triangle.Point3.x, 0, triangle.Point3.y));

#if UNITY_EDITOR
        if (_doDrawCircumcircle)
            UnityEditor.Handles.DrawWireDisc(new Vector3(triangle.Center.x, 0, triangle.Center.y), Vector3.up, triangle.Radius);
#endif

    }

}
