using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Gma.System.MouseKeyHook;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;

namespace ColorPicker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int PreviewWidth = 15;
        private const int PreviewHeight = 15;

        private const int UpdatesPerSecond = 30;

        private readonly CaptureSource _captureSource = new CaptureSource(PreviewWidth, PreviewHeight);

        private readonly DispatcherTimer _updateTimer = new DispatcherTimer {
            Interval = TimeSpan.FromMilliseconds(1000.0 / UpdatesPerSecond),
        };

        private readonly IKeyboardMouseEvents _globalHook = Hook.GlobalEvents();

        private Point _currentPosition;

        private BitmapSource _previewBitmap;

        private Color _selectedColor = Colors.Black;
        private string _selectedColorHex = "#000000";
        private string _selectedColorRgb = "RGB(0, 0, 0)";

        private Color _foregroundColor = Colors.White;

        private bool _colorSelectionActive = true;
        private bool _selectionKeyDown = false;

        public Point CurrentPosition
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;
                RaisePropertyChanged();
            }
        }

        public BitmapSource PreviewBitmap
        {
            get => _previewBitmap;
            set
            {
                _previewBitmap = value;
                RaisePropertyChanged();
            }
        }

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                RaisePropertyChanged();
            }
        }

        public string SelectedColorHex
        {
            get => _selectedColorHex;
            set
            {
                _selectedColorHex = value;
                RaisePropertyChanged();
            }
        }

        public string SelectedColorRgb
        {
            get => _selectedColorRgb;
            set
            {
                _selectedColorRgb = value;
                RaisePropertyChanged();
            }
        }

        public Color ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;
                RaisePropertyChanged();
            }
        }

        public bool ColorSelectionActive
        {
            get => _colorSelectionActive;
            set
            {
                _colorSelectionActive = value;
                RaisePropertyChanged();
            }
        }

        public bool SelectionKeyDown
        {
            get => _selectionKeyDown;
            set
            {
                _selectionKeyDown = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (sender, e) => SystemCommands.CloseWindow(this)));

            _updateTimer.Tick += _updateTimer_Tick;

            StartColorSelection();
        }

        private void StartColorSelection()
        {
            _globalHook.MouseDownExt += _globalHook_MouseDownExt;
            _globalHook.KeyDown += _globalHook_KeyDown;
            _globalHook.KeyUp += _globalHook_KeyUp;

            ColorSelectionActive = true;
            SelectionKeyDown = false;

            if (!IsMouseOver)
                _updateTimer.Start();
        }

        private void StopColorSelection()
        {
            _updateTimer.Stop();

            _globalHook.MouseDownExt -= _globalHook_MouseDownExt;
            _globalHook.KeyDown -= _globalHook_KeyDown;
            _globalHook.KeyUp -= _globalHook_KeyUp;

            ColorSelectionActive = false;
            SelectionKeyDown = false;
        }

        private void UpdateColor(int x, int y)
        {
            (BitmapSource previewBitmap, Color selectedColor) captureResult = _captureSource.CaptureAtPosition(x, y);
            PreviewBitmap = captureResult.previewBitmap;
            SelectedColor = captureResult.selectedColor;

            SelectedColorHex = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
            SelectedColorRgb = $"RGB({SelectedColor.R}, {SelectedColor.G}, {SelectedColor.B})";

            int brightness = (int)Math.Sqrt(SelectedColor.R * SelectedColor.R * 0.299 +
                                            SelectedColor.G * SelectedColor.G * 0.587 +
                                            SelectedColor.B * SelectedColor.B * 0.114);

            ForegroundColor = brightness > 128 ? Colors.Black : Colors.White;

            CurrentPosition = new Point(x, y);
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void _updateTimer_Tick(object sender, EventArgs e)
        {
            Point mousePosition = System.Windows.Forms.Cursor.Position;

            UpdateColor(mousePosition.X, mousePosition.Y);
        }

        private void _globalHook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (!ColorSelectionActive || !SelectionKeyDown)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            StopColorSelection();
            UpdateColor(e.X, e.Y);

            Focus();

            e.Handled = true;
        }

        private void _globalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LMenu)
                SelectionKeyDown = true;
        }

        private void _globalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LMenu)
                SelectionKeyDown = false;
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            DragMove();
        }

        private void MainWindow_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _updateTimer.Stop();
        }

        private void MainWindow_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ColorSelectionActive)
                _updateTimer.Start();
        }

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ColorSelectionActive)
                return;

            int newX = CurrentPosition.X;
            int newY = CurrentPosition.Y;

            if (e.Key == Key.Left)
                newX--;
            else if (e.Key == Key.Right)
                newX++;
            else if (e.Key == Key.Up)
                newY--;
            else if (e.Key == Key.Down)
                newY++;
            else
                return;

            UpdateColor(newX, newY);
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;

            Screen currentScreen = Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            Left = currentScreen.Bounds.Left;
            Top = currentScreen.Bounds.Top;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            StopColorSelection();

            _updateTimer.Stop();
            _captureSource.Dispose();
            _globalHook.Dispose();
        }

        private void StartSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            StartColorSelection();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}