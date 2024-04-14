using ObjVisualizer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjVisualizer.GraphicsComponents
{
    internal class GraphicsObject(ImageData Kd, ImageData Mrao, ImageData Norm, ImageData EmiMap)
    {
        private readonly ImageData _kdMap = Kd;
        private readonly ImageData _mraoMap = Mrao;
        private readonly ImageData _normMap = Norm;
        private readonly ImageData _emiMap = EmiMap;

        public ImageData KdMap { get =>  _kdMap; }
        public ImageData MraoMap { get =>  _mraoMap; }
        public ImageData NormMap { get =>  _normMap; }
        public ImageData EmiMap { get => _emiMap; }
    }
}
