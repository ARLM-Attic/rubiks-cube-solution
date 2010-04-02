// Rubik Cube 3D presentation and solution
// is developed by Halyna Shashyna
// Halyna.Shashyna@gmail.com
// MouseWork.cs
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
using Kit3D.Windows.Media.Media3D;
using Kit3D.Windows.Media;

namespace RubikCube
{
    public class MouseWork
    {
        private bool bool_MouseLeftButtonDownHandlerAdded;
        private bool bool_MouseMoveHandlerAdded;
        private mouse_dir Mouse_DIR;

        private int index_x;
        private int index_y;
        private int index_z;

        private int global_x_2;
        private int global_y_2;

        private int global_x_1;
        private int global_y_1;
        private int Mouse_X;
        private int Mouse_Y;

        private int N;

        private Page page;
        private ButtonWork buttonWork;
        private SolutionsAndRotations.GeneralRotations generalRotations;
        private SolutionsAndRotations solutionsAndRotations;

        private bool UpRotationByMouse(bool clock_wise_)
        {

            // up rotation
            if (Mouse_Y > 0)
            {
                
                 generalRotations.base_for_rotation(general_dir.up_dir, index_y, clock_wise_, true);

                 buttonWork.ActivateRandomButton();
                return true;
            }

            if (Mouse_Y < 0)
            {
                generalRotations.base_for_rotation(general_dir.up_dir, index_y, !clock_wise_, true);

                buttonWork.ActivateRandomButton();
                return true;
            }

            return false;

        }
        private void grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (index_x == -1 || index_y == -1 || index_z == -1)
                return;

            UIElement __relativeTo = sender as UIElement;
            global_x_2 = 0;
            global_y_2 = 0;

            if (__relativeTo != null)
            {
                global_x_2 = (int)e.GetPosition(__relativeTo).X;
                global_y_2 = (int)e.GetPosition(__relativeTo).Y;
            }
            else
            {
                global_x_1 = 0;
                global_y_1 = 0;
                return;
            }


            int dif_x = global_x_2 - global_x_1;
            int dif_y = -(global_y_2 - global_y_1);

            Mouse_X += dif_x;
            Mouse_Y += dif_y;
            if (Math.Abs(Mouse_X) + Math.Abs(Mouse_Y) < 25)
                return;
            Boolean clock_wise_ = false;
            switch (Mouse_DIR)
            {
                case mouse_dir.right_dir:
                    if (Math.Abs(2 * Mouse_X) <= Math.Abs(Mouse_Y))
                    {
                        // Front rotation
                        MouseHandlerPlusMinus(false);
                        bool bbb = FrontRotationByMouse(clock_wise_);

                        return;

                    }
                    else
                    {

                        if (Math.Sign(Mouse_X) * Math.Sign(Mouse_Y) < 0)
                        {
                            MouseHandlerPlusMinus(false);
                            return;
                        }
                        if ((Math.Abs(Mouse_Y) <= Math.Abs(Mouse_X) && (Math.Abs(Mouse_Y) > Math.Abs(Mouse_X / 8))))
                        {
                            // up rotation
                            MouseHandlerPlusMinus(false);
                            bool bbb = UpRotationByMouse(clock_wise_);

                            return;

                        }
                        else
                        {
                            MouseHandlerPlusMinus(false);
                            return;
                        }
                    }

                    break;
                case mouse_dir.front_dir:
                    if (Math.Abs(4 * Mouse_X) <= Math.Abs(Mouse_Y))
                    {
                        // Front rotation
                        MouseHandlerPlusMinus(false);
                        bool bbb = RightRotationByMouse(!clock_wise_);

                        return;

                    }
                    else
                    {

                        if (Math.Sign(Mouse_X) * Math.Sign(Mouse_Y) > 0)
                        {
                            MouseHandlerPlusMinus(false);
                            return;
                        }
                        if ((Math.Abs(Mouse_Y) <= Math.Abs(Mouse_X) && (Math.Abs(Mouse_Y) > Math.Abs(Mouse_X / 8))))
                        {
                            // up rotation
                            MouseHandlerPlusMinus(false);
                            bool bbb = UpRotationByMouse(!clock_wise_);

                            return;

                        }
                        else
                        {
                            MouseHandlerPlusMinus(false);
                            return;
                        }
                    }
                    break;
                case mouse_dir.up_dir:
                    if ((Math.Abs(Mouse_Y) <= Math.Abs(Mouse_X) && (Math.Abs(Mouse_Y) > Math.Abs(Mouse_X / 8))))
                    {
                        if (Math.Sign(Mouse_X) * Math.Sign(Mouse_Y) < 0)
                        {
                            // up rotation
                            MouseHandlerPlusMinus(false);
                            bool bbb = FrontRotationByMouse(clock_wise_);

                            return;
                        }
                        else
                        {
                            MouseHandlerPlusMinus(false);
                            bool bbb = RightRotationByMouse(!clock_wise_);

                            return;
                        }


                    }
                    else
                    {
                        MouseHandlerPlusMinus(false);
                        return;
                    }

                    break;
            }
        }
        private bool FrontRotationByMouse(bool clock_wise_){
            if (Mouse_Y > 0)
            {
                generalRotations.base_for_rotation(general_dir.front_dir, index_z, clock_wise_, true);

                buttonWork.ActivateRandomButton();
                return true;
            }

            if (Mouse_Y < 0)
            {

                generalRotations.base_for_rotation(general_dir.front_dir, index_z, !clock_wise_, true);

                buttonWork.ActivateRandomButton();
                return true;
            }

            return false;

        }
        private bool RightRotationByMouse(bool clock_wise_)
        {
            if (Mouse_Y > 0)
            {
                generalRotations.base_for_rotation(general_dir.right_dir, index_x, clock_wise_, true);

                buttonWork.ActivateRandomButton();
                return true;
            }

            if (Mouse_Y < 0)
            {
                generalRotations.base_for_rotation(general_dir.right_dir, index_x, !clock_wise_, true);

                buttonWork.ActivateRandomButton();
                return true;
            }

            return false;

        }
        public void functionMouseLeftButtonDownHandlerAdded()
        {
            if (bool_MouseLeftButtonDownHandlerAdded == false)
            {
                page.grid.MouseLeftButtonDown += new MouseButtonEventHandler(grid_MouseLeftButtonDown);

                bool_MouseLeftButtonDownHandlerAdded = true;

            }
        }
        public void functionMouseLeftButtonDownHandlerExtracted()
        {
            if (bool_MouseLeftButtonDownHandlerAdded == true)
            {
                page.grid.MouseLeftButtonDown -= new MouseButtonEventHandler(grid_MouseLeftButtonDown);

                bool_MouseLeftButtonDownHandlerAdded = false;

            }
        }
        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //
            index_x = -1;
            index_y = -1;
            index_z = -1;
            UIElement __relativeTo = sender as UIElement;
            global_x_1 = 0;
            global_y_1 = 0;

            if (__relativeTo != null)
            {
                global_x_1 = (int)e.GetPosition(sender as UIElement).X;
                global_y_1 = (int)e.GetPosition(sender as UIElement).Y;
            }
            else return;
            Mouse_X = 0;
            Mouse_Y = 0;
            
            Kit3D.Windows.Media.VisualTreeHelper.HitTest(solutionsAndRotations.ViewPort, null, HitTest, new PointHitTestParameters(e.GetPosition(solutionsAndRotations.ViewPort)));
        }
        private void MouseHandlerPlusMinus(bool PlusOrMinus)
        {
            if (PlusOrMinus == true)
            {
                if (bool_MouseMoveHandlerAdded == false)
                {
                    page.grid.MouseMove += new MouseEventHandler(grid_MouseMove);

                    bool_MouseMoveHandlerAdded = true;
                }
                else
                {
                    bool x = bool_MouseMoveHandlerAdded;
                }

            }
            else
            {
                if (bool_MouseMoveHandlerAdded == true)
                {
                    page.grid.MouseMove -= new MouseEventHandler(grid_MouseMove);
                    bool_MouseMoveHandlerAdded = false;
                }
                else
                {
                    bool x = bool_MouseMoveHandlerAdded;
                }

            }

        }
        private HitTestResultBehavior HitTest(HitTestResult result)
        {
            ModelVisual3D m = result.VisualHit;

            index_x = -1;
            index_y = -1;
            index_z = -1;
            if (m == null)
            {

                global_x_1 = 0;
                global_y_1 = 0;
                goto went_out;
            }
            Mouse_DIR = mouse_dir.nothing;
            for (int i = 0; i < N; i++)
            {
                for (int k = 0; k < N; k++)
                {
                    if (m == solutionsAndRotations.model[N - 1, i, k, (int)cube_side.right])
                    {
                        
                        Mouse_DIR = mouse_dir.right_dir;
                        index_x = N - 1;
                        index_y = i;
                        index_z = k;

                        MouseHandlerPlusMinus(true);
                        goto went_out;
                    }
                    if (m == solutionsAndRotations.model[i, N - 1, k, (int)cube_side.up])
                    {
                        Mouse_DIR = mouse_dir.up_dir;
                        index_x = i;
                        index_y = N - 1;
                        index_z = k;
                        MouseHandlerPlusMinus(true);
                        goto went_out;
                    }
                    if (m == solutionsAndRotations.model[i, k, N - 1, (int)cube_side.front])
                    {
                        Mouse_DIR = mouse_dir.front_dir;
                        index_x = i;
                        index_y = k;
                        index_z = N - 1;
                        MouseHandlerPlusMinus(true);
                        goto went_out;
                    }
                }

            }
        went_out:

            return HitTestResultBehavior.Stop;

        }
        public void CanAddMouseLeftButtonDownHandler(){
            if (bool_MouseLeftButtonDownHandlerAdded == false)
                functionMouseLeftButtonDownHandlerAdded();
        }
               
        public MouseWork(Page _page, ButtonWork _buttonWork, SolutionsAndRotations.GeneralRotations _gen, SolutionsAndRotations _sol, int _N)
        {

            bool_MouseLeftButtonDownHandlerAdded = false;
            bool_MouseMoveHandlerAdded = false;

            page = _page;
            buttonWork = _buttonWork;
            N = _N;
            generalRotations = _gen;
            solutionsAndRotations = _sol;
        }
    }
}
