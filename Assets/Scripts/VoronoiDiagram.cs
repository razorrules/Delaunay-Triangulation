using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoronoiDiagram
{

    private struct TouchingTriangles
    {
        public TouchingTriangles(Triangle triangle, List<Triangle> touching)
        {
            this.triangle = triangle;
            this.touching = touching;
        }

        public Triangle triangle;
        public List<Triangle> touching;
    }

    private List<TouchingTriangles> _tris;

    public void SetTris(List<Triangle> tris)
    {
        if (_tris == null)
            _tris = new List<TouchingTriangles>();
        else
            _tris.Clear();

        List<Triangle> touching = new List<Triangle>();
        for (int i = 0; i < tris.Count; i++)
        {
            touching.Clear();

            for (int j = 0; j < tris.Count; j++)
            {
                if (i == j)
                    continue;
                foreach (Edge e in tris[j].Edges)
                {
                    if (tris[i].Edges.Contains(e))
                    {
                        Debug.Log($"Triangle: {i} shares an edge with: {j}");
                        touching.Add(tris[j]);
                        continue;
                    }
                }
            }
            _tris.Add(new TouchingTriangles(tris[i], touching));
        }
    }

    public void OnDrawGizmos()
    {
        if (_tris == null || _tris.Count == 0)
            return;

        Color cachedColor = Gizmos.color;
        Gizmos.color = Color.blue;

        foreach (TouchingTriangles triangle in _tris)
        {

            Gizmos.DrawSphere(new Vector3(triangle.triangle.Center.x, 0, triangle.triangle.Center.y), 1);

            foreach (Triangle touching in triangle.touching)
            {
                Gizmos.DrawLine(new Vector3(triangle.triangle.Center.x, 0, triangle.triangle.Center.y),
                    new Vector3(touching.Center.x, 0, touching.Center.y));
            }
        }

        Gizmos.color = cachedColor;
    }

}
