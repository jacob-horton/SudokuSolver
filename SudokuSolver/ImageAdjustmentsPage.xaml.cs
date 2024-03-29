﻿using Common;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ViewModels;
using ViewModels.Converters;

namespace SudokuSolver
{
    /// <summary>
    /// Interaction logic for ImageAdjustmentsPage.xaml
    /// </summary>
    public partial class ImageAdjustmentsPage : Page
    {
        private readonly SudokuImageViewModel viewModel;
        private readonly QuadViewModel quadViewModel;

        private readonly string sudokuPath;
        private readonly double circleRadius = 7.5f;
        private int selected = -1; // Keeps track of the current corner being dragged

        private readonly GridClassificationHelper gridClassificationHelper;

        public ImageAdjustmentsPage(string sudokuPath)
        {
            InitializeComponent();
            Focus(); // Focus this page so that it receives the KeyDown event

            gridClassificationHelper = new GridClassificationHelper();
            viewModel = (SudokuImageViewModel)DataContext;

            this.sudokuPath = sudokuPath; // Stores the image path in case the user wants to save the sudoku
            viewModel.Image = new Bitmap(sudokuPath);
            viewModel.Threshold = 0.5;

            // Guess the corners and set up the quad model
            Vector2D[] detectedCorners = gridClassificationHelper.DetectCorners(viewModel.Image);
            quadViewModel = new QuadViewModel(detectedCorners);

            // Add ellipses at the corners of the quad onto the canvas (clockwise)
            // TODO (CHECK): Does this work with a simple for loop?
            Ellipse[] corners = new Ellipse[4];
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    int index = j * 2 + i;
                    corners[index] = new Ellipse { Width = circleRadius * 2, Height = circleRadius * 2, Fill = new SolidColorBrush(new Colour(100, 255, 100, 200)) };
                    CvsImage.Children.Add(corners[index]);
                }
            }

            // Add bindings for the ellipses drawn at each corner
            IValueConverter converter = new CirclePositionCentreConverter(circleRadius);
            for (int i = 0; i < corners.Length; i++)
            {
                Binding xBinding = new Binding($"[{i}].X") { Source = quadViewModel, Mode = BindingMode.OneWay, Converter = converter };
                Binding yBinding = new Binding($"[{i}].Y") { Source = quadViewModel, Mode = BindingMode.OneWay, Converter = converter };
                corners[i].SetBinding(Canvas.LeftProperty, xBinding);
                corners[i].SetBinding(Canvas.TopProperty, yBinding);
            }

            CreateQuad();
        }

        /// <summary>
        /// Create the quad that is rendered on the canvas
        /// </summary>
        private void CreateQuad()
        {
            // Set all the bindings for each coordinate of each edge of the quad
            for (int i = 0; i < quadViewModel.Length; i++)
            {
                Line line = new Line { Stroke = new SolidColorBrush(Colors.Fuchsia) };

                line.SetBinding(Line.X1Property, new Binding($"[{i}].X") { Source = quadViewModel, Mode = BindingMode.OneWay });
                line.SetBinding(Line.Y1Property, new Binding($"[{i}].Y") { Source = quadViewModel, Mode = BindingMode.OneWay });
                line.SetBinding(Line.X2Property, new Binding($"[{(i + 1) % quadViewModel.Length}].X") { Source = quadViewModel, Mode = BindingMode.OneWay });
                line.SetBinding(Line.Y2Property, new Binding($"[{(i + 1) % quadViewModel.Length}].Y") { Source = quadViewModel, Mode = BindingMode.OneWay });

                CvsImage.Children.Add(line);
            }
        }

        /// <summary>
        /// Navigate to the next page and pass the sudoku grid and image path
        /// </summary>
        private void NavigateToGridPage()
        {
            Bitmap adjustedImage = viewModel.GetAdjustedImage(quadViewModel); // Get the grid cropped and with fixed perspective

            if (adjustedImage == null) // Could not get the adjusted image as the corners were invalid
            {
                MessageBox.Show("The corners form an invalid shape. Please ensure they match the image", "Error - Invalid Corners", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get all the classified digits for each cell
            int[,] sudoku = gridClassificationHelper.ClassifyGrid(adjustedImage);

            // Navigate to next page with the sudoku grid
            NavigationService.Navigate(new GridPage(sudoku, sudokuPath));
        }

        #region Events
        /// <summary>
        /// Update the greyscale based on the new threshold
        /// </summary>
        private void SldThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewModel.Greyscale();
        }

        /// <summary>
        /// Uses the corner detection to estimate where the corners are
        /// </summary>
        private void BtnEstimateCorners_Click(object sender, RoutedEventArgs e)
        {
            Vector2D[] corners = gridClassificationHelper.DetectCorners(viewModel.Image);
            for (int i = 0; i < corners.Count(); i++)
                quadViewModel[i] = corners[i];
        }

        /// <summary>
        /// Update the selected corner position to the mouse position
        /// </summary>
        private void CvsImage_MouseMoved(object sender, MouseEventArgs e)
        {
            // If there isn't a corner currently being dragged, return
            if (selected == -1) return;
            System.Windows.Point mousePos = e.GetPosition(CvsImage);

            // Set the corner to be the position of the mouse (clamped to the canvas bounds)
            quadViewModel[selected] = new Vector2D(
                Math.Max(Math.Min(mousePos.X, CvsImage.Width), 0),
                Math.Max(Math.Min(mousePos.Y, CvsImage.Height), 0)
            );
        }

        /// <summary>
        /// Loops through all the corners to see if the mouse is on them. If it is, it sets the currently selected corner to the one clicked on
        /// </summary>
        private void CvsImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the current mouse position
            System.Windows.Point mousePos = e.GetPosition(CvsImage);

            // Loop through all the corners
            for (int i = 0; i < quadViewModel.Length; i++)
            {
                double distSquaredFromPoint = (new Vector2D(mousePos) - quadViewModel[i]).LengthSquared();

                // Check if the mouse position is inside the circle (using length squared as it is more efficient than length)
                if (distSquaredFromPoint <= circleRadius * circleRadius)
                {
                    // If the mouse is inside the circle, set the currently selected corner to the index of the one clicked on
                    selected = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Reset selected corner
        /// </summary>
        private void CvsImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Reset selected so the corner will not continue to stick to the mouse
            selected = -1;
        }

        /// <summary>
        /// Go back to the welcome page
        /// </summary>
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }


        /// <summary>
        /// Go to the grid page if the next button is clicked
        /// </summary>
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            NavigateToGridPage();
        }

        /// <summary>
        /// Go to the grid page if enter is pressed
        /// </summary>
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) NavigateToGridPage();
        }

        #endregion
    }
}
