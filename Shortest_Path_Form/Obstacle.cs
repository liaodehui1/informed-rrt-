using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace Shortest_Path_Form
{
    public class Obstacle
    {
        public Point[] points { get; set; }
        public Obstacle(Point[] allPoints, Form1 form1)
        {
            points = new Point[allPoints.Length];
            for(int i=0;i< allPoints.Length;i++)
            {
                points[i] = new Point(allPoints[i].X, form1.boundaryYlimits- allPoints[i].Y);
            }
        }        
    }
}
