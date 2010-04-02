// Rubik Cube 3D presentation and solution
// is developed by Halyna Shashyna
// Halyna.Shashyna@gmail.com
// Page.xaml.cs
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Kit3D;
using Kit3D.Windows.Controls;
using Kit3D.Windows.Media;
using Kit3D.Windows.Media.Media3D;
using Kit3D.Util;
using Kit3D.Objects;


// http://www.alchemistmatt.com/cube/rubikstep1.html - there are all the instructions I have used to create animated Rubik Cube solutions and
// my great thanks to Matthew Monroe for creating them so accurate and vivid way.


namespace RubikCube
{
    public partial class Page : UserControl
    {
        
        private int N;
        
        private CubeLayout cubeLayout;
        
        private SolutionsAndRotations solutionAndRotations;

        
        private SolutionsAndRotations.SolutionFunctions solution;
        
        private class CubeLayout
        {
            private void Content_FullScreenChanged(object sender, EventArgs e)
            {
                if (App.Current.Host.Content.IsFullScreen == true)
                {

                    t.grid.Background = new SolidColorBrush(Colors.Black);
                }
                else
                {

                    t.grid.Background = new SolidColorBrush(Colors.White);
                }
                updateLayout(App.Current.Host.Content.ActualWidth, App.Current.Host.Content.ActualHeight,t);
            }

            private void root_Loaded(object sender, RoutedEventArgs e)
            {
                updateLayout(App.Current.Host.Content.ActualWidth, App.Current.Host.Content.ActualHeight,t);
            }
            private void Content_Resized(object sender, EventArgs e)
            {
                updateLayout(App.Current.Host.Content.ActualWidth, App.Current.Host.Content.ActualHeight, t);
            }
            private void updateLayout(double width, double height, Page page)
            {

                if (page.mainScale != null && page.root.Width != 0 && page.root.Height != 0)
                {
                    var scale = width / page.root.Width;
                    if (height / page.root.Height < scale)
                    {
                        scale = height / page.root.Height;
                    }
                    page.mainScale.ScaleX = scale;
                    page.mainScale.ScaleY = scale;
                }
            }
            public CubeLayout(Page p)
            {
                t = p;
                App.Current.Host.Content.Resized += new EventHandler(Content_Resized);
                App.Current.Host.Content.FullScreenChanged += new EventHandler(Content_FullScreenChanged);

            }
            private Page t;

        }
        
        private void dimen_N_Init()
        {
            dimen_N.Items.Add("1x1x1");
            dimen_N.Items.Add("2x2x2");
            dimen_N.Items.Add("3x3x3");
            dimen_N.Items.Add("4x4x4");
            dimen_N.Items.Add("5x5x5");
            dimen_N.Items.Add("6x6x6");
            dimen_N.Items.Add("7x7x7");
            
            dimen_N.SelectedIndex = N - 1;
            
            dimen_N.SelectionChanged += new SelectionChangedEventHandler(dimen_N_SelectionChanged);
            
        }
        private void Slider_Init()
        {
            hSlider.Maximum = 6;
            hSlider.Minimum = 4;

            hSlider.Value = 5;

            hSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(hSlider_ValueChanged);
        }
        
        
        public Page()
        {
            InitializeComponent();
            N = 3;
            dimen_N_Init();
            cubeLayout = new CubeLayout(this);
            
            AllClassesInitialization();
            Slider_Init();
        }
        private void AllClassesInitialization()
        {
            solutionAndRotations = new SolutionsAndRotations(this, N);
            solution = solutionAndRotations.Solution;
 
            
        }
        
        private void dimen_N_SelectionChanged(object sender, EventArgs e)
        {
            solutionAndRotations.cleanUp(N);

            object x = dimen_N.SelectedIndex + 1;
            N = (int)x;
            
            AllClassesInitialization();

        }
        private void hSlider_ValueChanged(object sender, EventArgs e)
        {
            solutionAndRotations.ScaleChanged();

        }

        private void BottomCorners_Click(object sender, RoutedEventArgs e)
        {
            solution.BottomCorners();
            
        }
        private void middleSlice_Click(object sender, RoutedEventArgs e)
        {
            solution.MiddleSlice();
            
        }
        private void button_reset_Click(object sender, RoutedEventArgs e)
        {
            solutionAndRotations.cleanUp(N);

            AllClassesInitialization();
            
        }

        private void TopWings_Click(object sender, RoutedEventArgs e)
        {
            solution.TopWings();
            
        }

        private void button_animated_reset_Click(object sender, RoutedEventArgs e)
        {
            solution.ButtonAnimated();
            
        }
        
        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            solution.ButtonBack();
            
        }
         
        private void TopCorners_Click(object sender, RoutedEventArgs e)
        {
            solution.TopCorners();
         }

         private void BottomWings_Click(object sender, RoutedEventArgs e)
        {
            solution.BottomWings();
        }


        private void RandomPosition_Click(object sender, RoutedEventArgs e)
        {
            solution.RandomPositionRealization();
        }


        private void Solve_Click(object sender, RoutedEventArgs e)
        {
                     
            solution.AllTheProcess();
            
        }
        
        
    }
    
}
