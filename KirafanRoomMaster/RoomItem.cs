using System.Drawing;
using System.Drawing.Drawing2D;

namespace Fyed.Kirafan {
    public class RoomItem {
        public int ID { get; set; }
        public string Name { get; set; }
        public Point[] Occupation { get; set; }

        public RoomItem() {
        }

        public RoomItem(int id, string name, params Point[] occupationPoints) {
            ID = id;
            Name = name;
            Occupation = occupationPoints;
        }

        public RoomItem Flip() {
            var newOccupation = (Point[])Occupation.Clone();
            var m = new Matrix();
            m.Rotate(90);
            m.TransformPoints(newOccupation);
            for (int i = 0; i < newOccupation.Length; i++) {
                newOccupation[i].X = -newOccupation[i].X;
            }
            return new RoomItem(ID, Name, newOccupation);
        }
    }
}