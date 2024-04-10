using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjVisualizer.Models.DataModels
{
    internal class ImageData(int Stride, short ColorSize, int Width, int Height, byte[] MapData)
    {
        private readonly byte[] _mapData = MapData;

        private readonly int _imageStride = Stride;
        private readonly short _imageColorSize = ColorSize;
        private readonly int _imageWidth = Width;
        private readonly int _imageHeight = Height;
        public int Stride { get => _imageStride; }
        public short ColorSize { get => _imageColorSize; }
        public int Width { get => _imageWidth;}
        public int Height { get => _imageHeight;}
        public byte[] MapData { get => _mapData; }
    }
}
