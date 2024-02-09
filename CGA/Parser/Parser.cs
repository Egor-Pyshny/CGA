using CGA.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CGA.Parser
{
    public class OBJParser
    {
        private List<Vector4> vertexes_g = new List<Vector4>();
        private List<Face3D> faces = new List<Face3D>();

        public void parseFile()
        {
            foreach (string line in System.IO.File.ReadLines("shuttle.obj"))
            {
                string[] values = line.TrimEnd().Split(' ');
                switch (values[0])
                {
                    case "v":

                        Vector4 point = new Vector4(
                            float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture),
                            1
                        );
                        vertexes_g.Add(point);
                        break;
                    case "f":
                        List<int> indexes = new List<int>();
                        if (line.Contains("/") || line.Contains("//"))
                        {
                            for (int i = 1; i < values.Length; i++)
                            {
                                var s = values[i];
                                var temp = s.Split('/');
                                indexes.Add(Convert.ToInt32(temp[0]) - 1);
                            }
                        }
                        else
                        {
                            for (int i = 1; i < values.Length; i++)
                            {
                                var s = values[i];
                                indexes.Add(Convert.ToInt32(s) - 1);
                            }
                        }
                        Face3D polygon = new Face3D(indexes.ToArray());
                        faces.Add(polygon);
                        break;
                }
            }
        }

        public Mesh getMesh()
        {
            return new Mesh(this.faces, this.vertexes_g);
        }

    }
}
