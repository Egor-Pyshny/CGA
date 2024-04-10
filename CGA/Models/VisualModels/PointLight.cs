using System.Numerics;

namespace ObjVisualizer.GraphicsComponents
{
    internal readonly struct PointLight(float x, float y, float z, float intency, bool ambient, bool specular, Vector3 ColorSpecular, Vector3 ColorDiffuce)
    {
        public readonly float X = x;
        public readonly float Y = y;
        public readonly float Z = z;
        public readonly float Intency = intency;

        public readonly float _ambientIntencity = .05f;

        public readonly float _specularIntencity = 1f;

        public readonly float _diffuceIntencity = 1f;


        private readonly bool ambient = ambient;
        private readonly bool specular = specular;

        private readonly Vector3 LightColorAmbient = new Vector3(1,1f,1);

        private readonly Vector3 LightColorDiffuse= new Vector3(1,1f,1);

        private readonly Vector3 LightColorSpecular= new Vector3(1,0f,0);

        public float CalculateLightLaba2(Vector3 point, Vector3 normal)
        {
            Vector3 l = new Vector3(X, Y, Z) - point;
            float lightResult = 0f;
            float angle = Vector3.Dot(normal, l);

            if (angle > 0)
            {
                lightResult = Intency * angle / (l.Length() * normal.Length());
            }

           
            return lightResult;
        }
        public Vector3 CalculateLightLaba3(Vector3 point, Vector3 normal, Vector3 eye)
        {
            Vector3 l = new Vector3(X, Y, Z) - point;
            int s = 10;
            Vector3 lightResult = new(0, 0, 0);
            if (ambient)
                lightResult = LightColorAmbient * _ambientIntencity;
            float angle = Vector3.Dot(normal, l);

            if (angle > 0)
            {
                lightResult += _diffuceIntencity*LightColorDiffuse * Intency * angle / (l.Length() * normal.Length());
            }
            if (specular)
            {
                Vector3 R = 2 * normal * angle - l;
                Vector3 V = eye - point;
                float r_dot_v = Vector3.Dot(R, V);
                if (r_dot_v > 0)
                {
                    lightResult += _specularIntencity * LightColorSpecular * Intency * float.Pow(r_dot_v / (R.Length() * V.Length()), s);
                }
            }
           


            return lightResult;
        }

        public Vector3 CalculateLightWithMaps(Vector3 point, Vector3 normal, Vector3 eye, float specIntenc)
        {
            Vector3 l = new Vector3(X, Y, Z) - point;
            int s = 1;
            Vector3 lightResult = new(0, 0, 0);
            if (ambient)
                lightResult = LightColorAmbient * _ambientIntencity;
            float angle = Vector3.Dot(normal, l);

            if (angle > 0)
            {
                lightResult += _diffuceIntencity * LightColorDiffuse * Intency * angle / (l.Length() * normal.Length());
            }
            if (specular)
            {
                Vector3 R = 2 * normal * angle - l;
                Vector3 V = eye - point;
                float r_dot_v = Vector3.Dot(R, V);
                if (r_dot_v > 0)
                {
                    lightResult += specIntenc / 255 * LightColorSpecular * Intency * float.Pow(r_dot_v / (R.Length() * V.Length()), s);
                }
            }

            return lightResult;
        }
    }
}
