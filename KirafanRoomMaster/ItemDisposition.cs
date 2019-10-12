using System.Drawing;

namespace Fyed.Kirafan {
    public class ItemDisposition {
        public Point Location { get; private set; }
        public RoomItem Item { get; private set; }

        public ItemDisposition(Point location, RoomItem item) {
            Location = location;
            Item = item;
        }
    }
}