using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fyed.Kirafan {
    public static class RoomExtensions {
        public static Point ToRoomCoordination(this Point p, Point roomPoint) {
            return new Point(roomPoint.X + p.X, roomPoint.Y + p.Y);
        }
        public static IEnumerable<Point> ToRoomCoordination(this IEnumerable<Point> seq, Point roomPoint) {
            return seq.Select(p => p.ToRoomCoordination(roomPoint));
        }

        public static bool IsAcceptableWith(this Room room, params IRoomReviewer[] reviewers) {
            return reviewers.All(x => x.IsAcceptable(room));
        }
    }
}