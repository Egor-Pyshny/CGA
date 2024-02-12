using CGA.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CGA.Models.DataModels
{
    public class Mesh
    {
        private List<Face3D> faces;
        private List<Vector4> vertexes;

        public Mesh(List<Face3D> faces, List<Vector4> vertexes_g)
        {
            vertexes = vertexes_g;
            this.faces = faces;
        }

        public static Mesh loadMesh(string path)
        {
            OBJParser parser = new OBJParser(path);
            parser.parseFile();
            return parser.getMesh();
        }

        public List<Vector4> getVertexes()
        {
            return vertexes;
        }

        public List<Vector4> GetPositionsInWorldModel(Matrix4x4 worldModel)
        {
            var result = new List<Vector4>();
            foreach (var position in vertexes)
            {
                var temp = Vector4.Transform(position, worldModel);
                result.Add(temp/temp.W);
            }
            return result;
        }

        public List<Face3D> getFaces()
        {
            return faces;
        }

        public void setFaces(List<Face3D> faces)
        {
            this.faces = faces;
        }
    }
}
