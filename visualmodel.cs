// Rubik Cube 3D presentation and solution
// is developed by Halyna Shashyna
// Halyna.Shashyna@gmail.com
// visualmodel.cs
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
    class visualmodel
    {
        private int[] colors = { 1, 2, 4, 8, 16, 32, 0 };
        private Transform3DGroup _trans_group;
        private ModelVisual3D _model;
        private GeometryModel3D _geom_model;
        private SolidColorBrush[] brush = {
             new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
             new SolidColorBrush(Color.FromArgb(255, 255, 127, 0)),
             new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
             new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
             new SolidColorBrush(Color.FromArgb(255, 0, 0, 255)),
             new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
             new SolidColorBrush(Color.FromArgb(255, 10, 10, 10))};
        private MeshGeometry3D mesh;
        private int color;
        

        public visualmodel(int i, int j, int k, int c, int N)
        {
            

            form_cube_side cube = new form_cube_side(i, j, k, c, N);

            mesh = cube.give_me_mesh();
            int col = 6;
            if ((j == N-1) && (c == (int)cube_side.up))
            {
                col = (int)cube_side.up;
                goto After;
            }

            if ((j == 0) && (c == (int)cube_side.down))
            {
                col = (int)cube_side.down;
                goto After;
            }
            if ((i == 0) && (c == (int)cube_side.left))
            {
                col = (int)cube_side.left;
                goto After;
            }

            if ((i == N-1) && (c == (int)cube_side.right))
            {
                col = (int)cube_side.right;
                goto After;
            }
            if ((k == 0) && (c == (int)cube_side.back))
            {
                col = (int)cube_side.back;
                goto After;
            }
            if ((k == N-1) && (c == (int)cube_side.front))
            {
                col = (int)cube_side.front;

            }
            After:
            Material colorMaterial;
            
            colorMaterial = new DiffuseMaterial(new Kit3DBrush(brush[col]));

            
            color = colors[col];
            
            GeometryModel3D mGeometry = new GeometryModel3D(mesh, colorMaterial);
            _geom_model = mGeometry;
            Model3DGroup group = new Model3DGroup();

                       
            mGeometry.SeamSmoothing = 1.1;
            group.Children.Add(mGeometry);
            _model = new ModelVisual3D();
            _model.Content = group;
            

            _trans_group = new Transform3DGroup();
            TranslateTransform3D trans_init = new TranslateTransform3D(0, 0, 0);
            _trans_group.Children.Add(trans_init);
            _model.Transform = _trans_group;

        }
        public ModelVisual3D get_model()
        {
            return _model;
        }
        public Transform3DGroup get_trans()
        {
            return _trans_group;
        }

        public int get_color()
        {
            return color;
        }
    }
}
