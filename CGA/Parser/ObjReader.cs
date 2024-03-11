using ObjVisualizer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace ObjVisualizer.Parser
{
    internal sealed class ObjReader : IObjReader
    {
        public IEnumerable<Vector4> Vertices => _vertices;
        public IEnumerable<Vector3> VertexTextures => _vertexTextures;
        public IEnumerable<Vector3> VertexNormals => _vertexNormals;
        public IEnumerable<Face> Faces => _faces;

        private static readonly Dictionary<string, ObjReader> _readers = [];

        private readonly Dictionary<string, Action<string[]>> _actions;

        private readonly List<Vector4> _vertices = [];
        private readonly List<Vector3> _vertexTextures = [];
        private readonly List<Vector3> _vertexNormals = [];
        private readonly List<Face> _faces = [];

        private ObjReader(string filename)
        {
            _actions = new()
            {
                ["v"] = AddVertex,
                ["vt"] = AddVertexTexture,
                ["vn"] = AddVertexNormal,
                ["f"] = AddFace
            };

            using var reader = new StreamReader(filename);

            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                var data = line.Split(' ');

                if (_actions.TryGetValue(data[0], out Action<string[]>? value))
                {
                    value.Invoke(data);
                }
            }
        }

        public static IObjReader GetObjReader(string filename)
        {
            if (_readers.TryGetValue(filename, out var reader))
            {
                return reader;
            }

            reader = new(filename);

            _readers.Add(filename, reader);

            return reader;
        }

        private void AddVertex(string[] data)
        {
            var coords = data[1..].Where(d => d != string.Empty).ToArray();

            if (coords.Length == 4)
            {
                _vertices.Add(new(
                    float.Parse(coords[0], CultureInfo.InvariantCulture),
                    float.Parse(coords[1], CultureInfo.InvariantCulture),
                    float.Parse(coords[2], CultureInfo.InvariantCulture),
                    float.Parse(coords[3], CultureInfo.InvariantCulture)));
            }

            if (coords.Length == 3)
            {
                _vertices.Add(new(
                    float.Parse(coords[0], CultureInfo.InvariantCulture),
                    float.Parse(coords[1], CultureInfo.InvariantCulture),
                    float.Parse(coords[2], CultureInfo.InvariantCulture),
                    1.0f));
            }
        }

        private void AddVertexTexture(string[] data)
        {
            var coords = data[1..].Where(d => d != string.Empty).ToArray();

            if (coords.Length == 1)
            {
                _vertexTextures.Add(new(float.Parse(coords[0], CultureInfo.InvariantCulture), 0, 0));
            }
            if (coords.Length == 2)
            {
                _vertexTextures.Add(new(
                    float.Parse(coords[0], CultureInfo.InvariantCulture),
                    float.Parse(coords[1], CultureInfo.InvariantCulture),
                    0));
            }
            if (coords.Length == 3)
            {
                _vertexTextures.Add(new(
                    float.Parse(coords[0], CultureInfo.InvariantCulture),
                    float.Parse(coords[1], CultureInfo.InvariantCulture),
                    float.Parse(coords[2], CultureInfo.InvariantCulture)));
            }
        }

        private void AddVertexNormal(string[] data)
        {
            var coords = data[1..].Where(d => d != string.Empty).ToArray();

            _vertexNormals.Add(new(
                float.Parse(coords[0], CultureInfo.InvariantCulture),
                float.Parse(coords[1], CultureInfo.InvariantCulture),
                float.Parse(coords[2], CultureInfo.InvariantCulture)));
        }

        private void AddFace(string[] data)
        {
            List<int> vs = [];
            List<int> vns = [];
            List<int> vts = [];

            var coords = data[1..].Where(d => d != string.Empty).ToArray();

            for (int i = 0; i < coords.Length; i++)
            {
                var elem = coords[i].Split('/');

                int vId = int.Parse(elem[0]);
                if (vId != -1)
                {
                    vs.Add(vId);
                }
                else
                {
                    vs.Add(_vertices.Count);
                }

                int vtId;
                int vnId;

                if (elem.Length > 1)
                {
                    if (elem[1] != string.Empty)
                    {
                        vtId = int.Parse(elem[1]);

                        if (vtId != -1)
                        {
                            vts.Add(vtId);
                        }
                        else
                        {
                            vts.Add(_vertexTextures.Count);
                        }
                    }
                    else
                    {
                        vnId = int.Parse(elem[2]);

                        if (vnId != -1)
                        {
                            vns.Add(vnId);
                        }
                        else
                        {
                            vns.Add(_vertexNormals.Count);
                        }
                    }
                }
                if (elem.Length > 2)
                {
                    vnId = int.Parse(elem[2]);

                    if (vnId != -1)
                    {
                        vns.Add(vnId);
                    }
                    else
                    {
                        vns.Add(_vertexNormals.Count);
                    }
                }
            }

            _faces.Add(new(vs, vts, vns));
        }
    }
}