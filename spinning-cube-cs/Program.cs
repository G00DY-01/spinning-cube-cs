using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

public class Program
{
    public static void Main(string[] args)
    {
        string x = "";
        var positions = new List<Vec3>();
        while (x != "e")
        {
            Console.WriteLine("Enter a cube position as x,y,z or type 'e' to exit: ");
            x = Console.ReadLine();
            if (x == "e")
            {
                break;
            }
            var xyz = x.Split(',');
            if (xyz.Length != 3)
            {
                Console.WriteLine("Buddy, you aint breaking the system. Just do x,y,z");
                continue;
            }
            try
            {
                float posX = float.Parse(xyz[0]);
                float posY = float.Parse(xyz[1]);
                float posZ = float.Parse(xyz[2]);
                positions.Add(new Vec3(posX, posY, posZ));
            }
            catch
            {
                Console.WriteLine("The numbers arent numbering... input numbers please.");
            }
        }
        CubeRenderer.Start(100f, positions);
        
    }
}
