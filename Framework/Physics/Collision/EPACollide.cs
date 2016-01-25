using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Physics.Collision
{
    class EPAFace
    {
        public EPAVertex[] Points = new EPAVertex[3];
        public Vector3 Normal;
        public float Distance;
        public EPAFace(EPAVertex p1, EPAVertex p2, EPAVertex p3)
        {
            Points[0] = p1;
            Points[1] = p2;
            Points[2] = p3;

            Normal = Vector3.Cross(Points[2].Position - Points[1].Position, Points[0].Position - Points[1].Position);
            Normal.Normalize();
            Distance = Vector3.Dot(Normal, Points[0].Position);
            if(float.IsNaN(Distance))
            {

            }

            //Correct the order of the points if winding was backwards
            if(Distance < 0)
            {
                Points[0] = p2;
                Points[1] = p1;
                Normal = Vector3.Cross(Points[2].Position - Points[1].Position, Points[0].Position - Points[1].Position);
                Normal.Normalize();
                Distance = Vector3.Dot(Normal, Points[0].Position);
            }
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
    class EPAVertex
    {
        public List<EPAFace> Faces;
        public Vector3 Position;
        public EPAVertex(Vector3 position)
        {
            Position = position;
            Faces = new List<EPAFace>();
        }
        public override string ToString()
        {
            return Position.ToString();
        }
    }
    class EPAEdge
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
    class EPACollide
    {
        public static bool Detect(ISupportMappable support1, ISupportMappable support2, List<Vector3> simplex, Matrix orientation1,
             Matrix orientation2, Vector3 position1, Vector3 position2, Vector3 velocity1, Vector3 velocity2,
             out Vector3 point, out Vector3 normal, out float penetration)
        {
            List<EPAFace> faces = new List<EPAFace>();
            List<EPAVertex> vertices = new List<EPAVertex>();
            vertices.Add(new EPAVertex(simplex[0]));
            vertices.Add(new EPAVertex(simplex[1]));
            vertices.Add(new EPAVertex(simplex[2]));
            vertices.Add(new EPAVertex(simplex[3]));
            faces.Add(new EPAFace(vertices[2], vertices[1], vertices[0]));
            faces.Add(new EPAFace(vertices[3], vertices[2], vertices[0]));
            faces.Add(new EPAFace(vertices[0], vertices[1], vertices[3]));
            faces.Add(new EPAFace(vertices[1], vertices[2], vertices[3]));
            int iterationLimit = 50;
            Vector3 s1, s2;
            while (true)
            {
                faces.Sort((f1, f2) => f1.Distance.CompareTo(f2.Distance));
                EPAFace closestFace = faces[0];
                normal = faces[0].Normal;
                Vector3 negativeDirection = -normal;
                GJKCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref velocity1, ref negativeDirection, out s1);
                GJKCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref velocity2, ref normal, out s2);
                point = s2 - s1;
                penetration = Vector3.Dot(normal, point);
                if (penetration - faces[0].Distance < 0.01)
                    break;
                SimplexInsert(faces, vertices, new EPAVertex(point));

                if (iterationLimit-- == 0)
                    return false;
            }
            point = (s1 + s2) / 2;
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
