using Fyed.Kirafan;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fyed.Kirafan.UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const string BUTTON_NAME_START = "開始";
        private const string BUTTON_NAME_STOP = "停止";
        private const string ITEM_DEF_FILE_NAME = "items.json";

        private CancellationTokenSource cts;
        private CancellationToken cancellationToken;
        private SynchronizationContext syncContext;
        private BlockingCollection<Room> roomBuffer;
        private Room halfwayRoom;
        private object halfwayRoomLock;
        private RoomMaster master;
        private ItemSelector.ItemSetup[] itemSetup;

        public MainWindow() {
            InitializeComponent();

            syncContext = SynchronizationContext.Current;
            var items = ReadItemDefinitions();
            itemSetup = items.Select(x => new ItemSelector.ItemSetup(x)).ToArray();
            halfwayRoomLock = new object();
        }

        private void StartStop(object sender, RoutedEventArgs e) {
            if (cts == null) {
                Start();
            }
            else {
                Stop();
            }
        }

        private void Start() {
            setItemsButton.IsEnabled = false;
            optionsPanel.IsEnabled = false;
            startStopButton.Content = BUTTON_NAME_STOP;
            roomBuffer = new BlockingCollection<Room>(10);
            var avoidAPDuplication = avoidAPDplicationCheckBox.IsChecked ?? false;
            var avoidDeadAP = avoidDeadAPCheckBox.IsChecked ?? false;
            var shuffleItems = shuffleItemsCheckBox.IsChecked ?? false;
            var checkReachability = checkReachabilityCheckBox.IsChecked ?? false;

            cts = new CancellationTokenSource();
            cancellationToken = cts.Token;
            var task = Task.Factory.StartNew(() => {
                FindCoordination(shuffleItems, avoidAPDuplication, avoidDeadAP, checkReachability);
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            task.ContinueWith(task => {
                cts.Dispose();
                cts = null;

                DisplayTaskError(task);

                setItemsButton.IsEnabled = true;
                startStopButton.IsEnabled = true;
                optionsPanel.IsEnabled = true;
                coordinatingLabel.Visibility = Visibility.Hidden;
                startStopButton.Content = BUTTON_NAME_START;
            }, TaskScheduler.FromCurrentSynchronizationContext());

            FindNext();
        }

        private void Stop() {
            startStopButton.IsEnabled = false;
            cts.Cancel();
        }

        private static void DisplayTaskError(Task task) {
            var error = task.Exception;
            if (error != null) {
                DisplayError(error);
            }
        }

        private static void DisplayError(Exception e) {
            if (e != null) {
                string messageText;
                var error = e as AggregateException;
                if (error != null) {
                    messageText = string.Join("----------------------------------------" + Environment.NewLine,
                    error.InnerExceptions.Select(x => x.ToString()));
                }
                else {
                    messageText = e.ToString();
                }
                var msgWindow = new MessageWindow(messageText);
                msgWindow.ShowDialog();
            }
        }

        private void FindCoordination(bool shuffleItems, bool avoidAPDuplication, bool avoidDeadAP, bool checkReachability) {
            halfwayRoom = null;
            var width = GetRoomWidth();
            var height = GetRoomHeight();
            var startRoom = new Room(width, height);
            var items = SetupItems(shuffleItems);
            var reviewers = SetupReviewers(avoidAPDuplication, avoidDeadAP, checkReachability);
            master = new RoomMaster();
            var task = master.FindRoomCoordinationAsync(startRoom, items, reviewers, coordinatedRoom => {
                roomBuffer.Add(coordinatedRoom, cancellationToken);
            }, halfwayRoom => {
                lock (halfwayRoomLock) {
                    this.halfwayRoom = halfwayRoom;
                }
            }, cancellationToken);
            task.ContinueWith(task => {
                DisplayTaskError(task);
                roomBuffer.CompleteAdding();
            });
            try {
                task.Wait();
            }
            catch (AggregateException e) {
                var errors = e.InnerExceptions;
                if (errors.Count == 1 && errors[0] is TaskCanceledException) {
                    Debug.WriteLine(e.ToString());
                }
            }
        }

        private IRoomReviewer[] SetupReviewers(bool avoidAPDuplication, bool avoidDeadAP, bool checkReachability) {
            var reviewers = new List<IRoomReviewer>();
            if (avoidAPDuplication || avoidDeadAP) {
                reviewers.Add(new ActionPointsReviewer(avoidAPDuplication, avoidDeadAP));
            }
            if (checkReachability) {
                reviewers.Add(new ReachabilityReviewer());
            }
            return reviewers.ToArray();
        }

        private RoomItem[] SetupItems(bool shuffleItems) {
            var id = 0;
            var items = itemSetup
                .Where(x => x.IsSelected)
                .Where(x => x.Count > 0)
                .SelectMany(x => Enumerable.Repeat(x, x.Count))
                .Select(x => new RoomItem(id++, x.Item.Name, (System.Drawing.Point[])(x.Item.Occupation.Clone())))
                .ToArray();

            if (shuffleItems) {
                items = ShuffleItems(items);
            }

            return items;
        }

        private RoomItem[] ShuffleItems(RoomItem[] items) {
            var random = new Random();
            return items
                .OrderBy(x => random.Next())
                .ToArray();
        }

        private void FindNext(object sender, RoutedEventArgs e) {
            FindNext();
        }

        private void FindNext() {
            coordinatingLabel.Visibility = Visibility.Visible;
            saveImageButton.IsEnabled = false;
            findNextButton.IsEnabled = false;
            FindNextAsync();
        }

        private void FindNextAsync() {
            var roomConsumeTask = Task.Factory.StartNew(() => {
                Room coordinatedRoom;
                while (true) {
                    if (roomBuffer.TryTake(out coordinatedRoom, 500)) {
                        syncContext.Send(_ => {
                            coordinatingLabel.Visibility = Visibility.Hidden;

                            DrawRoom(coordinatedRoom);

                            saveImageButton.IsEnabled = true;
                            findNextButton.IsEnabled = true;

                            if (roomBuffer.IsCompleted && roomBuffer.Count == 0) {
                                coordinatingLabel.Visibility = Visibility.Hidden;
                                findNextButton.IsEnabled = false;
                            }
                        }, null);
                        break;
                    }
                    else {
                        if (roomBuffer.IsCompleted) {
                            break;
                        }
                        Room halfwayRoom;
                        lock (halfwayRoomLock) {
                            halfwayRoom = this.halfwayRoom;
                        }

                        if (halfwayRoom != null) {
                            syncContext.Send(_ => {
                                DrawRoom(halfwayRoom);
                            }, null);
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void DrawRoom(Room coordinatedRoom) {
            var width = GetRoomWidth();
            var height = GetRoomHeight();
            var unitLength = GetUnitLength();
            roomCanvas.Children.Clear();
            roomCanvas.Width = width * unitLength;
            roomCanvas.Height = height * unitLength;

            var roomBorder = new System.Windows.Shapes.Rectangle();
            Canvas.SetLeft(roomBorder, 0);
            Canvas.SetTop(roomBorder, 0);
            roomBorder.Width = width * unitLength;
            roomBorder.Height = height * unitLength;
            roomBorder.Stroke = new SolidColorBrush(Colors.Black);
            roomBorder.StrokeThickness = 1;
            roomCanvas.Children.Add(roomBorder);

            var adjacentPoints = new[] {
                new System.Drawing.Point(0, -1),
                new System.Drawing.Point(-1, 0),
                new System.Drawing.Point(1, 0),
                new System.Drawing.Point(0, 1),
                new System.Drawing.Point(-1, -1),
                new System.Drawing.Point(1, -1),
                new System.Drawing.Point(-1, 1),
                new System.Drawing.Point(1, 1)
            };

            foreach (var dsp in coordinatedRoom.Dispositions) {
                var item = dsp.Item;
                var location = dsp.Location;
                var startPoint = new System.Drawing.Point(location.X * unitLength, location.Y * unitLength);
                var perimeterPoints = master.GetPerimeter(item, startPoint, unitLength).ToArray();
                
                var itemPolygon = new Polygon();
                foreach (var p in perimeterPoints) {
                    itemPolygon.Points.Add(new System.Windows.Point(p.X, p.Y));
                }
                itemPolygon.Stroke = new SolidColorBrush(Colors.Blue);
                itemPolygon.StrokeThickness = 1;
                itemPolygon.Fill = new SolidColorBrush(Colors.LightBlue);
                Canvas.SetZIndex(itemPolygon, 1);
                roomCanvas.Children.Add(itemPolygon);

                var itemNameText = new TextBlock();
                Canvas.SetLeft(itemNameText, perimeterPoints[0].X);
                Canvas.SetTop(itemNameText , perimeterPoints[0].Y);
                itemNameText.Width = perimeterPoints[3].X - perimeterPoints[0].X;
                itemNameText.Height = perimeterPoints[1].Y - perimeterPoints[0].Y;
                itemNameText.Text = item.Name;
                itemNameText.TextWrapping = TextWrapping.Wrap;
                itemNameText.FontSize = 15;
                Canvas.SetZIndex(itemNameText, 2);
                roomCanvas.Children.Add(itemNameText);

                var actionPointRect = new System.Windows.Shapes.Rectangle();
                Canvas.SetLeft(actionPointRect, startPoint.X);
                Canvas.SetTop(actionPointRect, startPoint.Y);
                actionPointRect.Width = unitLength;
                actionPointRect.Height = unitLength;
                Canvas.SetZIndex(actionPointRect, 0);
                var color = Colors.Orange;
                color.A = 50;
                actionPointRect.Fill = new SolidColorBrush(color);
                roomCanvas.Children.Add(actionPointRect);

                if (item.Occupation.All(p => (p.X != 0 || p.Y != 0))) {
                    var apCenter = new System.Drawing.Point(startPoint.X + unitLength / 2, startPoint.Y + unitLength / 2);
                    var adjacentPoint = adjacentPoints.FirstOrDefault(p => item.Occupation.Contains(p));
                    if (adjacentPoint != default(System.Drawing.Point)) {
                        var line = new Line();
                        line.X1 = startPoint.X + unitLength / 2;
                        line.Y1 = startPoint.Y + unitLength / 2;
                        line.X2 = startPoint.X + (adjacentPoint.X * unitLength) + unitLength / 2;
                        line.Y2 = startPoint.Y + (adjacentPoint.Y * unitLength) + unitLength / 2;
                        line.Stroke = new SolidColorBrush(Colors.Purple);
                        line.StrokeThickness = 1;
                        Canvas.SetZIndex(line, 3);
                        roomCanvas.Children.Add(line);

                        var anchor = new Ellipse();
                        var anchorRadius = 5;
                        anchor.Width = anchorRadius * 2;
                        anchor.Height = anchorRadius * 2;
                        anchor.Fill = new SolidColorBrush(Colors.Purple);
                        Canvas.SetLeft(anchor, line.X2 - anchorRadius);
                        Canvas.SetTop(anchor, line.Y2 - anchorRadius);
                        Canvas.SetZIndex(anchor, 3);
                        roomCanvas.Children.Add(anchor);
                        
                    }
                }
            }
        }

        private void SaveImage(object sender, RoutedEventArgs e) {
            try {
                var bounds = VisualTreeHelper.GetDescendantBounds(roomCanvas);

                var hwndSource = (HwndSource)HwndSource.FromVisual(roomCanvas);
                double dpiX = 96d;
                double dpiY = 96d;
                var targetBitmap = new RenderTargetBitmap(
                    (int)bounds.Width,
                    (int)bounds.Height,
                    dpiX,
                    dpiY,
                    PixelFormats.Default);
                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen()) {
                    var brush = new VisualBrush(roomCanvas);
                    dc.DrawRectangle(brush, null, new Rect(new System.Windows.Point(), bounds.Size));
                }

                targetBitmap.Render(dv);


                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(targetBitmap));

                var dialog = new SaveFileDialog();
                dialog.DefaultExt = ".png";
                dialog.Title = "ルーム画像保存";
                dialog.AddExtension = true;
                dialog.CheckPathExists = true;
                dialog.OverwritePrompt = true;
                dialog.ValidateNames = true;

                var dialogResult = dialog.ShowDialog();
                if (dialogResult == true) {
                    var fileName = dialog.FileName;
                    var dir = System.IO.Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }
                    using (var output = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                        encoder.Save(output);
                    }
                }
            }
            catch (Exception error) {
                DisplayError(error);
            }
        }

        private int GetRoomHeight() {
            // fixed room size, for now
            return 9;
        }

        private int GetRoomWidth() {
            // fixed room size, for now
            return 9;
        }

        private int GetUnitLength() {
            // fixed length, for now...
            return 80;
        }

        private void SetItems(object sender, RoutedEventArgs e) {
            var itemSelector = new ItemSelector();
            itemSelector.Items.AddRange(itemSetup);
            itemSelector.ShowDialog();
            itemSetup = itemSelector.Items.ToArray();
        }

        private RoomItem[] ReadItemDefinitions() {
            try {
                var exePath = Assembly.GetEntryAssembly().Location;
                var exeDir = System.IO.Path.GetDirectoryName(exePath);
                var itemDefFilePath = System.IO.Path.Join(exeDir, ITEM_DEF_FILE_NAME);
                if (File.Exists(itemDefFilePath)) {
                    var itemsJson = File.ReadAllText(itemDefFilePath, new UTF8Encoding());
                    var items = JsonSerializer.Deserialize<RoomItem[]>(itemsJson);
                    return items;
                }
                else {
                    return new RoomItem[0];
                }
            }
            catch (Exception e) {
                Debug.WriteLine(e.ToString());
                return new RoomItem[0];
            }

        }
    }
}
