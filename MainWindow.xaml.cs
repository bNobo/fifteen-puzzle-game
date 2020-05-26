using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Taquin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] initialCases;
        private double ratio;

        public MainWindow()
        {
            InitializeComponent();

            string[] cases = { "A", "B", "C", "D", "E", "F", "G", "H" };
            initialCases = (string[])cases.Clone();
            // todo: faire une méthode de mélange qui donne forcément une grille soluble
            //Shuffle(cases);
            Random r = new Random();
            int skipIndex = 8;//r.Next(8);
            int caseIndex = 0;

            for (int i = 0; i < 9; i++)
            {
                if (skipIndex == i)
                {
                    continue;
                }

                var row = i / 3;
                var column = i % 3;
                var button = new Button();
                Image imagePortion = new Image { Source = GetImagePortion(row, column) };
                imagePortion.Stretch = Stretch.Fill;
                button.Name = "button" + i;
                button.SetValue(Grid.RowProperty, row);
                button.SetValue(Grid.ColumnProperty, column);
                button.Content = imagePortion;
                button.Tag = cases[caseIndex++];
                button.Margin = new Thickness();
                button.BorderThickness = new Thickness();
                button.Click += Button_Click;
                uxGrid.Children.Add(button);
                this.RegisterName(button.Name, button); 
            }

            SizeChanged += MainWindow_SizeChanged;
        }

        private ImageSource GetImagePortion(int row, int column)
        {
            BitmapImage image = new BitmapImage(new Uri("lapin.jpg", UriKind.Relative));
            image.CacheOption = BitmapCacheOption.OnLoad;
            var croppedWidth = (int)(1920 / 3);
            var croppedHeight = (int)(1080 / 3);
            var croppedBitmap = new CroppedBitmap(image, new Int32Rect { X = column * croppedWidth, Y = row * croppedHeight, Height = croppedHeight, Width = croppedWidth });
            return croppedBitmap;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (ratio == 0)
            //{
            //    ratio = ActualWidth / ActualHeight;
            //}

            //if (e.WidthChanged)
            //{
            //    var width = (int)(e.NewSize.Width / 3) * 3;
            //    var height = width / ratio;
            //    SizeChanged -= MainWindow_SizeChanged;
            //    Width = width;
            //    Height = height;
            //    SizeChanged += MainWindow_SizeChanged;
            //}
            //else if (e.HeightChanged)
            //{
            //    e.Handled = true;
            //    //var height = (int)(e.NewSize.Height / 3) * 3;
            //    //var width = ratio * height;
            //    //SizeChanged -= MainWindow_SizeChanged;
            //    //Width = width;
            //    //Height = height;
            //    //SizeChanged += MainWindow_SizeChanged;
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ResizeMode != ResizeMode.CanMinimize)
            {
                ResizeMode = ResizeMode.CanMinimize;
            }

            var button = sender as Button;
            var row = (int)button.GetValue(Grid.RowProperty);
            var column = (int)button.GetValue(Grid.ColumnProperty);

            if (CanMoveLeft(row, column))
            {
                Console.WriteLine("CanMoveLeft");
                Move(button, row, --column, Direction.Left);
            }
            else if (CanMoveTop(row, column))
            {
                Console.WriteLine("CanMoveTop");
                Move(button, --row, column, Direction.Top);
            }
            else if (CanMoveRight(row, column))
            {
                Console.WriteLine("CanMoveRight");
                Move(button, row, ++column, Direction.Right);
            }
            else if (CanMoveBottom(row, column))
            {
                Console.WriteLine("CanMoveBottom");
                Move(button, ++row, column, Direction.Bottom);
            }

            if (IsGameOver())
            {
                MessageBox.Show("Bravo, vous avez gagné !");
            }
        }

        private bool IsGameOver()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == 2 && j == 2)
                    {
                        break;
                    }

                    var value = GetCellValue(i, j);
                    if (value != initialCases[i * 3 + j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void Move(Button button, int row, int column, Direction direction)
        {
            Thickness margin = default(Thickness);

            switch (direction)
            {
                case Direction.Left:
                    margin = new Thickness(0, 0, -button.ActualWidth, 0);                    
                    break;
                case Direction.Top:
                    margin = new Thickness(0, 0, 0, -button.ActualHeight);
                    break;
                case Direction.Right:
                    margin = new Thickness(-button.ActualWidth, 0, 0, 0);
                    break;
                case Direction.Bottom:
                    margin = new Thickness(0, -button.ActualHeight, 0, 0);                    
                    break;
                default:
                    break;
            }

            // Surtout pas affecter de Margin sinon l'animation ne se joue qu'une seule fois
            //button.Margin = margin;
            // Pour que le bouton conserve sa taille pendant l'animation
            button.MaxWidth = button.ActualWidth;
            button.MinWidth = button.ActualWidth;
            button.MaxHeight = button.ActualHeight;
            button.MinHeight = button.ActualHeight;
            button.SetValue(Grid.RowProperty, row);
            button.SetValue(Grid.ColumnProperty, column);

            ThicknessAnimation thicknessAnimation = new ThicknessAnimation(margin, new Thickness(), new Duration(TimeSpan.FromMilliseconds(180)));
            button.BeginAnimation(MarginProperty, thicknessAnimation);
        }

        private bool CanMoveBottom(int row, int column)
        {
            if (row == 2)
            {
                return false;
            }
            else
            {
                return IsCellFree(++row, column);
            }
        }

        private bool CanMoveRight(int row, int column)
        {
            if (column == 2)
            {
                return false;
            }
            else
            {
                return IsCellFree(row, ++column);
            }
        }

        private bool CanMoveTop(int row, int column)
        {
            if (row == 0)
            {
                return false;
            }
            else
            {
                return IsCellFree(--row, column);
            }
        }

        private bool CanMoveLeft(int row, int column)
        {
            if (column == 0)
            {
                return false;
            }
            else
            {
                return IsCellFree(row, --column);
            }
        }

        private bool IsCellFree(int row, int column)
        {
            foreach (Button item in uxGrid.Children.OfType<Button>())
            {
                var currentRow = (int)item.GetValue(Grid.RowProperty);
                var currentColumn = (int)item.GetValue(Grid.ColumnProperty);

                if (currentRow == row && currentColumn == column)
                {
                    return false;
                }
            }

            return true;
        }

        private string GetCellValue(int row, int column)
        {
            foreach (Button item in uxGrid.Children.OfType<Button>())
            {
                var currentRow = (int)item.GetValue(Grid.RowProperty);
                var currentColumn = (int)item.GetValue(Grid.ColumnProperty);

                if (currentRow == row && currentColumn == column)
                {
                    return item.Tag as string;
                }
            }

            return null;
        }

        private void Shuffle(string[] cases)
        {
            Random r = new Random();

            for (int i = 0; i < 10; i++)
            {
                int i1 = r.Next(7);
                int i2 = r.Next(7);
                string buf = cases[i1];
                cases[i1] = cases[i2];
                cases[i2] = buf;
            }
        }
    }
}
