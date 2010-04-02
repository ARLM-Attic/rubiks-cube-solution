// Rubik Cube 3D presentation and solution
// is developed by Halyna Shashyna
// Halyna.Shashyna@gmail.com
// SolutionsAndRotations.cs
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
using Kit3D.Windows.Controls;
using System.Collections.Generic;

namespace RubikCube
{
    public enum general_dir { front_dir, right_dir, up_dir }
    public enum mouse_dir { front_dir, right_dir, up_dir, nothing }
    public enum cube_side { front, back, right, left, up, down }
    public enum color { red, magenta, green, yellow, blue, white }
    public enum color_bin { front_color = 1, back_color = 2, right_color = 4, left_color = 8, up_color = 16, down_color = 32 }
    public enum ToDo { DoAll, DoModelRotation, DoVisualRotation }
    public enum solutionStep { NOTHINGsolutionStep, ALL, I_step, II_step, III_step, IV_step, V_step }

    public class SolutionsAndRotations
    {
        private int N;
        private Page page;
        private N_init initArray;

        private int[, , ,] colors_group;
        private int[, , ,] colors_group_copy;

        private int[,] colors_group_back;
        private int[,] colors_group_left;
        private int[,] colors_group_down;

        private ModelVisual3D[,] model_back;
        private ModelVisual3D[,] model_left;
        private ModelVisual3D[,] model_down;

        private Transform3DGroup[, , ,] trans_group;

        private Viewport3D viewport;

        private LinkedList<rotations> History = new LinkedList<rotations>();
        public LinkedList<rotations> history
        {
            get
            {
                return History;
            }
            set
            {
                History = value;
            }
        }
        private Queue<rotations> QueueRotations = new Queue<rotations>(32);
        public Queue<rotations> queue
        {
            get
            {
                return QueueRotations;
            }
            set
            {
                QueueRotations = value;
            }
        }
        private LinkedList<rotations> RandomSelection = new LinkedList<rotations>();
        public LinkedList<rotations> randomSelection
        {
            get
            {
                return RandomSelection;
            }
            set
            {
                RandomSelection = value;
            }
        }
        private ModelVisual3D[, , ,] SideCubeModel;
        
        private SolutionsAndRotations.GeneralRotations generalRotation;
        private ButtonWork buttonWork;
        private MouseWork mouseWork;
        private SolutionFunctions solutions;
        public ModelVisual3D[, , ,] model
        {
            get
            {
                return SideCubeModel;
            }
            set
            {
                SideCubeModel = value;
            }
        }
        public Viewport3D ViewPort
        {
            get
            {
                return viewport;
            }
        }
        public struct rotations
        {
            public int _dir;
            public int _index;
            public Boolean _clock_wise;
            public rotations(int dir, int index, Boolean clock_wise)
            {
                _dir = dir;
                _index = index;
                _clock_wise = clock_wise;
            }
        }
        public void ScaleChanged()
        {
            double x = page.hSlider.Value;
            Point3D pointOfView = new Point3D(x * 8 + 2, x * 8 + 2, x * 8 + 2);
            PerspectiveCamera camera = new PerspectiveCamera(pointOfView,
                                                    new Vector3D(-1, -1, -1),
                                                    new Vector3D(0, 1, 0),
                                                    45);
            viewport.Camera = camera;
        }
        public void cleanUp(int M)
        {
            
            History.Clear();
            QueueRotations.Clear();
            RandomSelection.Clear();
            if (solutions.SolutionTimer != null)
                solutions.SolutionTimer.Stop();
            page.dimen_N.IsEnabled = true;

            for (int i = 0; i < M; i++)
                for (int j = 0; j < M; j++)
                {
                    viewport.Children.Remove(model_back[i, j]);
                    viewport.Children.Remove(model_left[i, j]);
                    viewport.Children.Remove(model_down[i, j]);
                    colors_group_back[i, j] = 0;
                    colors_group_down[i, j] = 0;
                    colors_group_left[i, j] = 0;
                    for (int k = 0; k < M; k++)
                    {
                        foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                        {
                            viewport.Children.Remove(SideCubeModel[i, j, k, (int)c]);

                            SideCubeModel[i, j, k, (int)c] = null;
                            
                            colors_group[i, j, k, (int)c] = 0;
                            colors_group_copy[i, j, k, (int)c] = 0;
                            trans_group[i, j, k, (int)c] = null;
                        }
                    }
                }


        }
        public SolutionFunctions Solution
        {
            get
            {
                return solutions;
            }
        }
        private bool testOnSurface(int i, int j, int k)
        {
            
            int x, y, z;
            x = 0;
            y = 0;
            z = 0;
            if (N == 1) return true;
            if (i % (N - 1) == 0)
                x = 1;
            if (j % (N - 1) == 0)
                y = 1;
            if (k % (N - 1) == 0)
                z = 1;

            if (x + y + z == 0)
                return false; //not on surface
            else
                return true;// this cube  on surface 
            
        }
        
        private void formMirrors()
        {
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    visualmodel_mirror vm = new visualmodel_mirror(-N - 0, i, j, (int)cube_side.right, N);
                    model_left[i, j] = vm.get_model();
                    colors_group_left[i, j] = 8;
                    viewport.Children.Add(model_left[i, j]);

                }

            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {

                    visualmodel_mirror vm = new visualmodel_mirror(i, j, -N - 0, (int)cube_side.front, N);
                    model_back[i, j] = vm.get_model();
                    colors_group_back[i, j] = 2;
                    viewport.Children.Add(model_back[i, j]);

                }
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    visualmodel_mirror vm = new visualmodel_mirror(i, -N - 0, j, (int)cube_side.up, N);
                    model_down[i, j] = vm.get_model();
                    colors_group_down[i, j] = 32;
                    viewport.Children.Add(model_down[i, j]);
                }
        }
        private void CameraTuning()
        {
            viewport.HorizontalAlignment = HorizontalAlignment.Stretch;
            viewport.VerticalAlignment = VerticalAlignment.Stretch;
            Point3D pointOfView = new Point3D(30, 30, 30);
            PerspectiveCamera camera = new PerspectiveCamera(pointOfView,
                                                    new Vector3D(-1, -1, -1),
                                                    new Vector3D(0, 1, 0),
                                                    45);
            viewport.Camera = camera;

            page.grid.Children.Add(viewport);
        }
        private void FormViewport()
        {
            viewport = new Viewport3D();
            CameraTuning();
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    for (int k = 0; k < N; k++)
                    {
                        bool OnSurface;
                        OnSurface = testOnSurface(i, j, k);
                        if (!OnSurface) continue;
                        foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                        {
                            visualmodel vm = new visualmodel(i, j, k, (int)c, (int)N);
                            SideCubeModel[i, j, k, (int)c] = vm.get_model();
                            
                            colors_group[i, j, k, (int)c] = vm.get_color();
                            trans_group[i, j, k, (int)c] = vm.get_trans();
                            viewport.Children.Add(SideCubeModel[i, j, k, (int)c]);

                        }
                    }

            formMirrors();

        }
        private void formArrays(int _N)
        {
            N = _N;
            initArray = new N_init(N);

            model_back = initArray.ModelBack;
            model_left = initArray.ModelLeft;
            model_down = initArray.ModelDown;

            SideCubeModel = initArray.Model;

            colors_group_back = initArray.colorsGroupBack;
            colors_group_left = initArray.colorsGroupLeft;
            colors_group_down = initArray.colorsGroupDown;

            colors_group = initArray.colorsGroup;
            colors_group_copy = initArray.colorsGroupCopy;

            trans_group = initArray.TransGroup;
        }
        public SolutionsAndRotations(Page _page, int _N)
        {
            page = _page;
            N = _N;
            formArrays(N);
            FormViewport();

            //-------------------------------------
            
            generalRotation = new SolutionsAndRotations.GeneralRotations(this.viewport, N, this);

            buttonWork = new ButtonWork(page, N);
            mouseWork = new MouseWork(page, buttonWork, generalRotation, this, N);
            solutions = new SolutionsAndRotations.SolutionFunctions(page, N, this, mouseWork, buttonWork, generalRotation);

            buttonWork.trans(solutions);
            solutions.CurrentSolutionStep = solutionStep.NOTHINGsolutionStep;

            mouseWork.functionMouseLeftButtonDownHandlerAdded();

            buttonWork.ButtonsResetsAndBack(false);
            buttonWork.ButtonsInit();


            //------------------

        }
        private class N_init
        {

            private ModelVisual3D[,] model_back;
            private ModelVisual3D[,] model_left;
            private ModelVisual3D[,] model_down;
            private ModelVisual3D[, , ,] SideCubeModel;
            private ModelVisual3D[, , ,] model_copy;

            private int[,] colors_group_back;
            private int[,] colors_group_left;
            private int[,] colors_group_down;

            private int[, , ,] colors_group;
            private int[, , ,] colors_group_copy;

            private Transform3DGroup[, , ,] trans_group;
            private Transform3DGroup[, , ,] trans_group_copy;

            public N_init(int N)
            {
                model_back = new ModelVisual3D[N, N];
                model_left = new ModelVisual3D[N, N];
                model_down = new ModelVisual3D[N, N];
                SideCubeModel = new ModelVisual3D[N, N, N, 6];
                model_copy = new ModelVisual3D[N, N, N, 6];

                colors_group_back = new int[N, N];
                colors_group_left = new int[N, N];
                colors_group_down = new int[N, N];

                colors_group = new int[N, N, N, 6];
                colors_group_copy = new int[N, N, N, 6];

 
                trans_group = new Transform3DGroup[N, N, N, 6];
                trans_group_copy = new Transform3DGroup[N, N, N, 6];
            }
            public int[,] colorsGroupBack
            {
                get
                {
                    return colors_group_back;
                }
            }
            public int[,] colorsGroupLeft
            {
                get
                {
                    return colors_group_left;
                }
            }
            public int[,] colorsGroupDown
            {
                get
                {
                    return colors_group_down;
                }
            }
            public ModelVisual3D[,] ModelBack
            {
                get
                {
                    return model_back;
                }
            }//
            public ModelVisual3D[,] ModelLeft
            {
                get
                {
                    return model_left;
                }
            }//
            public ModelVisual3D[,] ModelDown
            {
                get
                {
                    return model_down;
                }
            }//
            public ModelVisual3D[, , ,] Model
            {
                get
                {
                    return SideCubeModel;
                }
            }//
            public ModelVisual3D[, , ,] ModelCopy
            {
                get
                {
                    return model_copy;
                }
            }//

            public int[, , ,] colorsGroup
            {
                get
                {
                    return colors_group;
                }
            }//
            public int[, , ,] colorsGroupCopy
            {
                get
                {
                    return colors_group_copy;
                }
            }//
            
            public Transform3DGroup[, , ,] TransGroup
            {
                get
                {
                    return trans_group;
                }
            }
            public Transform3DGroup[, , ,] TransGroupCopy
            {
                get
                {
                    return trans_group_copy;
                }
            }

            
            ~N_init()
            {

            }
        }
        public class SolutionFunctions
        {
            private Storyboard solutionTimer;
            public Storyboard SolutionTimer
            {
                get
                {
                    return solutionTimer;
                }
                set
                {
                    solutionTimer = value;
                }
            }
            private int attempt;

            private Queue<rotations> QueueRotations = new Queue<rotations>(32);
            private List<rotations> RandomSelection = new List<rotations>();
            private SolutionsAndRotations solutionsAndRotations;
            
            private int colorCenterTop;
            private int colorCenterFront;
            private int colorCenterLeft;
            private int colorCenterRight;
            private int colorCenterBack;
            private int colorCenterDown;

            private int colorCorner_11;
            private int colorCorner_31;
            private int colorCorner_33;
            private int colorCorner_13;

            private int colorWing_12;
            private int colorWing_21;
            private int colorWing_32;
            private int colorWing_23;

            private int count_RightOriented_bottom_corners;
            private int MilliSecForAnimatedReset = 800;
            private MouseWork mouseWork;
            private ButtonWork buttonWork;
            
            private int N;
            private Page page;
            private GeneralRotations generalRotations;
            private Storyboard animatedReset;
            private ComboBox dimen_N;
            
            private solutionStep currentSolutionStep;
            public solutionStep CurrentSolutionStep
            {
                get
                {
                    return currentSolutionStep;
                }
                set
                {
                    currentSolutionStep = value;
                }
            }
            public SolutionFunctions(Page _page, int _N, SolutionsAndRotations _sol,  MouseWork _mw, ButtonWork _bw, SolutionsAndRotations.GeneralRotations _gen)
            {
                
                page = _page;
                N = _N;
                
                currentSolutionStep = solutionStep.NOTHINGsolutionStep;
                dimen_N = page.dimen_N;
                solutionsAndRotations = _sol;
                mouseWork = _mw;
                buttonWork = _bw;
                generalRotations = _gen;
            }
            
            private void animatedReset_Completed(object sender, EventArgs e)
            {
                if (solutionsAndRotations.History.Count != 0)
                {
                    SolutionsAndRotations.rotations rotationForAnimatedReset = solutionsAndRotations.History.Last.Value;
                    solutionsAndRotations.History.RemoveLast();
                    generalRotations.base_for_rotation((general_dir)rotationForAnimatedReset._dir, rotationForAnimatedReset._index, rotationForAnimatedReset._clock_wise, false);
                    animatedReset.Begin();
                }
                else
                {
                    animatedReset.Stop();
                    animatedReset.Completed -= new EventHandler(animatedReset_Completed);
                    
                    buttonWork.ButtonsResetsAndBack(false); 
                                        
                    buttonWork.AllSolutionButtons(false);

                    buttonWork.SetButtons("RandomPosition", true);
                    page.RandomPosition.Content = "Random Position";

                    dimen_N.IsEnabled = true;
                    
                    buttonWork.FirstRandomization = 0;

                    mouseWork.CanAddMouseLeftButtonDownHandler();
                    
                }
            }

            public void ButtonAnimated()
            {
                buttonWork.ButtonsResetsAndBack(false);

                buttonWork.AllSolutionButtons(false);

                buttonWork.SetButtons("RandomPosition", false);

                mouseWork.functionMouseLeftButtonDownHandlerExtracted();

                animatedReset = new Storyboard();
                animatedReset.Duration = TimeSpan.FromMilliseconds(MilliSecForAnimatedReset);
                animatedReset.Completed += new EventHandler(animatedReset_Completed);
                animatedReset.Begin();
 
            }
            public void ButtonReset()
            {
                solutionsAndRotations.cleanUp(N);
                solutionsAndRotations.FormViewport();
                solutionsAndRotations.RandomSelection.Clear();

                buttonWork.ButtonsResetsAndBack(false);
            
                buttonWork.FirstRandomization = 0;
                page.RandomPosition.Content = "Random Position";
                buttonWork.SetButtons("RandomPosition", true);

                buttonWork.AllSolutionButtons(false);
                
                dimen_N.IsEnabled = true;

                currentSolutionStep = solutionStep.NOTHINGsolutionStep;

                mouseWork.CanAddMouseLeftButtonDownHandler();
            }
            public void ButtonBack()
            {
                int count = solutionsAndRotations.History.Count;
                if (count != 0)
                {
                    SolutionsAndRotations.rotations x = solutionsAndRotations.History.Last.Value;
                   
                    generalRotations.base_for_rotation((general_dir)x._dir, x._index, x._clock_wise, false);
                    
                    solutionsAndRotations.History.RemoveLast();
                }
                if (count == 1)
                {
                    page.RandomPosition.Content = "Random Position";
                    buttonWork.SetButtons("RandomPosition", true);

                    buttonWork.AllSolutionButtons(false);
                    buttonWork.ButtonsResetsAndBack(false);

                    page.dimen_N.IsEnabled = true;

                    buttonWork.FirstRandomization = 0;
                }
            }
            public void TopCorners()
            {
                copyMirrors();
                int count = 0;
                while (count < 4)
                {
                    count = putTopCornersToRightPositions();
                    
                }
                //MessageBox.Show("top corners are done");
                currentSolutionStep = solutionStep.I_step;

                PlayCurrentSolution();
            }
            public void RandomPositionRealization()
            {
                buttonWork.IncRandomization();
                general_dir DIR  = general_dir.right_dir;
                
                bool bool_clock_wise;
                int TheSeed = (int)DateTime.Now.Ticks;
                
                Random dir = new Random(TheSeed/3);
                Random sliceNumber = new Random(TheSeed / 2);
                Random clock_wise = new Random(TheSeed );

                copyMirrors();

                for (int i = 0; i < 3; i++){
                    
                    int RandomDir = dir.Next(0,3);
                    int RandomSliceNumber = sliceNumber.Next(0,N);
                    int RandomClock_wise = clock_wise.Next(0,2);
                    if (RandomClock_wise == 0)
                        bool_clock_wise = true;
                    else
                        bool_clock_wise = false;

                    switch (RandomDir)
                    {
                        case 0:
                            DIR = general_dir.right_dir;
                            break;
                        case 1:
                            DIR = general_dir.up_dir;
                            break;
                        case 2:
                            DIR = general_dir.front_dir;
                            break;
                    }
                    toQueue(DIR, RandomSliceNumber, bool_clock_wise, true);

                    RandomSelection.Add(new rotations(RandomDir, RandomSliceNumber, bool_clock_wise));
                }
                PlayCurrentSolution();
            }
            public void BottomWings()
            {
                copyMirrors();//-----------------------------------
                int i = 1;
                while (i == 1)
                {
                    putBottomWingsToRightPlaces();
                    //MessageBox.Show("Bottom wings on the places");
                    i = putKeyHole();
                }
                //MessageBox.Show("Key hole is restored");
                orientationAfterBottomWings();
                //MessageBox.Show("orientation is ready");

                RightSlice_orientation();
                LeftSlice_orientation();

                currentSolutionStep = solutionStep.IV_step;

                PlayCurrentSolution();
            }
            private void PlayQueue()
            {
                rotations rotaionElement = QueueRotations.Dequeue();

                generalRotations.base_for_rotation_show(
                    (general_dir)rotaionElement._dir,
                                 rotaionElement._index,
                                 rotaionElement._clock_wise, true);
            }
            private void solutionTimer_Completed(object sender, EventArgs e)
            {

                if (QueueRotations.Count != 0)
                {
                    PlayQueue();
                    solutionTimer.Begin();
                }
                else
                {
                    if ((currentSolutionStep == solutionStep.ALL) || (currentSolutionStep == solutionStep.V_step))
                        solutionsAndRotations.History.Clear();
                    buttonWork.ShowButtonsAfterRotating();

                    solutionTimer.Stop();
                    bool x4 = (currentSolutionStep == solutionStep.I_step);
                    bool x1 = (currentSolutionStep == solutionStep.II_step);
                    bool x2 = (currentSolutionStep == solutionStep.III_step);
                    bool x3 = (currentSolutionStep == solutionStep.IV_step);

                    bool x5 = x1 || x2 || x3 || x4;
                    if (!x5)
                        mouseWork.functionMouseLeftButtonDownHandlerAdded();
                }



            }
            private void PlayCurrentSolution()
            {

                buttonWork.HideAllButtonsWhileRotating();
                bool x1 = (currentSolutionStep == solutionStep.II_step);
                bool x2 = (currentSolutionStep == solutionStep.III_step);
                bool x3 = (currentSolutionStep == solutionStep.IV_step);
                bool x4 = (currentSolutionStep == solutionStep.V_step);
                bool x5 = x1 || x2 || x3 || x4;

                if (!x5)
                    mouseWork.functionMouseLeftButtonDownHandlerExtracted();//------------------------------------------------------
                solutionTimer = new Storyboard();
                solutionTimer.Duration = TimeSpan.FromMilliseconds(MilliSecForAnimatedReset);
                solutionTimer.Completed += new EventHandler(solutionTimer_Completed);
                solutionTimer.Begin();
            }
            //--------------------------------------------------------------------------
            private void define_centers()
            {
                colorCenterTop = solutionsAndRotations.colors_group[1, 2, 1, (int)cube_side.up];
                colorCenterFront = solutionsAndRotations.colors_group[1, 1, 2, (int)cube_side.front];
                colorCenterLeft = solutionsAndRotations.colors_group[0, 1, 1, (int)cube_side.left];
                colorCenterRight = solutionsAndRotations.colors_group[2, 1, 1, (int)cube_side.right];
                colorCenterBack = solutionsAndRotations.colors_group[1, 1, 0, (int)cube_side.back];
                colorCenterDown = solutionsAndRotations.colors_group[1, 0, 1, (int)cube_side.down];
            }
            //------------------------------------------------------------------------------------------
            private void top_down_orientation()
            {


                define_centers();
                int C = (int)color_bin.up_color;

                if (colorCenterTop == C)
                {
                    ////MessageBox.Show("nothing to do");
                }
                if (colorCenterBack == C)
                {
                    toQueue(general_dir.right_dir, 2, false, true);
                    toQueue(general_dir.right_dir, 1, false, true);
                    toQueue(general_dir.right_dir, 0, false, true);

                }
                if (colorCenterRight == C)
                {
                    toQueue(general_dir.front_dir, 2, false, true);
                    toQueue(general_dir.front_dir, 1, false, true);
                    toQueue(general_dir.front_dir, 0, false, true);

                }
                if (colorCenterFront == C)
                {
                    toQueue(general_dir.right_dir, 2, true, true);
                    toQueue(general_dir.right_dir, 1, true, true);
                    toQueue(general_dir.right_dir, 0, true, true);

                }
                if (colorCenterLeft == C)
                {
                    toQueue(general_dir.front_dir, 2, true, true);
                    toQueue(general_dir.front_dir, 1, true, true);
                    toQueue(general_dir.front_dir, 0, true, true);

                }
                if (colorCenterDown == C)
                {
                    toQueue(general_dir.front_dir, 2, true, true);
                    toQueue(general_dir.front_dir, 1, true, true);
                    toQueue(general_dir.front_dir, 0, true, true);
                    toQueue(general_dir.front_dir, 2, true, true);
                    toQueue(general_dir.front_dir, 1, true, true);
                    toQueue(general_dir.front_dir, 0, true, true);

                    //MessageBox.Show("from down");
                }
            }
            private void back_side_orientation()
            {

                define_centers();

                int C = (int)color_bin.front_color;

                if (colorCenterFront == C)
                {
                    ////MessageBox.Show("nothing to do");
                }
                if (colorCenterBack == C)
                {
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);

                }
                if (colorCenterRight == C)
                {
                    toQueue(general_dir.up_dir, 2, true, true);
                    toQueue(general_dir.up_dir, 1, true, true);
                    toQueue(general_dir.up_dir, 0, true, true);

                }

                if (colorCenterLeft == C)
                {
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);

                }

            }
            private void UpSlice_orientation()
            {
                for (int loop = 0; loop < 4; loop++)
                {
                    if (solutionsAndRotations.colors_group[1, 1, 2, (int)cube_side.front] == solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.front])
                        return;
                    else
                        toQueue(general_dir.up_dir, 2, false, true);
                }
            }
            private void RightSlice_orientation()
            {
                for (int loop = 0; loop < 4; loop++)
                {
                    if (solutionsAndRotations.colors_group[1, 1, 2, (int)cube_side.front] == solutionsAndRotations.colors_group[2, 1, 2, (int)cube_side.front])
                        return;
                    else
                        toQueue(general_dir.right_dir, 2, false, true);
                }
            }
            private void LeftSlice_orientation()
            {
                for (int loop = 0; loop < 4; loop++)
                {
                    if (solutionsAndRotations.colors_group[0, 1, 2, (int)cube_side.front] == solutionsAndRotations.colors_group[1, 1, 2, (int)cube_side.front])
                        return;
                    else
                        toQueue(general_dir.right_dir, 0, false, true);
                }
            }
            private void copyMirrors()
            {
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                        for (int k = 0; k < N; k++)
                        {
                            bool OnSurface;
                            OnSurface = solutionsAndRotations.testOnSurface(i, j, k);
                            if (!OnSurface) continue;
                            foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                            {
                                solutionsAndRotations.colors_group_copy[i, j, k, (int)c] = solutionsAndRotations.colors_group[i, j, k, (int)c];
                            }
                        }
            }
            
            public void AllTheProcess()
            {

                mouseWork.functionMouseLeftButtonDownHandlerExtracted();
                copyMirrors();
                int count = 0;
                while (count < 4)
                {
                    count = putTopCornersToRightPositions();

                }
                //--------Top Wings  ------------------------------

                int i = 0;
                while (i == 0)
                {
                    i = loopTopWings();
                    if (QueueRotations.Count > 500) return;
                }
                UpSlice_orientation();

                //----------Bottom Corners
                i = 0;
                toQueue(general_dir.right_dir, 0, true, true);//
                toQueue(general_dir.right_dir, 1, true, true);//
                toQueue(general_dir.right_dir, 2, true, true);//
                while (i != 4)
                {
                    i = all_to_right_bottom_corners();
                    //MessageBox.Show(i.ToString());
                    if (i != 2 && i != 4)
                        toQueue(general_dir.front_dir, 2, false, true);//
                    if (QueueRotations.Count > 500) return;
                }
                // finished positioning of bottom slice which is now on front
                //MessageBox.Show("all bottom corners are on the good positions");
                int attempt = 0;
                i = 0;
                int n1;
                int n2;
                int n3;
                int n4;
                while (i < 3)
                {
                    attempt++;
                    if (attempt == 13)
                    {
                        MessageBox.Show("something is wrong");
                        break;
                    }
                    n1 = 0;
                    n2 = 0;
                    n3 = 0;
                    n4 = 0;
                    i = how_many_goodoriented_corners(ref n1, ref n2, ref n3, ref n4);
                    
                    if (i == 1)
                        pos_1(n1, n2, n3, n4);
                    if (i == 2)
                        pos_2(n1, n2, n3, n4);
                    if (i == 3)
                    {
                        MessageBox.Show("ERROR !!!");
                        break;
                    }
                    if (i == 4)
                        break;
                    MOVE_2();
                    if (QueueRotations.Count > 500) return;
                }
                toQueue(general_dir.right_dir, 2, false, true);
                toQueue(general_dir.right_dir, 1, false, true);
                toQueue(general_dir.right_dir, 0, false, true);
                //--------Bottom Wings

                i = 1;
                while (i == 1)
                {
                    putBottomWingsToRightPlaces();
                    //MessageBox.Show("Bottom wings on the places");
                    i = putKeyHole();
                }

                //MessageBox.Show("Bottom wings on the places");

                //MessageBox.Show("Key hole is restored");
                orientationAfterBottomWings();
                //MessageBox.Show("orientation is ready");

                RightSlice_orientation();
                LeftSlice_orientation();
                //---------Middle Slice  ------------------------------------
                
                Boolean end;
                end = false;

                while (end == false)
                {
                    n1 = 0;
                    n2 = 0;
                    n3 = 0;
                    n4 = 0;
                    i = countOnRightPlace_(ref n1, ref n2, ref n3, ref n4);
                    
                    if (i == 0)
                    {
                        //V U2 V-1 U2 move 7
                        MOVE_7();
                    }
                    if (i == 1)
                    {
                        if (n1 == 1)
                        {

                            toQueue(general_dir.right_dir, 2, true, true);//V
                            toQueue(general_dir.right_dir, 2, true, true);//V

                            toQueue(general_dir.right_dir, 1, true, true);//V
                            toQueue(general_dir.right_dir, 1, true, true);//V

                            toQueue(general_dir.right_dir, 0, true, true);//V
                            toQueue(general_dir.right_dir, 0, true, true);//V


                        }
                        if (n2 == 1)
                        {
                            toQueue(general_dir.right_dir, 0, true, true);//V
                            toQueue(general_dir.right_dir, 1, true, true);//V
                            toQueue(general_dir.right_dir, 2, true, true);//V

                        }
                        if (n4 == 1)
                        {
                            toQueue(general_dir.right_dir, 0, false, true);//V
                            toQueue(general_dir.right_dir, 1, false, true);//V
                            toQueue(general_dir.right_dir, 2, false, true);//V

                        }
                        MOVE_7();
                    }
                    if (i == 2)
                    {
                        // diagonal?
                        if (n1 + n3 == 2)
                        {
                            toQueue(general_dir.right_dir, 0, true, true);//V
                            toQueue(general_dir.right_dir, 1, true, true);//V
                            toQueue(general_dir.right_dir, 2, true, true);//V

                            //MessageBox.Show("rotation to the right diagonal position");

                        }
                        if ((n2 + n4) == 2)
                        {
                            //MessageBox.Show("already in the right diagonal position");
                        }
                        MOVE_8();
                    }
                    if (i == 4)
                    {
                        int m1, m2, m3, m4;
                        m1 = 0;
                        m2 = 0;
                        m3 = 0;
                        m4 = 0;
                        int j = countOnOrientedPlace(ref m1, ref m2, ref m3, ref m4);
                        if (j == 0)
                        {
                            //MessageBox.Show("all disoriented - right position for MOVE_8");
                            MOVE_8();
                        }
                        if (j == 2)
                        {
                            if (m3 + m4 == 2)
                            {
                                //MessageBox.Show("right position for MOVE_8");
                                MOVE_8();
                            }

                            if (m1 + m4 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, false, true);//V
                                toQueue(general_dir.right_dir, 1, false, true);//V
                                toQueue(general_dir.right_dir, 2, false, true);//V
                                //MessageBox.Show("rotation 90 before MOVE_8");
                                MOVE_8();
                            }
                            if (m2 + m3 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, true, true);//V
                                toQueue(general_dir.right_dir, 1, true, true);//V
                                toQueue(general_dir.right_dir, 2, true, true);//V
                                //MessageBox.Show("rotation 90 before MOVE_8");
                                MOVE_8();
                            }
                            if (m1 + m2 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, false, true);//V
                                toQueue(general_dir.right_dir, 0, false, true);//V

                                toQueue(general_dir.right_dir, 1, false, true);//V
                                toQueue(general_dir.right_dir, 1, false, true);//V

                                toQueue(general_dir.right_dir, 2, false, true);//V
                                toQueue(general_dir.right_dir, 2, false, true);//V
                                //MessageBox.Show("rotation 180 before MOVE_8");
                                MOVE_8();
                            }

                            if (m2 + m4 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, true, true);//V
                                toQueue(general_dir.right_dir, 1, true, true);//V
                                toQueue(general_dir.right_dir, 2, true, true);//V
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                MOVE_8();
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F

                                MOVE_8();
                            }
                            if (m1 + m3 == 2)
                            {
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                MOVE_8();
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F

                                MOVE_8();
                            }
                        }

                        if (j == 4)
                        {
                            //MessageBox.Show("is done?");
                            end = true;
                        }
                    }
                    //

                }
                currentSolutionStep = solutionStep.ALL;
                PlayCurrentSolution();

                buttonWork.DisableSomeButtons();
                buttonWork.SetButtons("RandomPosition", false);
                buttonWork.FirstRandomization = 0;
            }
            private int putTopCornersToRightPositions()
            {
                int j = 3;

                int count = 0;
                top_down_orientation();
                back_side_orientation();

                define_centers();
                //--------------------------------------
                define_corners();
                if (colorCorner_11 == colorCenterTop)
                {
                    if ((solutionsAndRotations.colors_group[0, j - 1, 0, (int)cube_side.back] == colorCenterBack) &&
                        (solutionsAndRotations.colors_group[0, j - 1, 0, (int)cube_side.left] == colorCenterLeft))
                    {
                        //MessageBox.Show("corner 11 on right place");
                        count++;
                    }

                }
                else
                {
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);
                    //MessageBox.Show("180 degrees rotation");
                    putCornerToRightPlace((int)color_bin.back_color, (int)color_bin.left_color, (int)color_bin.up_color);
                    return count;
                }
                define_corners();
                if (colorCorner_31 == colorCenterTop)
                {
                    if ((solutionsAndRotations.colors_group[2, j - 1, 0, (int)cube_side.right] == colorCenterRight) &&
                        (solutionsAndRotations.colors_group[2, j - 1, 0, (int)cube_side.back] == colorCenterBack))
                    {
                        //MessageBox.Show("corner 31 on right place");
                        count++;
                    }
                }
                else
                {
                    toQueue(general_dir.up_dir, 2, true, true);
                    toQueue(general_dir.up_dir, 1, true, true);
                    toQueue(general_dir.up_dir, 0, true, true);
                    //MessageBox.Show("90 degrees rotation");
                    putCornerToRightPlace((int)color_bin.right_color, (int)color_bin.back_color, (int)color_bin.up_color);
                    return count;

                }
                define_corners();
                if (colorCorner_13 == colorCenterTop)
                {
                    if ((solutionsAndRotations.colors_group[0, j - 1, 2, (int)cube_side.left] == colorCenterLeft) &&
                        (solutionsAndRotations.colors_group[0, j - 1, 2, (int)cube_side.front] == colorCenterFront))
                    {
                        //MessageBox.Show("corner 13 on right place");
                        count++;
                    }
                }
                else
                {
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);
                    //MessageBox.Show("-90 degrees rotation");
                    putCornerToRightPlace((int)color_bin.left_color, (int)color_bin.front_color, (int)color_bin.up_color);
                    return count;
                }
                define_corners();
                if (colorCorner_33 == colorCenterTop)
                {
                    if ((solutionsAndRotations.colors_group[2, j - 1, 2, (int)cube_side.front] == colorCenterFront) &&
                        (solutionsAndRotations.colors_group[2, j - 1, 2, (int)cube_side.right] == colorCenterRight))
                    {
                        //MessageBox.Show("corner 33 on right place");
                        count++;
                    }
                }
                else
                {
                    putCornerToRightPlace((int)color_bin.front_color, (int)color_bin.right_color, (int)color_bin.up_color);

                }
                return count;
            }
            private void putBottomWingsToRightPlaces()
            {

                int count = 0;
                while (count < 4)
                {
                    if (QueueRotations.Count > 500) return;
                    count = 0;
                    for (int loop = 0; loop < 4; loop++)
                    {
                        if ((solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.front] == solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.front]) &&
                        (solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.down] == solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.down]))
                        {// bottom wing is on good position and well oriented
                            //MessageBox.Show((loop + 1).ToString() + " bottom wing is done");
                            count++;
                            if (loop < 3)
                            {
                                toQueue(general_dir.up_dir, 0, false, true);

                                //MessageBox.Show("rotation 90 to next side");
                            }

                        }
                        else
                        {
                            FIGURE_9_or_10();

                        }
                    }
                }
            }
            private void FIGURE_9_or_10()
            {
                // 
                int res;
                res = findBottomWingOnMiddleHorizontalSlice(solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.front], solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.down]);
                if (res == 1) FIGURE_9();
                if (res == 2) FIGURE_10();
                if (res == 0)
                {
  
                    //MessageBox.Show("***");

                }
                if (res == 3)
                {

                    //MessageBox.Show("inserting into the middle slice is done");

                }
                if (res == 4)
                {
      
                    //MessageBox.Show("gonna leave this wing as a last one");

                }
            }
            private int findBottomWingOnMiddleHorizontalSlice(int front, int down)
            {
                for (int i = 0; i < 4; i++)
                {
                    int right_321 = solutionsAndRotations.colors_group[2, 1, 0, (int)cube_side.right];
                    int back_321 = solutionsAndRotations.colors_group[2, 1, 0, (int)cube_side.back];
                    if ((right_321 == down) && (back_321 == front))
                    {
                        return 1;
                    }
                    if ((right_321 == front) && (back_321 == down))
                    {
                        return 2;
                    }
                    toQueue(general_dir.up_dir, 1, false, true);
                    //MessageBox.Show("next middle rotation");

                }
                int right_312 = solutionsAndRotations.colors_group[2, 0, 1, (int)cube_side.right];
                int down_312 = solutionsAndRotations.colors_group[2, 0, 1, (int)cube_side.down];
                if ((front + down) == (right_312 + down_312))
                {
                    toQueue(general_dir.up_dir, 0, true, true);
                    FIGURE_9();
                    //MessageBox.Show("Wrong spot");
                    return 3;
                }
                int back_211 = solutionsAndRotations.colors_group[1, 0, 0, (int)cube_side.back];
                int down_211 = solutionsAndRotations.colors_group[1, 0, 0, (int)cube_side.down];
                if ((front + down) == (back_211 + down_211))
                {
                    toQueue(general_dir.up_dir, 0, true, true);
                    toQueue(general_dir.up_dir, 0, true, true);
                    FIGURE_9();
                    //MessageBox.Show("Wrong back spot");
                    return 3;
                }
                int left_112 = solutionsAndRotations.colors_group[0, 0, 1, (int)cube_side.left];
                int down_112 = solutionsAndRotations.colors_group[0, 0, 1, (int)cube_side.down];
                if ((front + down) == (left_112 + down_112))
                {
                    toQueue(general_dir.up_dir, 0, false, true);
                    FIGURE_9();
                    //MessageBox.Show("Wrong spot");
                    return 3;
                }
                int front_213 = solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.front];
                int down_213 = solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.down];
                if ((front + down) == (front_213 + down_213))
                {

                    FIGURE_9();// move 3
                    //MessageBox.Show("Oriented Wrong");
                    return 3;
                }
                int front_233 = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.front];
                int up_233 = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.up];
                if ((front + down) == (front_233 + up_233))
                {

                    FIGURE_10();
                    //MessageBox.Show("we are in keyhole");
                    return 4;
                }
                //MessageBox.Show("nothing found to insert into bottom wing");
                return 0;

            }
            private void FIGURE_9()
            {
                //F H2 F-1 - move 3
                toQueue(general_dir.front_dir, 2, true, true);
                toQueue(general_dir.up_dir, 1, true, true);
                toQueue(general_dir.up_dir, 1, true, true);
                toQueue(general_dir.front_dir, 2, false, true);
                //MessageBox.Show("figure 9 is done");
            }
            private void FIGURE_10()
            {
                //F-1 H F  - move 4
                toQueue(general_dir.front_dir, 2, false, true);
                toQueue(general_dir.up_dir, 1, true, true);

                toQueue(general_dir.front_dir, 2, true, true);
                //MessageBox.Show("figure 10 is done");
            }

            private void findCorner(ref int i_, ref int j_, ref int k_, int sum)
            {

                int sum_temp;
                for (int i = 0; i < N + 0; i += N - 1)
                {
                    for (int j = 0; j < N + 0; j += N - 1)
                    {
                        for (int k = 0; k < N + 0; k += N - 1)
                        {
                            sum_temp = 0;
                            foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                            {
                                sum_temp += solutionsAndRotations.colors_group[i, j, k, (int)c];
                            }
                            if (sum_temp == sum)
                            {
                                i_ = i;
                                j_ = j;
                                k_ = k;
                                break;
                            }
                        }
                    }
                }

            }
            //---------------------------------------------------------------------
            private void define_corners()
            {
                int j = 3;
                colorCorner_11 = solutionsAndRotations.colors_group[0, j - 1, 0, (int)cube_side.up];
                colorCorner_31 = solutionsAndRotations.colors_group[2, j - 1, 0, (int)cube_side.up];
                colorCorner_33 = solutionsAndRotations.colors_group[2, j - 1, 2, (int)cube_side.up];
                colorCorner_13 = solutionsAndRotations.colors_group[0, j - 1, 2, (int)cube_side.up];
            }
            private void define_wings()
            {
                int j = 3;
                colorWing_12 = solutionsAndRotations.colors_group[0, j - 1, 1, (int)cube_side.up];
                colorWing_21 = solutionsAndRotations.colors_group[1, j - 1, 0, (int)cube_side.up];
                colorWing_32 = solutionsAndRotations.colors_group[2, j - 1, 1, (int)cube_side.up];
                colorWing_23 = solutionsAndRotations.colors_group[1, j - 1, 2, (int)cube_side.up];
            }

            //-----------------------------------------------------------------------
            public void TopWings()
            {
                copyMirrors();//-----------------------------------
                int i = 0;
                while (i == 0)
                {
                    i = loopTopWings();
                    if (QueueRotations.Count > 500)
                    {
                        MessageBox.Show("Error");
                        return;
                    }
                }
                UpSlice_orientation();
                currentSolutionStep = solutionStep.II_step;
                PlayCurrentSolution();
            }
            private int loopTopWings()
            {
                int j = 3;

                top_down_orientation();
                back_side_orientation();

                define_centers();
                //--------------------------------------
                define_wings();
                if ((colorWing_23 == colorCenterTop) && (solutionsAndRotations.colors_group[1, j - 1, 2, (int)cube_side.front] == solutionsAndRotations.colors_group[0, j - 1, 2, (int)cube_side.front]))
                {
                    ////MessageBox.Show("wing 233 on right place");

                }
                else
                {
                    putWingsToRightPlace(solutionsAndRotations.colors_group[0, j - 1, 2, (int)cube_side.front], (int)color_bin.up_color);
                    return 0;


                }
                //define_wings();
                if ((colorWing_12 == colorCenterTop) && (solutionsAndRotations.colors_group[0, j - 1, 1, (int)cube_side.left] == solutionsAndRotations.colors_group[0, j - 1, 0, (int)cube_side.left]))
                {

                    //MessageBox.Show("wing 132 on right place");
                }
                else
                {
                    int s = solutionsAndRotations.colors_group[0, j - 1, 0, (int)cube_side.left];
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);

                    putWingsToRightPlace(s, (int)color_bin.up_color);
                    return 0;

                }
                //define_wings();
                if ((colorWing_21 == colorCenterTop) && (solutionsAndRotations.colors_group[1, j - 1, 0, (int)cube_side.back] == solutionsAndRotations.colors_group[2, j - 1, 0, (int)cube_side.back]))
                {
                    //MessageBox.Show("wing 231 on right place");

                }
                else
                {
                    int s;
                    s = solutionsAndRotations.colors_group[2, j - 1, 0, (int)cube_side.back];
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 1, false, true);
                    toQueue(general_dir.up_dir, 0, false, true);
                    //MessageBox.Show("180 degrees rotation");
                    putWingsToRightPlace(s, (int)color_bin.up_color);
                    return 0;
                }
                //define_wings();
                if ((colorWing_32 == colorCenterTop) && (solutionsAndRotations.colors_group[2, j - 1, 1, (int)cube_side.right] == solutionsAndRotations.colors_group[2, j - 1, 2, (int)cube_side.right]))
                {
                    //MessageBox.Show("wing 332 on right place");
                    return 1;
                }
                else
                {
                    int s;
                    s = solutionsAndRotations.colors_group[2, j - 1, 2, (int)cube_side.right];
                    toQueue(general_dir.up_dir, 2, true, true);
                    toQueue(general_dir.up_dir, 1, true, true);
                    toQueue(general_dir.up_dir, 0, true, true);
                    
                    //MessageBox.Show("-90 degrees rotation");
                    putWingsToRightPlace(s, (int)color_bin.up_color);
                    return 0;
                }
                
            }
            //----------------------------------------------------------------------------------------------
            private void putWingsToRightPlace(int rel_front, int rel_top)
            {
                int i_ = -1;
                int j_ = -1;
                int k_ = -1;

                findWing(ref i_, ref  j_, ref k_, rel_front + rel_top);
                if (i_ == 1 && j_ == 2 && k_ == 2)// up wing
                {
                    //MessageBox.Show("up position for work with wing");

                    // --------VI-------------

                    if ((solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.front] == rel_top) &&
                        (solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.up] == rel_front))
                    {
                        X();// step is done
                        //MessageBox.Show("233 goes to VI or VII later");
                    }
                }
                if (i_ == 1 && j_ == 2 && k_ == 0)// up far wing
                {
                    toQueue(general_dir.up_dir, 2, false, true);
                    toQueue(general_dir.up_dir, 2, false, true);
                    //MessageBox.Show("180 degrees rotation");
                    X();
                    //MessageBox.Show("231 goes to VI or VII later");
                }
                if (i_ == 0 && j_ == 2 && k_ == 1)// up left wing
                {
                    toQueue(general_dir.up_dir, 2, false, true);

                    //MessageBox.Show("90 degrees rotation before X");
                    X();
                    toQueue(general_dir.up_dir, 2, true, true);//--------------------------------
                    //MessageBox.Show("132 goes to VI or VII later");
                }
                if (i_ == 2 && j_ == 2 && k_ == 1)// up right wing
                {
                    toQueue(general_dir.up_dir, 2, true, true);

                    //MessageBox.Show("90 degrees rotation before X");
                    X();
                    toQueue(general_dir.up_dir, 2, false, true);//--------------------------------
                    //MessageBox.Show("332 goes to VI or VII later");
                }
                if (i_ == 0 && j_ == 1 && k_ == 2)// middle slice
                {

                    toQueue(general_dir.up_dir, 1, false, true);//H

                    //MessageBox.Show("123 goes right");

                }
                if (i_ == 2 && j_ == 1 && k_ == 0)// middle slice
                {

                    toQueue(general_dir.up_dir, 1, true, true);//H

                    //MessageBox.Show("321 goes left");

                }
                if (i_ == 0 && j_ == 1 && k_ == 0)// middle slice
                {

                    toQueue(general_dir.up_dir, 1, false, true);//H
                    toQueue(general_dir.up_dir, 1, false, true);//H
                    //MessageBox.Show("121 goes 180 degrees");

                }

                if (i_ == 0 && j_ == 0 && k_ == 1)//down slice
                {

                    toQueue(general_dir.up_dir, 0, false, true);// F

                    //MessageBox.Show("112 goes right");
                }
                if (i_ == 2 && j_ == 0 && k_ == 1)//down slice
                {

                    toQueue(general_dir.up_dir, 0, true, true);// F

                    //MessageBox.Show("312 goes left");
                }
                if (i_ == 1 && j_ == 0 && k_ == 0)//down slice
                {

                    toQueue(general_dir.up_dir, 0, false, true);// F
                    toQueue(general_dir.up_dir, 0, false, true);// F

                    //MessageBox.Show("211 goes 180");
                }


                findWing(ref i_, ref  j_, ref k_, rel_front + rel_top);

                if (i_ == 2 && j_ == 1 && k_ == 2)
                {
                    //MessageBox.Show("right position for work with wing");

                    // --------VI-------------

                    if ((solutionsAndRotations.colors_group[2, 1, 2, (int)cube_side.front] == rel_front) &&
                        (solutionsAndRotations.colors_group[2, 1, 2, (int)cube_side.right] == rel_top))
                    {
                        //MessageBox.Show("VI step is done");
                        toQueue(general_dir.up_dir, 1, false, true);//H-1
                        toQueue(general_dir.front_dir, 2, true, true);//F
                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.front_dir, 2, false, true);//F-1
                    }
                    //------VII-------------------------------------------------------
                    if ((solutionsAndRotations.colors_group[2, 1, 2, (int)cube_side.front] == rel_top) &&
                        (solutionsAndRotations.colors_group[2, 1, 2, (int)cube_side.right] == rel_front))
                    {
                        //MessageBox.Show("VII step is done");


                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.front_dir, 2, false, true);//F-1
                        toQueue(general_dir.up_dir, 1, false, true);//H-1
                        toQueue(general_dir.front_dir, 2, true, true);//F
                    }
                }
                if (i_ == 1 && j_ == 0 && k_ == 2)
                {
                    //MessageBox.Show("second right position for work with wing");
                    //------VIII-------------------------------------------------------

                    if ((solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.front] == rel_top) &&
                        (solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.down] == rel_front))
                    {
                        //MessageBox.Show("VIII step is done");
                        toQueue(general_dir.front_dir, 2, false, true);//F-1
                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.front_dir, 2, true, true);//F
                    }
                    //------IX-------------------------------------------------------
                    if ((solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.front] == rel_front) &&
                        (solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.down] == rel_top))
                    {
                        //MessageBox.Show("IX step is done");
                        toQueue(general_dir.front_dir, 2, false, true);//F-1
                        toQueue(general_dir.up_dir, 1, false, true);//H-1

                        toQueue(general_dir.front_dir, 2, true, true);//F
                        toQueue(general_dir.front_dir, 2, true, true);//F
                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.front_dir, 2, false, true);//F-1
                    }
                }

            }

            void X()
            {
                //MessageBox.Show("X step is done");

                toQueue(general_dir.front_dir, 2, true, true);//F
                toQueue(general_dir.up_dir, 1, false, true);//H-1
                toQueue(general_dir.front_dir, 2, false, true);//F-1
                toQueue(general_dir.up_dir, 1, true, true);//H
            }
            //-----------------------------------------------------------------------
            void findWing(ref int i_, ref int j_, ref int k_, int sum)
            {

                int sum_temp;
                for (int i = 0; i < N + 0; i++)
                {
                    for (int j = 0; j < N + 0; j++)
                    {
                        for (int k = 0; k < N + 0; k++)
                        {
                            sum_temp = 0;
                            foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                            {
                                sum_temp += solutionsAndRotations.colors_group[i, j, k, (int)c];
                            }
                            if (sum_temp == sum)
                            {
                                i_ = i;
                                j_ = j;
                                k_ = k;
                                break;
                            }
                        }
                    }
                }

            }

            public void BottomCorners()
            {
                copyMirrors();//-----------------------------------

                int i;

                i = 0;
                toQueue(general_dir.right_dir, 0, true, true);//
                toQueue(general_dir.right_dir, 1, true, true);//
                toQueue(general_dir.right_dir, 2, true, true);//
                while (i != 4)
                {
                    i = all_to_right_bottom_corners();
                    //MessageBox.Show(i.ToString());
                    if (i != 2 && i != 4)
                        toQueue(general_dir.front_dir, 2, false, true);//
                    if (QueueRotations.Count > 500) return;
                }
                // finished positioning of bottom slice which is now on front
                //MessageBox.Show("all bottom corners are on the good positions");
                int attempt = 0;
                i = 0;
                int n1;
                int n2;
                int n3;
                int n4;
                while (i < 3)
                {
                    attempt++;
                    if (attempt == 13)
                    {
                        MessageBox.Show("something is wrong");
                        break;
                    }
                    n1 = 0;
                    n2 = 0;
                    n3 = 0;
                    n4 = 0;
                    i = how_many_goodoriented_corners(ref n1, ref n2, ref n3, ref n4);
                    //MessageBox.Show(i.ToString());
                    if (i == 1)
                        pos_1(n1, n2, n3, n4);
                    if (i == 2)
                        pos_2(n1, n2, n3, n4);
                    if (i == 3)
                    {
                        MessageBox.Show("ERROR !!!");
                        break;
                    }
                    if (i == 4)
                        break;
                    MOVE_2();
                    if (QueueRotations.Count > 500)
                    {
                        MessageBox.Show("Error");
                        return;
                    }
                }
                toQueue(general_dir.right_dir, 2, false, true);
                toQueue(general_dir.right_dir, 1, false, true);
                toQueue(general_dir.right_dir, 0, false, true);

                currentSolutionStep = solutionStep.III_step;

                PlayCurrentSolution();
            }
            private int all_to_right_bottom_corners()
            {

                int front_color = (int)color_bin.down_color;
                int right_color = (int)color_bin.right_color;

                int left_color = (int)color_bin.left_color;
                int down_color = (int)color_bin.back_color;
                int up_color = (int)color_bin.front_color;

                int pos_sum1 = 0;
                int i1 = findBottomCorner(0, 2, 2, ref pos_sum1, left_color + front_color + up_color);

                int pos_sum2 = 0;
                int i2 = findBottomCorner(2, 2, 2, ref pos_sum2, front_color + right_color + up_color);

                int pos_sum3 = 0;
                int i3 = findBottomCorner(0, 0, 2, ref pos_sum3, front_color + down_color + left_color);

                int pos_sum4 = 0;
                int i4 = findBottomCorner(2, 0, 2, ref pos_sum4, front_color + right_color + down_color);

                int res = i1 + i2 + i3 + i4;
                if (res != 2)
                    return res;

                int sum_i_j;
                sum_i_j = pos_sum1 + pos_sum2 + pos_sum3 + pos_sum4;
                if (sum_i_j == 8)
                {
                    // diagonal
                    //MessageBox.Show("Diagonal");
                    if (i1 == 0 && i4 == 0) // up horizontal parallel
                    {
                        toQueue(general_dir.front_dir, 2, true, true);// wrong positions must rotate to 180 to be down
                        //MessageBox.Show("rotation to 90");
                    }
                    if (i2 == 0 && i3 == 0) // up horizontal parallel
                    {
                        //MessageBox.Show("nothing to rotate - good position ");
                    }
                    //--------
                    MOVE_1();
                    //--------

                }
                if ((sum_i_j == 10) || (sum_i_j == 6))
                {
                    if (sum_i_j == 6)
                    {
                        // parallel
                        //MessageBox.Show("Parallel");
                        if (i1 == 0 && i2 == 0) // up horizontal parallel
                        {
                            toQueue(general_dir.front_dir, 2, true, true);// wrong positions must rotate to 180 to be down
                            toQueue(general_dir.front_dir, 2, true, true);
                            //MessageBox.Show("rotation to 180");
                        }
                        if (i2 == 0 && i4 == 0) // right vertical parallel
                        {
                            toQueue(general_dir.front_dir, 2, true, true);// wrong positions must rotate to 90 to be down

                            //MessageBox.Show("rotation to 90");
                        }

                    }
                    if (sum_i_j == 10)
                    {
                        // parallel
                        //MessageBox.Show("Parallel");
                        if (i1 == 0 && i3 == 0) // down horizontal parallel
                        {


                            //MessageBox.Show("right position for swapping");

                        }
                        if (i3 == 0 && i4 == 0) // left vertical parallel
                        {
                            toQueue(general_dir.front_dir, 2, false, true);// wrong positions must rotate to 90 to be down

                            //MessageBox.Show("rotation to 90");
                        }
                    }
                    MOVE_1();

                }
                return res;

            }
            private void prepairToMove2()
            {
                //if (count_bottom_corners >2) return;
                count_RightOriented_bottom_corners = 0;
                int n1;
                int n2;
                int n3;
                int n4;
                attempt++;
                if (attempt == 13)
                {
                    MessageBox.Show("something is wrong");
                    return;
                }
                n1 = 0;
                n2 = 0;
                n3 = 0;
                n4 = 0;
                count_RightOriented_bottom_corners = how_many_goodoriented_corners(ref n1, ref n2, ref n3, ref n4);

                //MessageBox.Show(i.ToString());
                if (count_RightOriented_bottom_corners == 1)
                    pos_1(n1, n2, n3, n4);
                if (count_RightOriented_bottom_corners == 2)
                    pos_2(n1, n2, n3, n4);
                if (count_RightOriented_bottom_corners == 3)
                {
                    MessageBox.Show("ERROR !!!");
                    return;
                }
                if (count_RightOriented_bottom_corners == 4)
                    return;
                MOVE_2();
                //LaunchPlayQueue(3);

            }

            //------------------------------
            private void pos_1(int n1, int n2, int n3, int n4)
            {
                if (n1 == 1)
                {
                    toQueue(general_dir.front_dir, 2, true, true);//F
                    toQueue(general_dir.front_dir, 1, true, true);//F
                    toQueue(general_dir.front_dir, 0, true, true);//F
                    //MessageBox.Show("F");
                }
                if (n3 == 1)
                {
                    toQueue(general_dir.front_dir, 2, true, true);//F
                    toQueue(general_dir.front_dir, 2, true, true);//F

                    toQueue(general_dir.front_dir, 1, true, true);//F
                    toQueue(general_dir.front_dir, 1, true, true);//F

                    toQueue(general_dir.front_dir, 0, true, true);//F
                    toQueue(general_dir.front_dir, 0, true, true);//F
                    //MessageBox.Show("180");
                }
                if (n4 == 1)
                {
                    toQueue(general_dir.front_dir, 2, false, true);//F-1
                    toQueue(general_dir.front_dir, 1, false, true);//F-1
                    toQueue(general_dir.front_dir, 0, false, true);//F-1
                    //MessageBox.Show("F-1");
                }
            }
            private void pos_2(int n1, int n2, int n3, int n4)
            {
                // 1,3 right postition 
                if ((n1 == 1) && (n2 == 1))
                {
                    toQueue(general_dir.front_dir, 2, false, true);//F-1
                    toQueue(general_dir.front_dir, 1, false, true);//F-1
                    toQueue(general_dir.front_dir, 0, false, true);//F-1
                    //MessageBox.Show("front - 1");
                }
                if ((n3 == 1) && (n4 == 1))//
                {
                    toQueue(general_dir.front_dir, 2, true, true);//F
                    toQueue(general_dir.front_dir, 1, true, true);//F
                    toQueue(general_dir.front_dir, 0, true, true);//F
                    //MessageBox.Show("front");

                }
                if ((n2 == 1) && (n4 == 1))//
                {
                    toQueue(general_dir.front_dir, 2, true, true);//F
                    toQueue(general_dir.front_dir, 2, true, true);//F

                    toQueue(general_dir.front_dir, 1, true, true);//F
                    toQueue(general_dir.front_dir, 1, true, true);//F

                    toQueue(general_dir.front_dir, 0, true, true);//F
                    toQueue(general_dir.front_dir, 0, true, true);//F

                    //MessageBox.Show("180");
                }
            }
            //-----------
            private int how_many_goodoriented_corners(ref int n1, ref int n2, ref int n3, ref int n4)
            {

                int j = 3;

                define_centers();

                if ((solutionsAndRotations.colors_group[0, 2, j - 1, (int)cube_side.left] == colorCenterLeft) &&
                    (solutionsAndRotations.colors_group[0, 2, j - 1, (int)cube_side.up] == colorCenterTop) &&
                    (solutionsAndRotations.colors_group[0, 2, j - 1, (int)cube_side.front] == colorCenterFront))
                    n1 = 1;

                if ((solutionsAndRotations.colors_group[2, 2, j - 1, (int)cube_side.right] == colorCenterRight) &&
                    (solutionsAndRotations.colors_group[2, 2, j - 1, (int)cube_side.up] == colorCenterTop) &&
                    (solutionsAndRotations.colors_group[2, 2, j - 1, (int)cube_side.front] == colorCenterFront))
                    n2 = 1;

                if ((solutionsAndRotations.colors_group[0, 0, j - 1, (int)cube_side.left] == colorCenterLeft) &&
                    (solutionsAndRotations.colors_group[0, 0, j - 1, (int)cube_side.down] == colorCenterDown) &&
                    (solutionsAndRotations.colors_group[0, 0, j - 1, (int)cube_side.front] == colorCenterFront))
                    n3 = 1;

                if ((solutionsAndRotations.colors_group[2, 0, j - 1, (int)cube_side.right] == colorCenterRight) &&
                    (solutionsAndRotations.colors_group[2, 0, j - 1, (int)cube_side.down] == colorCenterDown) &&
                    (solutionsAndRotations.colors_group[2, 0, j - 1, (int)cube_side.front] == colorCenterFront))
                    n4 = 1;

                int n = n1 + n2 + n3 + n4;
                return n;

            }

            private void MOVE_2()
            {
                //U-1 F2 U F   U-1 F U F2 
                toQueue(general_dir.up_dir, 2, false, true);//U-1

                toQueue(general_dir.front_dir, 2, true, true);//F
                toQueue(general_dir.front_dir, 2, true, true);//F
                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.front_dir, 2, true, true);//F
                //-----------------------
                toQueue(general_dir.up_dir, 2, false, true);//U-1

                toQueue(general_dir.front_dir, 2, true, true);//F

                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.front_dir, 2, true, true);//F
                toQueue(general_dir.front_dir, 2, true, true);//F
                //MessageBox.Show("MOVE 2");

            }

            //-------------------
            private void MOVE_1()
            {
                // U-1 F U L-1  
                toQueue(general_dir.up_dir, 2, false, true);//U-1
                toQueue(general_dir.front_dir, 2, true, true);//F
                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.right_dir, 0, true, true);//L-1
                // U L U-1 F2
                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.right_dir, 0, false, true);//L
                toQueue(general_dir.up_dir, 2, false, true);//U-1
                toQueue(general_dir.front_dir, 2, true, true);//F
                toQueue(general_dir.front_dir, 2, true, true);//F
            }
            private int findBottomCorner(int i_, int j_, int k_, ref int pos_sum, int sum)
            {

                int sum_temp;
                pos_sum = 0;
                sum_temp = 0;
                foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                {
                    sum_temp += solutionsAndRotations.colors_group[i_, j_, k_, (int)c];
                }
                if (sum_temp == sum)
                {
                    //------------------------------------------------------------------------------------
                    pos_sum = i_ + j_ + 2;// + 2 for according with 1... N instead of current 0... N-1
                    //------------------------------------------------------------------------------------
                    return 1;
                }
                return 0;
            }

            private void orientationAfterBottomWings()
            {
                int front = 0;
                bool end = false;
                

                front = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.front];
                if (front == (int)color_bin.front_color)
                {
                    end = true;
                }
                else
                {
                    toQueue(general_dir.up_dir, 2, true, true);//F
                }
                
                end = false;
                

                front = solutionsAndRotations.colors_group[1, 1, 2, (int)cube_side.front];
                if (front == (int)color_bin.front_color)
                {
                    end = true;
                }
                else
                {
                    toQueue(general_dir.up_dir, 1, true, true);//F
                }
                
                end = false;
                

                front = solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.front];
                if (front == (int)color_bin.front_color)
                {
                    end = true;
                }
                else
                {
                    toQueue(general_dir.up_dir, 0, true, true);
                }
                
                toQueue(general_dir.front_dir, 2, true, true);//F
                toQueue(general_dir.front_dir, 1, true, true);//F
                toQueue(general_dir.front_dir, 0, true, true);//F
            }

            private int putKeyHole()
            {
                int up = solutionsAndRotations.colors_group[0, 2, 2, (int)cube_side.up];
                int front = solutionsAndRotations.colors_group[0, 2, 2, (int)cube_side.front];

                int up_hole = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.up];
                int front_hole = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.front];
                if ((up == up_hole) && (front == front_hole))
                {
                    return 0;
                }
                if (up + front == up_hole + front_hole)
                {
                    FIGURE_10();
                    return 1;
                }
                for (int i = 0; i < 4; i++)
                {
                    int right_323 = solutionsAndRotations.colors_group[2, 1, 2, (int)cube_side.right];
                    int front_323 = solutionsAndRotations.colors_group[2, 1, 2, (int)cube_side.front];
                    if ((right_323 == up) && (front_323 == front))
                    {
                        //F H-1 F-1   H-1 F H F-1 
                        // figure 11 move 5
                        toQueue(general_dir.front_dir, 2, true, true);//F
                        toQueue(general_dir.up_dir, 1, false, true);//H-1
                        toQueue(general_dir.front_dir, 2, false, true);//F-1

                        toQueue(general_dir.up_dir, 1, false, true);//H-1
                        toQueue(general_dir.front_dir, 2, true, true);//F
                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.front_dir, 2, false, true);//F-1

                        //MessageBox.Show("figure 11 is done");
                    }
                    if ((right_323 == front) && (front_323 == up))
                    {
                        //F H-1 F-1   H2 F-1 H-1 F 
                        // figure 12 move 6
                        toQueue(general_dir.front_dir, 2, true, true);//F
                        toQueue(general_dir.up_dir, 1, false, true);//H-1
                        toQueue(general_dir.front_dir, 2, false, true);//F-1

                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.up_dir, 1, true, true);//H
                        toQueue(general_dir.front_dir, 2, false, true);//F-1
                        toQueue(general_dir.up_dir, 1, false, true);//H-1
                        toQueue(general_dir.front_dir, 2, true, true);//F
                        //MessageBox.Show("figure 12 is done");
                    }
                    toQueue(general_dir.up_dir, 1, false, true);
                    //MessageBox.Show("next middle rotation");
                }
                return 0;
            }
            public void MiddleSlice()
            {
                int n1, n2, n3, n4;
                int i;

                Boolean end;
                end = false;
                copyMirrors();//-----------------------------------
                while (end == false)
                {
                    n1 = 0;
                    n2 = 0;
                    n3 = 0;
                    n4 = 0;
                    i = countOnRightPlace_(ref n1, ref n2, ref n3, ref n4);
                    //MessageBox.Show(i.ToString());
                    if (i == 0)
                    {
                        //V U2 V-1 U2 move 7
                        MOVE_7();
                    }
                    if (i == 1)
                    {
                        if (n1 == 1)
                        {

                            toQueue(general_dir.right_dir, 2, true, true);//V
                            toQueue(general_dir.right_dir, 2, true, true);//V

                            toQueue(general_dir.right_dir, 1, true, true);//V
                            toQueue(general_dir.right_dir, 1, true, true);//V

                            toQueue(general_dir.right_dir, 0, true, true);//V
                            toQueue(general_dir.right_dir, 0, true, true);//V


                        }
                        if (n2 == 1)
                        {
                            toQueue(general_dir.right_dir, 0, true, true);//V
                            toQueue(general_dir.right_dir, 1, true, true);//V
                            toQueue(general_dir.right_dir, 2, true, true);//V

                        }
                        if (n4 == 1)
                        {
                            toQueue(general_dir.right_dir, 0, false, true);//V
                            toQueue(general_dir.right_dir, 1, false, true);//V
                            toQueue(general_dir.right_dir, 2, false, true);//V

                        }
                        MOVE_7();
                    }
                    if (i == 2)
                    {
                        // diagonal?
                        if (n1 + n3 == 2)
                        {
                            toQueue(general_dir.right_dir, 0, true, true);//V
                            toQueue(general_dir.right_dir, 1, true, true);//V
                            toQueue(general_dir.right_dir, 2, true, true);//V

                            //MessageBox.Show("rotation to the right diagonal position");

                        }
                        if ((n2 + n4) == 2)
                        {
                            //MessageBox.Show("already in the right diagonal position");
                        }
                        MOVE_8();
                    }
                    if (i == 4)
                    {
                        int m1, m2, m3, m4;
                        m1 = 0;
                        m2 = 0;
                        m3 = 0;
                        m4 = 0;
                        int j = countOnOrientedPlace(ref m1, ref m2, ref m3, ref m4);
                        if (j == 0)
                        {
                            //MessageBox.Show("all disoriented - right position for MOVE_8");
                            MOVE_8();
                        }
                        if (j == 2)
                        {
                            if (m3 + m4 == 2)
                            {
                                //MessageBox.Show("right position for MOVE_8");
                                MOVE_8();
                            }

                            if (m1 + m4 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, false, true);//V
                                toQueue(general_dir.right_dir, 1, false, true);//V
                                toQueue(general_dir.right_dir, 2, false, true);//V
                                //MessageBox.Show("rotation 90 before MOVE_8");
                                MOVE_8();
                            }
                            if (m2 + m3 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, true, true);//V
                                toQueue(general_dir.right_dir, 1, true, true);//V
                                toQueue(general_dir.right_dir, 2, true, true);//V
                                //MessageBox.Show("rotation 90 before MOVE_8");
                                MOVE_8();
                            }
                            if (m1 + m2 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, false, true);//V
                                toQueue(general_dir.right_dir, 0, false, true);//V

                                toQueue(general_dir.right_dir, 1, false, true);//V
                                toQueue(general_dir.right_dir, 1, false, true);//V

                                toQueue(general_dir.right_dir, 2, false, true);//V
                                toQueue(general_dir.right_dir, 2, false, true);//V
                                //MessageBox.Show("rotation 180 before MOVE_8");
                                MOVE_8();
                            }

                            if (m2 + m4 == 2)
                            {
                                toQueue(general_dir.right_dir, 0, true, true);//V
                                toQueue(general_dir.right_dir, 1, true, true);//V
                                toQueue(general_dir.right_dir, 2, true, true);//V
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                MOVE_8();
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F

                                MOVE_8();
                            }
                            if (m1 + m3 == 2)
                            {
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                MOVE_8();
                                //f2
                                toQueue(general_dir.front_dir, 2, true, true);//F
                                toQueue(general_dir.front_dir, 2, true, true);//F

                                MOVE_8();
                            }
                        }
                        if (j == 4)
                        {
                            //MessageBox.Show("is done?");
                            end = true;
                        }
                    }

                }
                currentSolutionStep = solutionStep.V_step;
                PlayCurrentSolution();
                buttonWork.FirstRandomization = 0;
            }
            private void MOVE_7()
            {
                //V U2 V-1 U2 move 7
                toQueue(general_dir.right_dir, 1, true, true);//V

                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.up_dir, 2, true, true);//U

                toQueue(general_dir.right_dir, 1, false, true);//V-1

                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.up_dir, 2, true, true);//U

            }
            private void MOVE_8()
            {

                // V U V U V U2   


                toQueue(general_dir.right_dir, 1, true, true);//V

                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.right_dir, 1, true, true);//V

                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.right_dir, 1, true, true);//V

                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.up_dir, 2, true, true);//U

                //V-1 U V-1 U V-1 U2 

                toQueue(general_dir.right_dir, 1, false, true);//V-1
                toQueue(general_dir.up_dir, 2, true, true);//U

                toQueue(general_dir.right_dir, 1, false, true);//V-1
                toQueue(general_dir.up_dir, 2, true, true);//U

                toQueue(general_dir.right_dir, 1, false, true);//V-1

                toQueue(general_dir.up_dir, 2, true, true);//U
                toQueue(general_dir.up_dir, 2, true, true);//U

            }
            private int countOnRightPlace_(ref int n1, ref int n2, ref int n3, ref int n4)
            {
                int count = 0;

                int good_1;
                int good_2;
                int current_1;
                int current_2;

                good_1 = solutionsAndRotations.colors_group[0, 2, 2, (int)cube_side.front];
                good_2 = solutionsAndRotations.colors_group[0, 2, 2, (int)cube_side.up];

                current_1 = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.front];
                current_2 = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.up];

                if (good_1 + good_2 == current_1 + current_2)
                {
                    n1 = 1;
                    count++;
                }
                //------------- II
                good_1 = solutionsAndRotations.colors_group[0, 2, 0, (int)cube_side.up];
                good_2 = solutionsAndRotations.colors_group[0, 2, 0, (int)cube_side.back];

                current_1 = solutionsAndRotations.colors_group[1, 2, 0, (int)cube_side.up];
                current_2 = solutionsAndRotations.colors_group[1, 2, 0, (int)cube_side.back];

                if (good_1 + good_2 == current_1 + current_2)
                {
                    n2 = 1;
                    count++;
                }

                //------------- III
                good_1 = solutionsAndRotations.colors_group[0, 0, 0, (int)cube_side.down];
                good_2 = solutionsAndRotations.colors_group[0, 0, 0, (int)cube_side.back];

                current_1 = solutionsAndRotations.colors_group[1, 0, 0, (int)cube_side.down];
                current_2 = solutionsAndRotations.colors_group[1, 0, 0, (int)cube_side.back];

                if (good_1 + good_2 == current_1 + current_2)
                {
                    n3 = 1;
                    count++;
                }
                //------------- IV
                good_1 = solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.down];
                good_2 = solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.front];

                current_1 = solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.down];
                current_2 = solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.front];

                if (good_1 + good_2 == current_1 + current_2)
                {
                    n4 = 1;
                    count++;
                }
                return count;
            }

            private int countOnOrientedPlace(ref int n1, ref int n2, ref int n3, ref int n4)
            {
                int count = 0;

                int good_1;
                int good_2;
                int current_1;
                int current_2;

                good_1 = solutionsAndRotations.colors_group[0, 2, 2, (int)cube_side.front];
                good_2 = solutionsAndRotations.colors_group[0, 2, 2, (int)cube_side.up];

                current_1 = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.front];
                current_2 = solutionsAndRotations.colors_group[1, 2, 2, (int)cube_side.up];

                if ((good_1 == current_1) && (good_2 == current_2))
                {
                    n1 = 1;
                    count++;
                }
                //------------- II
                good_1 = solutionsAndRotations.colors_group[0, 2, 0, (int)cube_side.up];
                good_2 = solutionsAndRotations.colors_group[0, 2, 0, (int)cube_side.back];

                current_1 = solutionsAndRotations.colors_group[1, 2, 0, (int)cube_side.up];
                current_2 = solutionsAndRotations.colors_group[1, 2, 0, (int)cube_side.back];

                if ((good_1 == current_1) && (good_2 == current_2))
                {
                    n2 = 1;
                    count++;
                }

                //------------- III
                good_1 = solutionsAndRotations.colors_group[0, 0, 0, (int)cube_side.down];
                good_2 = solutionsAndRotations.colors_group[0, 0, 0, (int)cube_side.back];

                current_1 = solutionsAndRotations.colors_group[1, 0, 0, (int)cube_side.down];
                current_2 = solutionsAndRotations.colors_group[1, 0, 0, (int)cube_side.back];

                if ((good_1 == current_1) && (good_2 == current_2))
                {
                    n3 = 1;
                    count++;
                }
                //------------- IV
                good_1 = solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.down];
                good_2 = solutionsAndRotations.colors_group[0, 0, 2, (int)cube_side.front];

                current_1 = solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.down];
                current_2 = solutionsAndRotations.colors_group[1, 0, 2, (int)cube_side.front];

                if ((good_1 == current_1) && (good_2 == current_2))
                {
                    n4 = 1;
                    count++;
                }
                return count;
            }

            private void toQueue(general_dir dir, int index, Boolean clock_wise, Boolean forward)
            {
                if (QueueRotations.Count < 501)
                {
                    QueueRotations.Enqueue(new rotations((int)dir, index, clock_wise));
                    generalRotations.base_for_rotation_without_show(dir, index, clock_wise, true);
                }

            }

            private void putCornerToRightPlace(int rel_front, int rel_right, int rel_top)
            {
                int i_ = -2;
                int j_ = -2;
                int k_ = -2;


                findCorner(ref i_, ref  j_, ref k_, rel_front + rel_right + rel_top);
                if (i_ == 2 && j_ == 2 && k_ == 0)
                {

                    toQueue(general_dir.right_dir, 2, false, true);// R-1
                    toQueue(general_dir.right_dir, 2, false, true);// R-1
                    ////MessageBox.Show("331 goes down");


                }
                if (i_ == 0 && j_ == 2 && k_ == 2)
                {

                    toQueue(general_dir.front_dir, 2, true, true);// F
                    toQueue(general_dir.front_dir, 2, true, true);// F

                    ////MessageBox.Show("133 goes down");
                }
                if (i_ == 0 && j_ == 2 && k_ == 0)
                {

                    toQueue(general_dir.right_dir, 0, false, true);// L
                    toQueue(general_dir.front_dir, 2, true, true); // F

                    ////MessageBox.Show("diagonal");
                }
                if (i_ == 0 && j_ == 0 && k_ == 2)
                {

                    toQueue(general_dir.up_dir, 0, false, true);// D
                    ////MessageBox.Show("113 goes right");
                }
                if (i_ == 2 && j_ == 0 && k_ == 0)
                {

                    toQueue(general_dir.up_dir, 0, true, true);// D-1
                    ////MessageBox.Show("311 goes left");
                }
                if (i_ == 0 && j_ == 0 && k_ == 0)
                {
                    toQueue(general_dir.up_dir, 0, true, true);// D-1
                    toQueue(general_dir.up_dir, 0, true, true);// D-1
                    ////MessageBox.Show("111 goes 180 degree");
                }
                // --------I-------------
                // 313 front = front original color
                // 313 right = up original color
                // 313 down = right original color
                if ((solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.front] == rel_front) &&
                    (solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.right] == rel_top) &&
                    (solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.down] == rel_right))
                {
                    ////MessageBox.Show("I step is done");
                    toQueue(general_dir.right_dir, 0, true, true);//L-1
                    toQueue(general_dir.front_dir, 2, false, true);//F-1
                    toQueue(general_dir.right_dir, 0, false, true);//L
                }
                //------II----------------
                // 313 down = front original color
                // 313 right = right original color
                // 313 front = top original color
                if ((solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.front] == rel_top) &&
                    (solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.right] == rel_right) &&
                    (solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.down] == rel_front))
                {
                    toQueue(general_dir.up_dir, 0, true, true);//D-1

                    toQueue(general_dir.right_dir, 2, false, true);//R-1
                    toQueue(general_dir.up_dir, 0, false, true);//D
                    toQueue(general_dir.right_dir, 2, true, true);//R
                    ////MessageBox.Show("II step is done");
                }
                // ----------III -----
                // 313 down = top original color
                // 313 right = front original color
                // 313 front = right original color
                if ((solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.front] == rel_right) &&
                    (solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.right] == rel_front) &&
                    (solutionsAndRotations.colors_group[2, 0, 2, (int)cube_side.down] == rel_top))
                {
                    toQueue(general_dir.right_dir, 2, false, true);//R-1

                    toQueue(general_dir.up_dir, 0, false, true);//D
                    toQueue(general_dir.right_dir, 2, true, true);//R

                    toQueue(general_dir.front_dir, 2, true, true);//F
                    toQueue(general_dir.up_dir, 0, false, true);//D
                    toQueue(general_dir.up_dir, 0, false, true);//D

                    toQueue(general_dir.front_dir, 2, false, true);//F-1
                    ////MessageBox.Show("III step is done");

                }
                if (i_ == 2 && j_ == 2 && k_ == 2)
                {
                    //----------IV----------
                    // 333 up = front original color
                    // 333 right = up original color
                    // 333 front = right original color
                    // if ((colors_group[3, 3, 3, (int)cube_side.front] == rel_right) &&
                    //     (colors_group[3, 3, 3, (int)cube_side.right] == rel_top) &&
                    //    (colors_group[3, 3, 3, (int)cube_side.up] == rel_front))
                    {
                        toQueue(general_dir.right_dir, 2, false, true);//R-1
                        toQueue(general_dir.right_dir, 2, false, true);//R-1 // my own addition
                        toQueue(general_dir.up_dir, 0, true, true);//D-1
                        toQueue(general_dir.right_dir, 2, true, true);//R
                        toQueue(general_dir.up_dir, 0, false, true);//D
                        ////MessageBox.Show("IV step is done");

                    }
                }
            }
        }
        public class GeneralRotations
        {
            const int MilliSecForRotation = 20;
            private Viewport3D viewport;
            private SolutionsAndRotations solutionsAndRotation;
            private int N;
            
            private Transform3DGroup temp;
            
            private int count_rot;
            private int nAnimation = 6;

            private RotateTransform3D trans_rotation;

            private Storyboard timer;
            
            private general_dir DIR;
            private int INDEX;
            private Boolean CLOCK_WISE;
            private Boolean PLAY_FORWARD;

            public GeneralRotations(Viewport3D _viewport, int _N, SolutionsAndRotations _sol)
            {
                viewport = _viewport;
                N = _N;
                solutionsAndRotation = _sol;
            }

            public struct I
            {
                public int i_x, i_y, i_z;
                public I(int x, int y, int z)
                {
                    i_x = x;
                    i_y = y;
                    i_z = z;
                }
            }
            
            private void addRotationToHistory()
            {
                if (PLAY_FORWARD == true)
                    solutionsAndRotation.History.AddLast(new rotations((int)DIR, INDEX, CLOCK_WISE));
            }
            
            private int switchForColors(int t)
            {
                if (t == 1)
                    return 0;
                if (t == 2)
                    return 1;
                if (t == 4)
                    return 2;
                if (t == 8)
                    return 3;
                if (t == 16)
                    return 4;
                if (t == 32)
                    return 5;
                return -1;
            }
 
            private void mirrors()
            {
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        int t = solutionsAndRotation.colors_group[i, j, 0, (int)cube_side.back];
                        int n = 0;
                        if (t != solutionsAndRotation.colors_group_back[i, j])
                        {
                            solutionsAndRotation.colors_group_back[i, j] = t;
                            viewport.Children.Remove(solutionsAndRotation.model_back[i, j]);
                            n = switchForColors(t);

                            visualmodel_mirror vm = new visualmodel_mirror(i, j, -N - 0, n, N, (int)cube_side.front);
                            solutionsAndRotation.model_back[i, j] = vm.get_model();
                            viewport.Children.Add(solutionsAndRotation.model_back[i, j]);
                        }
                       
                    }
                //----------------
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        int t = solutionsAndRotation.colors_group[0, i, j, (int)cube_side.left];
                        int n = 0;
                        if (t != solutionsAndRotation.colors_group_left[i, j])
                        {
                            solutionsAndRotation.colors_group_left[i, j] = t;
                            viewport.Children.Remove(solutionsAndRotation.model_left[i, j]);
                            n = switchForColors(t);

                            visualmodel_mirror vm = new visualmodel_mirror(-N - 0, i, j, n, N, (int)cube_side.right);
                            solutionsAndRotation.model_left[i, j] = vm.get_model();
                            viewport.Children.Add(solutionsAndRotation.model_left[i, j]);
                        }

                    }
                //-----------
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        int t = solutionsAndRotation.colors_group[i, 0, j, (int)cube_side.down];
                        int n = 0;
                        if (t != solutionsAndRotation.colors_group_down[i, j])
                        {
                            solutionsAndRotation.colors_group_down[i, j] = t;
                            viewport.Children.Remove(solutionsAndRotation.model_down[i, j]);
                            n = switchForColors(t);

                            visualmodel_mirror vm = new visualmodel_mirror(i, -N - 0, j, n, N, (int)cube_side.up);
                            solutionsAndRotation.model_down[i, j] = vm.get_model();
                            viewport.Children.Add(solutionsAndRotation.model_down[i, j]);
                        }

                    }
            }
            private void mirrorsCopy()
            {
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        int t = solutionsAndRotation.colors_group_copy[i, j, 0, (int)cube_side.back];
                        int n = 0;
                        if (t != solutionsAndRotation.colors_group_back[i, j])
                        {
                            solutionsAndRotation.colors_group_back[i, j] = t;
                            viewport.Children.Remove(solutionsAndRotation.model_back[i, j]);
                            n = switchForColors(t);

                            visualmodel_mirror vm = new visualmodel_mirror(i, j, -N - 0, n, N, (int)cube_side.front);
                            solutionsAndRotation.model_back[i, j] = vm.get_model();
                            viewport.Children.Add(solutionsAndRotation.model_back[i, j]);
                        }

                    }
                //----------------
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        int t = solutionsAndRotation.colors_group_copy[0, i, j, (int)cube_side.left];
                        int n = 0;
                        if (t != solutionsAndRotation.colors_group_left[i, j])
                        {
                            solutionsAndRotation.colors_group_left[i, j] = t;
                            viewport.Children.Remove(solutionsAndRotation.model_left[i, j]);
                            n = switchForColors(t);

                            visualmodel_mirror vm = new visualmodel_mirror(-N - 0, i, j, n, N, (int)cube_side.right);
                            solutionsAndRotation.model_left[i, j] = vm.get_model();
                            viewport.Children.Add(solutionsAndRotation.model_left[i, j]);
                        }

                    }
                //-----------
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        int t = solutionsAndRotation.colors_group_copy[i, 0, j, (int)cube_side.down];
                        int n = 0;
                        if (t != solutionsAndRotation.colors_group_down[i, j])
                        {
                            solutionsAndRotation.colors_group_down[i, j] = t;
                            viewport.Children.Remove(solutionsAndRotation.model_down[i, j]);
                            n = switchForColors(t);

                            visualmodel_mirror vm = new visualmodel_mirror(i, -N - 0, j, n, N, (int)cube_side.up);
                            solutionsAndRotation.model_down[i, j] = vm.get_model();
                            viewport.Children.Add(solutionsAndRotation.model_down[i, j]);
                        }

                    }
            }
            public void base_for_rotation(general_dir dir, int index, Boolean clock_wise, Boolean play_forward)
            {
                INDEX = index;
                DIR = dir;
                CLOCK_WISE = clock_wise;
                PLAY_FORWARD = play_forward;

                addRotationToHistory();

                animatedRotation();

                rollByModel(ToDo.DoAll);

                switchModels(ToDo.DoAll);

                mirrors();
            }
            private void switchModels(ToDo TD)
            {
                I[] rot_corner = new I[4];
                I[,] G = new I[N, N];

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        switch (DIR)
                        {
                            case general_dir.front_dir:
                                G[i, j] = new I(i, j, INDEX);
                                break;

                            case general_dir.right_dir:
                                G[i, j] = new I(INDEX, i, j);
                                break;
                            case general_dir.up_dir:
                                G[i, j] = new I(j, INDEX, i);
                                break;
                        }
                    }
                }
                Boolean xxx = ((CLOCK_WISE == false && PLAY_FORWARD == true) ||
                              (CLOCK_WISE == true && PLAY_FORWARD == false));

                foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                {
                    for (int j = 0; j <= N / 1; j++)
                    {
                        for (int i = j; i < N - j - 1; i++)
                        {
                            if (!xxx)
                            {
                                rot_corner[0] = G[i, j];
                                rot_corner[1] = G[N - j - 1, i];
                                rot_corner[2] = G[N - i - 1, N - j - 1];
                                rot_corner[3] = G[j, N - i - 1];
                            }
                            else
                            {
                                rot_corner[0] = G[i, j];
                                rot_corner[1] = G[j, N - i - 1];
                                rot_corner[2] = G[N - i - 1, N - j - 1];
                                rot_corner[3] = G[N - j - 1, i];
                            }

                            moving(rot_corner, (int)c, TD);

                        }
                    }
                }
            }
            private void rollByModel(ToDo TD)
            {
                int x = 0;
                int y = 0;
                int z = 0;

                int side1 = 0;
                int side2 = 0;
                int side3 = 0;
                int side4 = 0;

                for (int i = 0; i < N; i++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        switch (DIR)
                        {
                            case general_dir.front_dir:
                                x = i;
                                y = k;
                                z = INDEX;

                                side1 = (int)cube_side.up;
                                side2 = (int)cube_side.right;
                                side3 = (int)cube_side.down;
                                side4 = (int)cube_side.left;
                                break;

                            case general_dir.up_dir:

                                x = i;
                                y = INDEX;
                                z = k;
                                side1 = (int)cube_side.front;
                                side2 = (int)cube_side.left;
                                side3 = (int)cube_side.back;
                                side4 = (int)cube_side.right;
                                break;

                            case general_dir.right_dir:

                                x = INDEX;
                                y = i;
                                z = k;
                                side1 = (int)cube_side.front;
                                side2 = (int)cube_side.up;
                                side3 = (int)cube_side.back;
                                side4 = (int)cube_side.down;
                                break;
                        }

                        if (CLOCK_WISE == true)
                            swap_ref2<int>(ref side2, ref side4);

                        if (PLAY_FORWARD == false)// play back
                        {
                            swap_ref2<int>(ref side1, ref side4);
                            swap_ref2<int>(ref side2, ref side3);
                        }
                        if (TD == ToDo.DoAll)
                        {
                            swap_ref4<int>(ref solutionsAndRotation.colors_group[x, y, z, side1],
                                                        ref solutionsAndRotation.colors_group[x, y, z, side2],
                                                        ref solutionsAndRotation.colors_group[x, y, z, side3],
                                                        ref solutionsAndRotation.colors_group[x, y, z, side4]);

                            swap_ref4<ModelVisual3D>(ref solutionsAndRotation.SideCubeModel[x, y, z, side1],
                                                        ref solutionsAndRotation.SideCubeModel[x, y, z, side2],
                                                        ref solutionsAndRotation.SideCubeModel[x, y, z, side3],
                                                        ref solutionsAndRotation.SideCubeModel[x, y, z, side4]);


                            swap_ref4<Transform3DGroup>(ref solutionsAndRotation.trans_group[x, y, z, side1],
                                                        ref solutionsAndRotation.trans_group[x, y, z, side2],
                                                        ref solutionsAndRotation.trans_group[x, y, z, side3],
                                                        ref solutionsAndRotation.trans_group[x, y, z, side4]);
                        }
                        //----------------------------------------------------------------
                        if (TD == ToDo.DoModelRotation)
                        {
                            swap_ref4<int>(ref solutionsAndRotation.colors_group[x, y, z, side1],
                                                        ref solutionsAndRotation.colors_group[x, y, z, side2],
                                                        ref solutionsAndRotation.colors_group[x, y, z, side3],
                                                        ref solutionsAndRotation.colors_group[x, y, z, side4]);

                            swap_ref4<ModelVisual3D>(ref solutionsAndRotation.SideCubeModel[x, y, z, side1],
                                                        ref solutionsAndRotation.SideCubeModel[x, y, z, side2],
                                                        ref solutionsAndRotation.SideCubeModel[x, y, z, side3],
                                                        ref solutionsAndRotation.SideCubeModel[x, y, z, side4]);



                        }
                        //-----------------------------------------------------------------
                        if (TD == ToDo.DoVisualRotation)
                        {
                            swap_ref4<Transform3DGroup>(ref solutionsAndRotation.trans_group[x, y, z, side1],
                                                        ref solutionsAndRotation.trans_group[x, y, z, side2],
                                                        ref solutionsAndRotation.trans_group[x, y, z, side3],
                                                        ref solutionsAndRotation.trans_group[x, y, z, side4]);

                            swap_ref4<int>(ref solutionsAndRotation.colors_group_copy[x, y, z, side1],
                                                       ref solutionsAndRotation.colors_group_copy[x, y, z, side2],
                                                       ref solutionsAndRotation.colors_group_copy[x, y, z, side3],
                                                       ref solutionsAndRotation.colors_group_copy[x, y, z, side4]);
                        }

                    }
                }
            }
            public void base_for_rotation_show(general_dir dir, int index, Boolean clock_wise, Boolean play_forward)
            {

                INDEX = index;
                DIR = dir;
                CLOCK_WISE = clock_wise;
                PLAY_FORWARD = play_forward;

                animatedRotation();

                rollByModel(ToDo.DoVisualRotation);

                switchModels(ToDo.DoVisualRotation);

                mirrorsCopy();
                
            }
            private void animatedRotation()
            {
                Quaternion quar = new Quaternion();

                int xx = 0;
                int yy = 0;
                int zz = 0;
                switch (DIR)
                {
                    case general_dir.front_dir:
                        zz = 1;
                        break;
                    case general_dir.right_dir:
                        xx = 1;
                        break;
                    case general_dir.up_dir:
                        yy = 1;
                        break;
                }
                if (CLOCK_WISE == true)
                {
                    xx *= -1;
                    yy *= -1;
                    zz *= -1;
                }
                if (PLAY_FORWARD)
                {
                    quar = new Quaternion(new Vector3D(xx, yy, zz), 90 / nAnimation);
                }
                else
                {
                    quar = new Quaternion(new Vector3D(xx, yy, zz), -90 / nAnimation);
                }

                QuaternionRotation3D trans_quaternion = new QuaternionRotation3D(quar);
                trans_rotation = new RotateTransform3D(trans_quaternion);

                count_rot = 0;
                timer = new Storyboard();
                timer.Duration = TimeSpan.FromMilliseconds(MilliSecForRotation);
                timer.Completed += new EventHandler(timer_Completed);
                timer.Begin();

            }

            private void moving(I[] RC, int c, ToDo TD)
            {
                if (TD == ToDo.DoAll)
                {
                    swap_ref4<ModelVisual3D>(ref solutionsAndRotation.SideCubeModel[RC[0].i_x, RC[0].i_y, RC[0].i_z, c],
                                             ref solutionsAndRotation.SideCubeModel[RC[1].i_x, RC[1].i_y, RC[1].i_z, c],
                                             ref solutionsAndRotation.SideCubeModel[RC[2].i_x, RC[2].i_y, RC[2].i_z, c],
                                             ref solutionsAndRotation.SideCubeModel[RC[3].i_x, RC[3].i_y, RC[3].i_z, c]);
                    swap_ref4<int>(ref solutionsAndRotation.colors_group[RC[0].i_x, RC[0].i_y, RC[0].i_z, c],
                                   ref solutionsAndRotation.colors_group[RC[1].i_x, RC[1].i_y, RC[1].i_z, c],
                                   ref solutionsAndRotation.colors_group[RC[2].i_x, RC[2].i_y, RC[2].i_z, c],
                                   ref solutionsAndRotation.colors_group[RC[3].i_x, RC[3].i_y, RC[3].i_z, c]);

                    swap_ref4<Transform3DGroup>(ref solutionsAndRotation.trans_group[RC[0].i_x, RC[0].i_y, RC[0].i_z, c],
                                                ref solutionsAndRotation.trans_group[RC[1].i_x, RC[1].i_y, RC[1].i_z, c],
                                                ref solutionsAndRotation.trans_group[RC[2].i_x, RC[2].i_y, RC[2].i_z, c],
                                                ref solutionsAndRotation.trans_group[RC[3].i_x, RC[3].i_y, RC[3].i_z, c]);
                }
                //---------------------------------------------------------------------
                if (TD == ToDo.DoModelRotation)
                {
                    swap_ref4<ModelVisual3D>(ref solutionsAndRotation.SideCubeModel[RC[0].i_x, RC[0].i_y, RC[0].i_z, c],
                                             ref solutionsAndRotation.SideCubeModel[RC[1].i_x, RC[1].i_y, RC[1].i_z, c],
                                             ref solutionsAndRotation.SideCubeModel[RC[2].i_x, RC[2].i_y, RC[2].i_z, c],
                                             ref solutionsAndRotation.SideCubeModel[RC[3].i_x, RC[3].i_y, RC[3].i_z, c]);
                    swap_ref4<int>(ref solutionsAndRotation.colors_group[RC[0].i_x, RC[0].i_y, RC[0].i_z, c],
                                   ref solutionsAndRotation.colors_group[RC[1].i_x, RC[1].i_y, RC[1].i_z, c],
                                   ref solutionsAndRotation.colors_group[RC[2].i_x, RC[2].i_y, RC[2].i_z, c],
                                   ref solutionsAndRotation.colors_group[RC[3].i_x, RC[3].i_y, RC[3].i_z, c]);


                }
                //---------------------------------------------------------------------

                if (TD == ToDo.DoVisualRotation)
                {
                    swap_ref4<Transform3DGroup>(ref solutionsAndRotation.trans_group[RC[0].i_x, RC[0].i_y, RC[0].i_z, c],
                                                ref solutionsAndRotation.trans_group[RC[1].i_x, RC[1].i_y, RC[1].i_z, c],
                                                ref solutionsAndRotation.trans_group[RC[2].i_x, RC[2].i_y, RC[2].i_z, c],
                                                ref solutionsAndRotation.trans_group[RC[3].i_x, RC[3].i_y, RC[3].i_z, c]);
                    swap_ref4<int>(ref solutionsAndRotation.colors_group_copy[RC[0].i_x, RC[0].i_y, RC[0].i_z, c],
                                   ref solutionsAndRotation.colors_group_copy[RC[1].i_x, RC[1].i_y, RC[1].i_z, c],
                                   ref solutionsAndRotation.colors_group_copy[RC[2].i_x, RC[2].i_y, RC[2].i_z, c],
                                   ref solutionsAndRotation.colors_group_copy[RC[3].i_x, RC[3].i_y, RC[3].i_z, c]);
                }
                //---------------------------------------------------------------------

            }

            private void RotationForAnimation(RotateTransform3D trans_rotation, general_dir dir, int index)
            {
                for (int i = 0; i < N; i++)
                    for (int k = 0; k < N; k++)
                    {
                        foreach (cube_side c in EnumHelper.GetValues<cube_side>())
                        {
                            switch (dir)
                            {

                                case general_dir.front_dir:
                                    temp = solutionsAndRotation.trans_group[i, k, index, (int)c];

                                    break;
                                case general_dir.right_dir:
                                    temp = solutionsAndRotation.trans_group[index, i, k, (int)c];

                                    break;
                                case general_dir.up_dir:
                                    temp = solutionsAndRotation.trans_group[i, index, k, (int)c];

                                    break;
                            }
                            if (temp != null)// avoidng rotation for cubes not on surface
                            {
                                temp.Children.Add(trans_rotation);

                            }
                        }
                    }
            }

            private void timer_Completed(object sender, EventArgs e)
            {
                RotationForAnimation(trans_rotation, DIR, INDEX);
                count_rot++;
                if (count_rot < nAnimation)
                {

                    timer.Begin();
                }
                else
                {
                    timer.Stop();

                    timer.Completed -= new EventHandler(timer_Completed);
                }
            }


            private void swap_ref4<T>(ref T front, ref T left, ref T back, ref T right)
            {
                T temp;

                temp = front;

                front = left;
                left = back;
                back = right;
                right = temp;
            }

            private void swap_ref2<T>(ref T first, ref T second)
            {
                T temp;

                temp = first;

                first = second;
                second = temp;

            }

            public void base_for_rotation_without_show(general_dir dir, int index, Boolean clock_wise, Boolean play_forward)
            {

                INDEX = index;
                DIR = dir;
                CLOCK_WISE = clock_wise;
                PLAY_FORWARD = play_forward;



                addRotationToHistory();

                rollByModel(ToDo.DoModelRotation);

                switchModels(ToDo.DoModelRotation);

                mirrors();
            }

        }

    }
}
