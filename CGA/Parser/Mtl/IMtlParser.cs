using ObjVisualizer.Models.DataModels;

namespace ObjVisualizer.Parser.Mtl
{
    internal interface IMtlParser
    {
        ImageData GetMapKdBytes();
        ImageData GetMapMraoBytes();
        ImageData GetNormBytes();
    }
}
