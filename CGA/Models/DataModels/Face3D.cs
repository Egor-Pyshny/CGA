namespace CGA.Models.DataModels
{
    public class Face3D
    {
        public int[] g_vertexes;
        public int[] t_vertexes;
        public int[] n_vectors;
        public Face3D(int[] g_vertexes)
        {
            this.g_vertexes = g_vertexes;
        }
    }
}
