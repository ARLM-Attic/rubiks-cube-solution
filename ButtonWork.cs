// Rubik Cube 3D presentation and solution
// is developed by Halyna Shashyna
// Halyna.Shashyna@gmail.com
// ButtonWork.cs
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

namespace RubikCube
{
    public class ButtonWork
    {
        private Page page;
        private int N;
        
        private SolutionsAndRotations.SolutionFunctions solutions;

        private int firstRandomization;
        public int FirstRandomization
        {
            get
            {
                return firstRandomization;
            }
            set
            {
                firstRandomization = value;
            }
        }

        public ButtonWork(Page _page, int _N)
        {
            page = _page;
            page.dimen_N.IsEnabled = true;
            N = _N;
            InitFirstRandomization();
        }

        public void SolveAndRandomPosition(bool TrueOrFalse)
        {
            SetButtons("RandomPosition", TrueOrFalse);
            SetButtons("Solve", TrueOrFalse);
            SetButtons("TopCorners", TrueOrFalse);
        }
        public void ButtonsInit()
        {
            AllSolutionButtons(false);

            SetButtons("RandomPosition", true);

            page.RandomPosition.Content = "Random Position";
        }
        public void SetButtons(string Name, bool active)
        {
            Button B = (Button)page.FindName(Name);
            if (active)
            {

                B.Foreground = new SolidColorBrush(Color.FromArgb(255, 205, 89, 3));//Colors.Black);
                B.IsEnabled = true;
            }
            else
            {
                B.Foreground = new SolidColorBrush(Colors.LightGray);
                B.IsEnabled = false;
            }
        }
        public void AllSolutionButtons(bool TrueOrFalse)
        {


            SetButtons("TopCorners", TrueOrFalse);
            SetButtons("TopWings", TrueOrFalse);
            SetButtons("BottomCorners", TrueOrFalse);
            SetButtons("BottomWings", TrueOrFalse);
            SetButtons("middleSlice", TrueOrFalse);
            SetButtons("Solve", TrueOrFalse);
            
        }
        public void ButtonsResetsAndBack(bool TrueOrFalse)
        {
            SetButtons("button_animated_reset", TrueOrFalse);
            SetButtons("button_back", TrueOrFalse);
            SetButtons("button_reset", TrueOrFalse);
        }
        public void HideAllButtonsWhileRotating()
        {
            ButtonsResetsAndBack(false);
            AllSolutionButtons(false);
            SetButtons("RandomPosition", false);
        }
        public void ShowButtonsAfterRotating()
        {
            ButtonsResetsAndBack(true);
            page.dimen_N.IsEnabled = true;
            if (N == 3)
            {
                if ((solutions.CurrentSolutionStep == solutionStep.ALL) || (solutions.CurrentSolutionStep == solutionStep.V_step))
                {
                    page.RandomPosition.Content = "Random Position";
                    SetButtons("RandomPosition", true);
                    SetButtons("Solve", false);
                    SetButtons("TopCorners", false);

                    ButtonsResetsAndBack(false);

                    solutions.CurrentSolutionStep = solutionStep.NOTHINGsolutionStep;
                    return;
                }
                if (solutions.CurrentSolutionStep == solutionStep.NOTHINGsolutionStep)
                {
                    page.RandomPosition.Content = "Randomize More";
                    SolveAndRandomPosition(true);
                    ButtonsResetsAndBack(true);
                    return;
                }

                SetButtons("button_back", false);
                SetButtons("button_animated_reset", false);
                if (solutions.CurrentSolutionStep == solutionStep.I_step)
                {
                    SetButtons("TopWings", true);
                    return;

                }
                if (solutions.CurrentSolutionStep == solutionStep.II_step)
                {
                    SetButtons("BottomCorners", true);
                    return;

                }
                if (solutions.CurrentSolutionStep == solutionStep.III_step)
                {
                    SetButtons("BottomWings", true);
                    return;

                }
                if (solutions.CurrentSolutionStep == solutionStep.IV_step)
                {
                    SetButtons("middleSlice", true);
                    return;

                }
            }
            else
            {
                page.RandomPosition.Content = "Randomize More";
                SetButtons("RandomPosition", true);
                AllSolutionButtons(false);
            }
        }
        public void ActivateRandomButton()
        {
            firstRandomization++;
            if (firstRandomization > 0) page.RandomPosition.Content = "Randomize More";
            SetButtons("RandomPosition", true);
            if (firstRandomization > 1) 
                ShowButtonsAfterRotating();
        }
        public void DisableSomeButtons()
        {
            ButtonsResetsAndBack(false);
            
            
            InitFirstRandomization();

            SetButtons("RandomPosition", true);

        }
        public void trans(SolutionsAndRotations.SolutionFunctions _solutions)
        {
            solutions = _solutions;
        }
        public void InitFirstRandomization()
        {
            firstRandomization = 0;
            page.RandomPosition.Content = "Random Position";
        }
        public void IncRandomization()
        {
            firstRandomization++;
            if (firstRandomization > 0) page.RandomPosition.Content = "Randomize More";
        }
        
    }
}
