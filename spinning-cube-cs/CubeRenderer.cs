using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;


public struct Vec3
{
    public float x, y, z;

    public Vec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class CubeRenderer
{
    public static void Start(float size, List<Vec3> cubespos)
    {
        Console.CursorVisible = false;
        List<Vec3> verts = new List<Vec3>();
        for (int i = 0; i < cubespos.Count; i++)
        {
            var cubevert = new List<Vec3>();
            cubevert.Add(new Vec3(cubespos[i].x+0.5f, cubespos[i].y+0.5f, cubespos[i].z-0.5f));
            cubevert.Add(new Vec3(cubespos[i].x-0.5f, cubespos[i].y+0.5f, cubespos[i].z-0.5f));
            cubevert.Add(new Vec3(cubespos[i].x-0.5f, cubespos[i].y-0.5f, cubespos[i].z-0.5f));
            cubevert.Add(new Vec3(cubespos[i].x+0.5f, cubespos[i].y-0.5f, cubespos[i].z-0.5f));
            cubevert.Add(new Vec3(cubespos[i].x+0.5f, cubespos[i].y-0.5f, cubespos[i].z+0.5f));
            cubevert.Add(new Vec3(cubespos[i].x+0.5f, cubespos[i].y+0.5f, cubespos[i].z+0.5f));
            cubevert.Add(new Vec3(cubespos[i].x-0.5f, cubespos[i].y+0.5f, cubespos[i].z+0.5f));
            cubevert.Add(new Vec3(cubespos[i].x-0.5f, cubespos[i].y-0.5f, cubespos[i].z+0.5f));
            for (int j = 0; j < cubevert.Count; j++)
            {
                if (!verts.Contains(cubevert[j]))
                {
                    verts.Add(cubevert[j]);
                }
            }

        } 
        var edges = new List<(int, int)>();
        for (int i = 0; i < verts.Count; i++)
        {
            for (int j = i+1; j < verts.Count; j++)
            {
                var a = verts[i];
                var b = verts[j];

                float distx = a.x-b.x;
                float disty = a.y-b.y;
                float distz = a.z-b.z;
                float dist = distx*distx + disty*disty + distz*distz;
                if (dist == 1f)
                {
                    edges.Add((i, j));
                }

            }
        }

        float totalAngle = 0f;
        while (true)
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            char[,] buffer = new char[height, width];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    buffer[y, x] = ' ';

            var projectedPoints = new List<(int x, int y)>();

            foreach (var v in verts)
            {
                var ry = RotateY(v, totalAngle);
                var rz = RotateZ(ry, totalAngle);
                var (u, v2) = Project(rz);

                if (ConvertToScreen(u, v2, width, height, size, out int sx, out int sy))
                    projectedPoints.Add((sx, sy));
                else
                    projectedPoints.Add((-1, -1));
            }

            foreach (var (a, b) in edges)
            {
                var p0 = projectedPoints[a];
                var p1 = projectedPoints[b];

                if (p0.x >= 0 && p1.x >= 0)
                    DrawLine(p0.x, p0.y, p1.x, p1.y, buffer);
            }

            Console.SetCursorPosition(0, 0);

            var sb = new StringBuilder();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    sb.Append(buffer[y, x]);

                if (y < height - 1)
                    sb.Append('\n');
            }


            Console.Write(sb.ToString());

            Thread.Sleep(16);
            totalAngle += 0.05f;
        }
    }

    static Vec3 RotateZ(Vec3 v, float a)
    {
        float c = MathF.Cos(a);
        float s = MathF.Sin(a);

        return new Vec3(
            v.x * c - v.y * s,
            v.x * s + v.y * c,
            v.z
        );
    }

    static Vec3 RotateY(Vec3 v, float a)
    {
        float c = MathF.Cos(a);
        float s = MathF.Sin(a);

        return new Vec3(
            v.x * c - v.z * s,
            v.y,
            v.x * s + v.z * c
        );
    }

    static (float, float) Project(Vec3 v)
    {
        float z = v.z + 5f;
        float u = v.x / z;
        float v2 = (v.y / z) * 0.5f;
        return (u, v2);
    }

    static bool ConvertToScreen(float x, float y, int w, int h, float size, out int sx, out int sy)
    {
        x *= size;
        y *= size;

        x += w / 2f;
        y += h / 2f;

        sx = (int)x;
        sy = (int)y;

        return sx > 0 && sy > 0 && sx < w && sy < h;
    }

    static void DrawLine(int x0, int y0, int x1, int y1, char[,] buffer)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int w = buffer.GetLength(1);
        int h = buffer.GetLength(0);

        while (true)
        {
            if (x0 >= 0 && y0 >= 0 && x0 < w && y0 < h)
                buffer[y0, x0] = '#';

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = err * 2;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 <  dx) { err += dx; y0 += sy; }
        }
    }
}
