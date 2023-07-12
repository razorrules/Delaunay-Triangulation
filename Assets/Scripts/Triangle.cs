using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triangle used for Delaunay triangulation. Contains three points for the triangles, the indexes of
/// those triangles, along with a radius and position for the circumcircle of the triangle.
/// </summary>
[System.Serializable]
public struct Triangle : IEquatable<Triangle>
{
    //Points of the triangle
    public Vector2 Point1 { get; private set; }
    public Vector2 Point2 { get; private set; }
    public Vector2 Point3 { get; private set; }

    //Indexes of the points in the triangle
    public int Index1 { get; private set; }
    public int Index2 { get; private set; }
    public int Index3 { get; private set; }

    //Data related to circumcircle
    public Vector2 Center { get; private set; }
    public float Radius { get; private set; }

    public Edge[] Edges { get; private set; }

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3, int index1, int index2, int index3)
    {
        this.Point1 = p1;
        this.Point2 = p2;
        this.Point3 = p3;
        this.Index1 = index1;
        this.Index2 = index2;
        this.Index3 = index3;

        Center = Vector2.zero;
        Radius = 0;
        Edges = new Edge[] { new Edge(Point1, Point2, Index1, Index2), new Edge(Point2, Point3, Index2, Index3), new Edge(Point3, Point1, Index3, Index1) };

        CalculatingCircumcircle();
    }

    public bool ContainsPointInSuper(Triangle superTriangle)
    {
        if (ContainsPoint(superTriangle.Point1))
            return true;
        if (ContainsPoint(superTriangle.Point2))
            return true;
        if (ContainsPoint(superTriangle.Point3))
            return true;
        return false;
    }

    private bool ContainsPoint(Vector2 point)
    {
        if (Point1.Equals(point))
            return true;
        if (Point2.Equals(point))
            return true;
        if (Point3.Equals(point))
            return true;
        return false;
    }

    public bool PointInRadius(Vector2 p)
    {
        return Vector2.Distance(p, Center) < Radius;
    }

    public void CalculatingCircumcircle()
    {
        float d = (Point1.x * (Point2.y - Point3.y) + Point2.x * (Point3.y - Point1.y) + Point3.x * (Point1.y - Point2.y)) * 2;

        float ax = Point1.x * Point1.x;
        float ay = Point1.y * Point1.y;
        float bx = Point2.x * Point2.x;
        float by = Point2.y * Point2.y;
        float cx = Point3.x * Point3.x;
        float cy = Point3.y * Point3.y;

        float x = 1 / d * ((ax + ay) * (Point2.y - Point3.y) + (bx + by) * (Point3.y - Point1.y) + (cx + cy) * (Point1.y - Point2.y));
        float y = 1 / d * ((ax + ay) * (Point3.x - Point2.x) + (bx + by) * (Point1.x - Point3.x) + (cx + cy) * (Point2.x - Point1.x));

        Center = new Vector2(x, y);
        Radius = Mathf.Sqrt(Mathf.Pow(Point1.x - x, 2) + Mathf.Pow(Point1.y - y, 2));
    }

    public bool Equals(Triangle other)
    {
        bool equals = Point1.Equals(other.Point1) && Point2.Equals(other.Point2) && Point3.Equals(other.Point3);
        if (equals)
            return true;
        equals = Point1.Equals(other.Point2) && Point2.Equals(other.Point3) && Point3.Equals(other.Point1);
        if (equals)
            return true;
        return Point1.Equals(other.Point3) && Point2.Equals(other.Point1) && Point3.Equals(other.Point2);
    }
}
