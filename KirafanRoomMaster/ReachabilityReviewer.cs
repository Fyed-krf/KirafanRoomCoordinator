using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fyed.Kirafan {
    public class ReachabilityReviewer : IRoomReviewer {
        public bool IsAcceptable(Room room) {
            var uncheckedPoints = room.FreeFloors.ToList();

            if (uncheckedPoints.Count == 0) {
                return true;
            }

            var checkedPoints = new Queue<Point>();
            var p = uncheckedPoints[0];
            uncheckedPoints.Remove(p);
            checkedPoints.Enqueue(p);

            while (uncheckedPoints.Count > 0) {
                p = checkedPoints.Dequeue();

                var pUpper = new Point(p.X, p.Y - 1);
                if (uncheckedPoints.Remove(pUpper)) {
                    checkedPoints.Enqueue(pUpper);
                }

                var pLeft = new Point(p.X - 1, p.Y);
                if (uncheckedPoints.Remove(pLeft)) {
                    checkedPoints.Enqueue(pLeft);
                }

                var pRight = new Point(p.X + 1, p.Y);
                if (uncheckedPoints.Remove(pRight)) {
                    checkedPoints.Enqueue(pRight);
                }

                var pUnder = new Point(p.X, p.Y + 1);
                if (uncheckedPoints.Remove(pUnder)) {
                    checkedPoints.Enqueue(pUnder);
                }

                if (checkedPoints.Count == 0) {
                    return false;
                }
            }

            return true;
        }
    }
}