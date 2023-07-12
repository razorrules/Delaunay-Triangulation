using UnityEngine;
using System;

/// <summary>
/// An edge of a triangle containing the two points along with indexes of those points
/// </summary>
public struct Edge : IEquatable<Edge>
{

    public Vector2 Point1 { get; private set; }
    public Vector2 Point2 { get; private set; }

    public int Index1 { get; private set; }
    public int Index2 { get; private set; }

    public Edge(Vector2 p1, Vector2 p2, int index1, int index2)
    {
        this.Point1 = p1;
        this.Point2 = p2;
        this.Index1 = index1;
        this.Index2 = index2;
    }

    public bool Equals(Edge other)
    {
        if (Point1.Equals(other.Point1) && Point2.Equals(other.Point2))
            return true;
        return Point2.Equals(other.Point1) && Point1.Equals(other.Point2);
    }
}
