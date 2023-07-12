using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triangulate a list of points where no vertex lies within the circumcircle of any triangle.
/// </summary>
public class DelaunayTriangulation
{

    public List<Triangle> CalculatedTriangles { get; private set; }

    private Triangle _superTriangle;
    private List<Vector2> _points;
    private List<Triangle> _goodTriangles;
    private List<Triangle> _badTriangles;
    private bool _firstIteration;

    /// <summary>
    /// Setup for a new calculation
    /// </summary>
    private void Setup()
    {
        //initialize our list and mark it as first iteration
        _goodTriangles = new List<Triangle>();
        _badTriangles = new List<Triangle>();
        _firstIteration = true;
    }

    public void CalculateSuperTriangle()
    {
        //Lets get the min and max x positions for the points
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        //Along with the center.
        Vector2 center = Vector2.zero;

        for (int i = 0; i < _points.Count; i++)
        {
            center += _points[i];

            if (_points[i].x < minX)
                minX = _points[i].x;

            if (_points[i].x > maxX)
                maxX = _points[i].x;

            if (_points[i].y < minY)
                minY = _points[i].y;

            if (_points[i].y > maxY)
                maxY = _points[i].y;

        }

        center /= _points.Count;

        //Now that we have min and max, lets calculate width and height
        float width = minX - maxX;
        float height = minY - maxY;

        //Setup the super triangle where we know it will contain all points
        Vector2 p1 = center + (Vector2.up * (height * 3));
        Vector2 p2 = center + (Vector2.down * (height * 3)) + (Vector2.right * (width * 3));
        Vector2 p3 = center + (Vector2.down * (height * 3)) + (Vector2.left * (width * 3));

        _superTriangle = new Triangle(p1, p2, p3, -1, -2, -3);
    }

    public void Calculate(List<Vector2> points)
    {
        //Lets quickly null check and length check to ensure we are working with a valid list of points.
        if (points == null)
        {
            Debug.LogError("Must pass a valid list of points.");
        }
        if (points.Count < 3)
        {
            Debug.LogError("Cannot triangulate only " + points.Count + " points. Requires 3 or more points to triangulate.");
        }

        //Initial setup
        _points = points;
        Setup();

        //Calculate the super triangle
        CalculateSuperTriangle();

        //Loop through all points and triangulate them
        for (int i = 0; i < _points.Count; i++)
            TriangulatePoint(_points[i], i);

        //Remove the super triangle and connected triangles.
        FinishCalculation();
    }

    /// <summary>
    /// Remove the super triangle and all triangles connected to it.
    /// </summary>
    private void FinishCalculation()
    {
        CalculatedTriangles = new List<Triangle>();

        //Loop through all good triangles
        for (int i = 0; i < _goodTriangles.Count; i++)
        {
            //Ensure that it does not contain a point inside the super triangle
            if (!_goodTriangles[i].ContainsPointInSuper(_superTriangle))
                CalculatedTriangles.Add(_goodTriangles[i]);
        }
    }

    /// <summary>
    /// Validate all triangles in the good triangles list, and put the bad ones in the bad triangles list.
    /// </summary>
    /// <param name="point">Point to compare triangles against</param>
    private void ValidateTriangles(Vector2 point)
    {
        //Create a new temp list for the good tris, and clear the bad ones
        List<Triangle> validTris = new List<Triangle>();
        _badTriangles.Clear();

        //If it is our first iteration, then we add the super triangle as a base
        if (_firstIteration)
        {
            _badTriangles.Add(_superTriangle);
            _firstIteration = false;
        }

        //Loop through all of the triangles
        for (int i = 0; i < _goodTriangles.Count; i++)
        {
            //If the circumcircle contains the point, then it is a bad triangle, otherwise, it is valid
            if (_goodTriangles[i].PointInRadius(point))
                _badTriangles.Add(_goodTriangles[i]);
            else
                validTris.Add(_goodTriangles[i]);
        }

        //Ensure that we update the list of good triangles
        _goodTriangles = validTris;
    }

    /// <summary>
    /// Triangulate a point
    /// </summary>
    /// <param name="point">Point to triangulate</param>
    /// <param name="index">Index of point</param>
    private void TriangulatePoint(Vector2 point, int index)
    {
        //First, we need to validate all of the triangles to find the bad ones
        ValidateTriangles(point);

        //Create our list of edges, and edges we don't care about (blacklisted)
        List<Edge> edges = new List<Edge>();
        List<Edge> blacklistedEdges = new List<Edge>();

        //Loop through all of the triangles
        for (int i = 0; i < _badTriangles.Count; i++)
        {
            //Get the edges of triangle i
            foreach (Edge edge in _badTriangles[i].Edges)
            {
                //If we have already seen this edge, then remove it from the list and blacklist it.
                if (edges.Contains(edge))
                {
                    edges.Remove(edge);
                    blacklistedEdges.Add(edge);
                }
                //Otherwise, add the edge as normal
                else
                    edges.Add(edge);
            }
        }

        //Loop through all of the edges and triangulate them with point
        foreach (Edge e in edges)
            _goodTriangles.Add(new Triangle(e.Point1, e.Point2, point, e.Index1, e.Index2, index));

    }
}
