using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MinesweeperLib {

    public enum CellUserStatus : byte {
        Blank,
        Number1,
        Number2,
        Number3,
        Number4,
        Number5,
        Number6,
        Number7,
        Number8,
        Flag,
        Unopen,
        NoLoaded
    }

    public class Cell {
        public CellUserStatus Status = CellUserStatus.Unopen;
        public bool IsWrong = false;
        public bool IsMine = true;

        public CellTransport ToCellTransport() {
            return new CellTransport() { Status = (byte)this.Status, IsWrong = this.IsWrong };
        }
    }

    public class CellClient {
        public CellClient(CellTransport data) {
            this.Status = (CellUserStatus)data.Status;
            this.IsWrong = data.IsWrong;
        }

        public CellUserStatus Status = CellUserStatus.Unopen;
        public bool IsWrong = false;
    }

    public struct CellTransport {
        public byte Status;
        public bool IsWrong;
    }

    public struct Point {
        public Point(BigInteger x, BigInteger y) {
            X = x;
            Y = y;
        }

        public Point(string str) {
            var cache = str.Split(',');
            X = BigInteger.Parse(cache[0]);
            Y = BigInteger.Parse(cache[1]);
        }

        public static Point operator +(Point a, Point b) {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b) {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(Point a, Point b) {
            if (a.X == b.X && a.Y == b.Y) return true;
            else return false;
        }

        public static bool operator !=(Point a, Point b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if (obj is Point) {
                var cache = (Point)obj;
                if (this.X == cache.X && this.Y == cache.Y) return true;
                else return false;
            } else
                return false;
        }

        public override int GetHashCode() {
            return this.ToString().GetHashCode();
        }

        public BigInteger X;
        public BigInteger Y;

        public override string ToString() {
            return X.ToString() + "," + Y.ToString();
        }
    }


    public class MapChunk {

        public MapChunk(Point chunkPos, Cell[,] originalData, int chunckLength, bool isStrongLoading) {
            ChunkPosition = chunkPos;
            this.IsStrongLoading = isStrongLoading;
            if (originalData.GetLength(0) != chunckLength && originalData.GetLength(1) != chunckLength)
                throw new ArgumentException();
            data = originalData;
        }

        public Point ChunkPosition;
        public bool IsStrongLoading { get; set; }
        Cell[,] data;

        public Cell this[Point pos] {
            get {
                return data[int.Parse(pos.X.ToString()), int.Parse(pos.Y.ToString())];
            }
            set {
                data[int.Parse(pos.X.ToString()), int.Parse(pos.Y.ToString())] = value;
            }
        }
    }

}
