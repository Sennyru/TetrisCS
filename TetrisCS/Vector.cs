namespace TetrisCS
{
    /// <summary> (x, y) 좌표를 저장할 수 있는 구조체 </summary>
    public struct Vector
    {
        public int x, y;

        public Vector(int x, int y) { this.x = x; this.y = y; }

        /// <summary> new Vector(1, 0) </summary>
        public static Vector Right => new(1, 0);
        /// <summary> new Vector(-1, 0) </summary>
        public static Vector Left => new(-1, 0);
        /// <summary> new Vector(0, 1) <br/> 좌표계가 다르므로 유의. </summary>
        public static Vector Down => new(0, 1);

        public static Vector operator +(Vector a, Vector b) => new Vector(a.x + b.x, a.y + b.y);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.x - b.x, a.y - b.y);
        public static bool operator ==(Vector a, Vector b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vector a, Vector b) => !(a == b);
        public override bool Equals(object? obj) => this == obj as Vector?; // ???
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"({x}, {y})";
    }
}
