// Rubik Cube 3D presentation and solution
// is developed by Halyna Shashyna
// Halyna.Shashyna@gmail.com
// mesh.cs
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Kit3D.Windows.Controls;
using Kit3D.Windows.Media;
using Kit3D.Windows.Media.Media3D;
using System.Windows.Media.Imaging;



namespace RubikCube
{
    class form_cube_side
    {
        
        private MeshGeometry3D mesh;
        private Vector3D[] vectors = {
                               new Vector3D(0,0,1),
                               new Vector3D(0,0,-1),
                               new Vector3D(1, 0, 0),
                               new Vector3D(-1, 0, 0),
                               new Vector3D(0,1,0),
                               new Vector3D(0,-1,0)
                                };

        private int[,] triangles = { 
                               {1,2,3,0}, 
                               {4,7,6,5},
                               {0,3,7,4},
                               {5,6,2,1},
                               {5,1,0,4},
                               {2,6,7,3}
                               };



        public form_cube_side(int i, int j, int k, int c, int N)
        {
            double x, y, z;
            MeshGeometry3D _mesh = new MeshGeometry3D();

            int shift = 2 * ((int)(N / 2) + 1);
            if ((N % 2) == 0)
            {
                shift = N + 1;
            }

            
            double length = 2;
            double s = (length/2)*0.90;
            double shiftDouble = shift*length / 2;
            x = ((double)(i + 1)) * length - shiftDouble;
            y = ((double)(j + 1)) * length - shiftDouble;
            z = ((double)(k + 1)) * length - shiftDouble;
            


            Point3D[] P = new Point3D[8];

            P[0] = new Point3D(x + s, y + s, z + s);
            P[1] = new Point3D(x - s, y + s, z + s);
            P[2] = new Point3D(x - s, y - s, z + s);
            P[3] = new Point3D(x + s, y - s, z + s);

            P[4] = new Point3D(x + s, y + s, z - s);
            P[5] = new Point3D(x - s, y + s, z - s);
            P[6] = new Point3D(x - s, y - s, z - s);
            P[7] = new Point3D(x + s, y - s, z - s);

            _mesh.Positions.Add(P[triangles[c, 0]]);
            _mesh.Positions.Add(P[triangles[c, 1]]);
            _mesh.Positions.Add(P[triangles[c, 2]]);
            _mesh.Positions.Add(P[triangles[c, 3]]);

            
            _mesh.TriangleIndices.Add(0);
            _mesh.TriangleIndices.Add(1);
            _mesh.TriangleIndices.Add(2);

            _mesh.TriangleIndices.Add(0);
            _mesh.TriangleIndices.Add(2);
            _mesh.TriangleIndices.Add(3);
            
            _mesh.TextureCoordinates.Add(new Point(0, 0));
            _mesh.TextureCoordinates.Add(new Point(0, 1));
            _mesh.TextureCoordinates.Add(new Point(1, 1));
            _mesh.TextureCoordinates.Add(new Point(1, 0));

            mesh = _mesh;
        }
        public MeshGeometry3D give_me_mesh()
        {
            return mesh;

        }
    }
}
