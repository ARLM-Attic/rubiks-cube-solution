// Rubik Cube 3D presentation and solution
// is developed by Halyna Shashyna
// Halyna.Shashyna@gmail.com
// mirror.cs
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

namespace RubikCube
{
    class visualmodel_mirror
    {

        private ModelVisual3D _model;
        private SolidColorBrush[] brush = {
             new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
             new SolidColorBrush(Color.FromArgb(255, 255, 127, 0)),
             new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
             new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
             new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
             new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
             new SolidColorBrush(Color.FromArgb(255, 10, 10, 10))};
        private int[] colors = {1,2,4,8,16,32,64};
        private MeshGeometry3D mesh;
        private int color;
        
        public visualmodel_mirror(int i, int j, int k, int c, int N)
        {
 
            form_cube_side cube = new form_cube_side(i, j, k, c, N);

            mesh = cube.give_me_mesh();
            
            if (c == (int)cube_side.up)
            {
                c = (int)cube_side.down;

            }
            else 

            if (c == (int)cube_side.right)
            {
                c = (int)cube_side.left;

            }

            else
            if (c == (int)cube_side.front)
            {
                c = (int)cube_side.back;

            }
            color = colors[c];
 
            Material colorMaterial = new DiffuseMaterial(new Kit3DBrush(brush[c]));
            GeometryModel3D mGeometry = new GeometryModel3D(mesh, colorMaterial);
            mGeometry.SeamSmoothing = 1.1;
            Model3DGroup group = new Model3DGroup();

           
            group.Children.Add(mGeometry);
            _model = new ModelVisual3D();
            _model.Content = group;

        }
        
        public visualmodel_mirror(int i, int j, int k, int c, int N, int x)
        
        {
         
            form_cube_side cube = new form_cube_side(i, j, k, x, N);

            mesh = cube.give_me_mesh();

            Material colorMaterial = new DiffuseMaterial(new Kit3DBrush(brush[c]));
            GeometryModel3D mGeometry = new GeometryModel3D(mesh, colorMaterial);
            mGeometry.SeamSmoothing = 1.1;
            Model3DGroup group = new Model3DGroup();

            group.Children.Add(mGeometry);
            _model = new ModelVisual3D();
            _model.Content = group;

        }
        public ModelVisual3D get_model()
        {
            return _model;
        }

        public int get_color()
        {
            return color;
        }


    }
    
}
