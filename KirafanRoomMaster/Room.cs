using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Fyed.Kirafan {
    /// <summary>
    /// ルームを表すクラス。
    /// 縦横の長さとルームアイテムの配置を保持する。
    /// </summary>
    public class Room {
        /// <summary>
        /// ルームの幅を取得する
        /// </summary>
        /// <value>ルームの幅</value>
        public int Width { get; private set; }
        /// <summary>
        /// ルームの高さを取得する
        /// </summary>
        /// <value>ルームの高さ</value>
        public int Height { get; private set; }
        /// <summary>
        /// ルームアイテムの配置状況を取得する
        /// </summary>
        /// <value>ルームアイテムの配置状況を表す<see cref="ItemDisposition">の配列 </value>
        public ItemDisposition[] Dispositions { get; private set; }
        public Point[] FreeFloors { get; private set; }
        public int?[,] FloorSummary { get; private set; }

        /// <summary>
        /// 幅と高さが0のルームを取得する
        /// </summary>
        /// <value>幅と高さが0のルーム</value>
        public static Room None {
            get {
                return new Room(0, 0);
            }
        }

        private Room(int width, int height, ItemDisposition[] dispositions, Point[] freeFloors, int?[,] floorSummary) {
            if (width < 0) {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height < 0) {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            var floorWidth = floorSummary.GetLength(0);
            if (width != floorWidth) {
                throw new ArgumentException($"width of room and free floor doesn't match");
            }
            var floorHeight = floorSummary.GetLength(1);
            if (height != floorHeight) {
                throw new ArgumentException($"height of room and free floor doesn't match");
            }

            Width = width;
            Height = height;
            Dispositions = dispositions;
            FreeFloors = freeFloors;
            FloorSummary = floorSummary;
        }

        public Room(int width, int height) : this(width, height, new ItemDisposition[0], new Point[width * height], new int?[width, height]) {
            for (var x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    FreeFloors[x * Height + y] = new Point(x, y);
                    FloorSummary[x, y] = null;
                }
            }
        }


        /// <summary>
        /// 現在のルームに新しいアイテムを配置できるかを確認し、配置できる場合には現在のルームに新しいアイテムを配置したルームを返却する。
        /// </summary>
        /// <param name="newDisposition">配置するアイテムの情報</param>
        /// <param name="addedRoom">アイテムが追加されたルーム</param>
        /// <returns>アイテムが追加された場合はtrue、追加できなかった場合はfalse</returns>
        /// <remarks>
        /// このメソッドがtrueを返却した場合にも現在のルームは変更されない。
        /// 現在のルームに新しいアイテムが追加されたルームが作成され、addedRoomに返却される。
        /// </remarks>
        public bool TryAddItem(ItemDisposition newDisposition, out Room addedRoom) {
            var addPoint = newDisposition.Location;
            var newItem = newDisposition.Item;
            
            var canAdd = checkAddItem(addPoint, newItem);
            if (canAdd == true) {
                var newWidth = Width;
                
                var newHeight = Height;

                var newDispositions = new ItemDisposition[Dispositions.Length + 1];
                Dispositions.CopyTo(newDispositions, 0);
                newDispositions[newDispositions.Length-1] = newDisposition;

                var occupationPoints = newItem.Occupation
                    .ToRoomCoordination(addPoint)
                    .ToArray();
                var newFreeFloors = FreeFloors
                    .Except(occupationPoints)
                    .ToArray();
                
                var newFloorSummary = (int?[,])FloorSummary.Clone();
                foreach (var p in occupationPoints) {
                    newFloorSummary[p.X, p.Y] = newItem.ID;
                }

                addedRoom = new Room(newWidth, newHeight, newDispositions, newFreeFloors, newFloorSummary);
                return true;
            }
            else {
                addedRoom = Room.None;
                return false;
            }
            
        }


        private bool checkAddItem(Point addPoint, RoomItem newItem) {
            return newItem.Occupation
                .ToRoomCoordination(addPoint)
                .All(p => FreeFloors.Contains(p));
        }

        public override string ToString() {
            int maxItemId;
            if (Dispositions.Length > 0) {
                maxItemId = Dispositions
                    .Select(x => x.Item.ID)
                    .Max();

            }
            else {
                maxItemId = 0;
            }
            var idLength = maxItemId.ToString().Length;

            var format = $"[{{0,{idLength}:d{idLength}}}]";
            var emptyFloor = string.Format("[{0}]", new string(' ', idLength));
            var actionPoint = string.Format("[{0}]", new string('#', idLength));

            var result = new StringBuilder();
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    string floor;
                    var id = FloorSummary[x, y];
                    if (id.HasValue) {
                        floor = string.Format(format, id.Value);
                    }
                    else {
                        var p = new Point(x, y);
                        if (Dispositions.Select(d => d.Location).Contains(p)) {
                            floor = actionPoint;
                        }
                        else {
                            floor = emptyFloor;
                        }
                    }
                    result.Append(floor);
                }
                result.Append(Environment.NewLine);
            }

            return result.ToString();
        }
    }
}