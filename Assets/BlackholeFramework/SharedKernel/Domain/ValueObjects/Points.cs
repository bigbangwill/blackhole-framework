namespace Blackhole.SharedKernel.Domain
{
    public struct Point2
    {
        public float X;
        public float Y;

        public Point2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Point2Int
    {
        public int X;
        public int Y;

        public Point2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"{X}, {Y}";
    }

    public struct Point3
    {
        public float X;
        public float Y;
        public float Z;

        public Point3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct Point3Int
    {
        public int X;
        public int Y;
        public int Z;

        public Point3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}