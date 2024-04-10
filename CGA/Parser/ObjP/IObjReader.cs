using ObjVisualizer.Models.DataModels;
using System.Collections.Generic;
using System.Numerics;

namespace ObjVisualizer.Parser.Obj
{
    internal interface IObjReader
    {
        IEnumerable<Vector4> Vertices { get; }
        IEnumerable<Vector3> VertexTextures { get; }
        IEnumerable<Vector3> VertexNormals { get; }
        IEnumerable<Face> Faces { get; }
    }
}
