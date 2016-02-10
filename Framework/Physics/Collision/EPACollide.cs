using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics.Collision.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision
{
    public class EPAFace
    {
        public EPAVertex[] Points = new EPAVertex[3];
        public Vector3 Normal;
        public float Distance;
        public bool Degenerate = false;
        public EPAFace(EPAVertex p1, EPAVertex p2, EPAVertex p3)
        {
            Points[0] = p1;
            Points[1] = p2;
            Points[2] = p3;

            Normal = Vector3.Cross(Points[2].Position - Points[1].Position, Points[0].Position - Points[1].Position);
            Normal.Normalize();
            Distance = Vector3.Dot(Normal, Points[0].Position);

            //Correct the order of the points if winding was backwards
            if (Distance < 0)
            {
                Points[0] = p2;
                Points[1] = p1;
                Normal = Vector3.Cross(Points[2].Position - Points[1].Position, Points[0].Position - Points[1].Position);
                Normal.Normalize();
                Distance = Vector3.Dot(Normal, Points[0].Position);
            }
            if (float.IsNaN(Distance))
                Degenerate = true;
            Points[0].Faces.Add(this);
            Points[1].Faces.Add(this);
            Points[2].Faces.Add(this);
        }
        public int IndexOf(EPAVertex vertex)
        {
            if (vertex == Points[0]) return 0;
            if (vertex == Points[1]) return 1;
            if (vertex == Points[2]) return 2;
            throw new KeyNotFoundException();
        }
        public EPAVertex Next(EPAVertex vertex)
        {
            return Points[(IndexOf(vertex) + 1) % 3];
        }
        public List<EPAEdge> GetEdges()
        {
            return Points.Select(p => new EPAEdge(p, Next(p))).ToList();
        }
    }
    public class EPAVertex
    {
        public List<EPAFace> Faces;
        public Vector3 Position;
        public Vector3 Position1;
        public Vector3 Position2;
        public EPAVertex(Vector3 position, Vector3 position1, Vector3 position2)
        {
            Position = position;
            Position1 = position1;
            Position2 = position2;
            Faces = new List<EPAFace>();
        }
        public override string ToString()
        {
            return Position.ToString() + " (" + Position1.ToString() + ", " + Position2.ToString() + ")";
        }
    }
    public class EPAEdge
    {
        public EPAVertex P1;
        public EPAVertex P2;
        public EPAEdge(EPAVertex P1, EPAVertex P2)
        {
            this.P1 = P1;
            this.P2 = P2;
        }
        public bool Same(EPAEdge other)
        {
            return (P1 == other.P1 && P2 == other.P2) || (P1 == other.P2 && P2 == other.P1);
        }
    }
    public class EPACollide
    {
        static void Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float u, out float v, out float w)
        {
            Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            v = (d11 * d20 - d01 * d21) / denom;
            w = (d00 * d21 - d01 * d20) / denom;
            u = 1.0f - v - w;
        }
        public static bool Detect(ISupportMappable support1, ISupportMappable support2, List<EPAVertex> simplex, Matrix orientation1,
             Matrix orientation2, Vector3 position1, Vector3 position2, Vector3 velocity1, Vector3 velocity2,
             out Vector3 point, out Vector3 normal, out float penetration)
        {
            point = Vector3.Zero;
            normal = Vector3.Zero;
            penetration = 0;
            List<EPAFace> faces = new List<EPAFace>();
            List<EPAVertex> vertices = simplex;
            faces.Add(new EPAFace(vertices[2], vertices[1], vertices[0]));
            faces.Add(new EPAFace(vertices[3], vertices[2], vertices[0]));
            faces.Add(new EPAFace(vertices[0], vertices[1], vertices[3]));
            faces.Add(new EPAFace(vertices[1], vertices[2], vertices[3]));
            int iterationLimit = 50;
            Vector3 s1, s2;
            while (true)
            {
                if (faces.Any(face => face.Degenerate))
                    return false;
                faces.Sort((f1, f2) => f1.Distance.CompareTo(f2.Distance));
                EPAFace closestFace = faces[0];
                normal = faces[0].Normal;
                Vector3 negativeDirection = -normal;
                GJKCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref velocity1, ref negativeDirection, out s1);
                GJKCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref velocity2, ref normal, out s2);
                Vector3 s = s2 - s1;
                penetration = Vector3.Dot(normal, s);
                if (penetration - faces[0].Distance < 0.001)
                    break;
                SimplexInsert(faces, vertices, new EPAVertex(s, s1, s2));

                if (iterationLimit-- == 0)
                    return false;
            }
            EPAVertex[] points = faces[0].Points;
            float u, v, w;
            Barycentric(normal * penetration,
                points[0].Position,
                points[1].Position,
                points[2].Position,
                out u, out v, out w);
            point = points[0].Position1 * u + points[0].Position2 * u +
                points[1].Position1 * v + points[1].Position2 * v +
                points[2].Position1 * w + points[2].Position2 * w;
            point /= 2;
            //point = (s1 + s2) / 2;
            return true;
        }
        private static void SimplexInsert(List<EPAFace> faces, List<EPAVertex> vertices, EPAVertex newVertex)
        {
            var visibleFaces = faces.Where(face => Vector3.Dot(newVertex.Position, face.Normal) > face.Distance).ToList();

            //Remove all the faces, and keep track of the vertices we've affected
            //since they're on the, now open, edge we'll add the new faces on
            List<EPAEdge> openEdges = new List<EPAEdge>();
            foreach (var face in visibleFaces)
            {
                foreach (var edge in face.GetEdges())
                {
                    if (openEdges.RemoveAll(edge1 => edge.Same(edge1)) == 0)
                        openEdges.Add(edge);
                }
                foreach (var point in face.Points)
                {
                    point.Faces.Remove(face);
                }
                faces.Remove(face);
            }
            foreach (var edge in openEdges)
            {
                faces.Add(new EPAFace(edge.P1, edge.P2, newVertex));
            }
            vertices.Add(newVertex);

        }
    }
}
