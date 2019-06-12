using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shortest_Path_Form
{
    public partial class Form1 : Form
    {
        private Point startPoint{ get; set; }
        public  Node startNode { get; set; }
        private Point goalPoint{ get; set; }
        public Node goalNode { get; set; }
        private Boolean moveIn = false;
        private int num { get; set; }
        public int pointRadius { get; set; }
        public PictureBox myPictureBox { get; set; }
        public Bitmap myBitmap { get; set; }
        public List<Obstacle> obstacles { get; set; }
        public int boundaryXlimits { get; set; }
        public int boundaryYlimits { get; set; }
        private int flag1 { get; set; }
        private int flag2 { get; set; }
        public Graphics g { get; set; }
        public Form1()
        {
            InitializeComponent();
            myPictureBox = this.pictureBox1;
            boundaryXlimits = pictureBox1.Width;
            boundaryYlimits = pictureBox1.Height;
            Console.WriteLine(boundaryXlimits + " " + boundaryYlimits);
            myBitmap = new Bitmap(boundaryXlimits,boundaryYlimits);
            pictureBox1.Image = myBitmap;
            num = 20;
            pointRadius = 7;
            flag1 = 0;
            flag2 = 0;
            g = Graphics.FromImage(myBitmap);
        }
        //画障碍物
        private void painting(object sender, EventArgs e)
        {
            //Graphics g = Graphics.FromImage(myBitmap);
            g.Clear(Color.White);//清空绘图区

            obstacles = new List<Obstacle>();

            //Point[] points = new Point[12];
            //points[0] = new Point(400, 270);
            //points[1] = new Point(400, 410);
            //points[2] = new Point(90, 410);
            //points[3] = new Point(90, 100);
            //points[4] = new Point(400, 100);
            //points[5] = new Point(400, 240);
            //points[6] = new Point(390, 240);
            //points[7] = new Point(390, 110);
            //points[8] = new Point(100, 110);
            //points[9] = new Point(100, 400);
            //points[10] = new Point(390, 400);
            //points[11] = new Point(390, 270);
            //Obstacle obstacle = new Obstacle(points, this);
            //obstacles.Add(obstacle);
            //g.FillPolygon(new SolidBrush(Color.Black), points);

            for (int i = 0; i < num; i++)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                Point[] points = new Point[4];
                int orpointx = random.Next(10, boundaryXlimits - 100);
                int orpointy = random.Next(10, boundaryYlimits - 100);
                points[0] = new Point(orpointx, orpointy);
                points[1] = new Point(orpointx + random.Next(30, 60), orpointy);
                points[2] = new Point(orpointx + random.Next(30, 60), orpointy + random.Next(30, 60));
                points[3] = new Point(orpointx, orpointy + random.Next(30, 60));
                Obstacle obstacle = new Obstacle(points, this);
                obstacles.Add(obstacle);
                g.FillPolygon(new SolidBrush(Color.Black), points);
            }

            //Point[] points = new Point[4];
            //points[0] = new Point(30, 30);
            //points[1] = new Point(230, 30);
            //points[2] = new Point(230, 230);
            //points[3] = new Point(30, 230);
            //Obstacle obstacle = new Obstacle(points, this);
            //obstacles.Add(obstacle);
            //g.FillPolygon(new SolidBrush(Color.Black), points);
            pictureBox1.Image = myBitmap;
        }
        private void paintStartPoint(object sender, EventArgs e)
        {
            flag1 ++;
        }
        private void paintEndPoint(object sender, EventArgs e)
        {
            flag2 ++;
        }
        private void startRunner(object sender, EventArgs e)
        {
            InformedRRTStar informedRRTStar = new InformedRRTStar(this);
            informedRRTStar.runPlaner();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && moveIn && (flag1==1) || (flag2==1))
            {
                //Graphics g = Graphics.FromImage(myBitmap);
                if (flag1==1)//画起点
                {
                    startPoint = this.pictureBox1.PointToClient(MousePosition);//获取鼠标点的位置为相对于控件pictureBox1
                    Color color=myBitmap.GetPixel(startPoint.X, startPoint.Y);
                    if(!color.Name.Equals("ff000000"))
                    {
                        g.FillEllipse(new SolidBrush(Color.Red), startPoint.X, startPoint.Y, pointRadius + 10, pointRadius + 10);
                        pictureBox1.Image = myBitmap;
                        startNode = new Node(startPoint.X, boundaryYlimits - startPoint.Y);
                        Console.WriteLine("startNode " + startNode.X + " " + startNode.Y);
                        flag1++;
                    }
                    else
                    {
                        MessageBox.Show("起点在障碍物");
                        flag1 = 0;
                    }
                }
                if (flag2==1)//画终点
                {
                    goalPoint = this.pictureBox1.PointToClient(MousePosition);
                    Color color = myBitmap.GetPixel(goalPoint.X, goalPoint.Y);
                    if (!color.Name.Equals("ff000000"))
                    {
                        g.FillEllipse(new SolidBrush(Color.Blue), goalPoint.X, goalPoint.Y, pointRadius + 10, pointRadius + 10);
                        pictureBox1.Image = myBitmap;
                        goalNode = new Node(goalPoint.X, boundaryYlimits - goalPoint.Y);
                        Console.WriteLine("goalNode " + goalPoint.X + " " + goalNode.Y);
                        flag2++;
                    }
                    else
                    {
                        MessageBox.Show("终点在障碍物");
                        flag2 = 0;
                    }
                }
            }
        }
        
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            moveIn = true;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            moveIn = false;
        }

        private void resetting(object sender, EventArgs e)
        {
            g = Graphics.FromImage(myBitmap);
            g.Clear(Color.White);//清空绘图区
            myPictureBox.Image = myBitmap;
            myPictureBox.Refresh();
            flag1 = 0;
            flag2 = 0;
            startNode = new Node();
            goalNode = new Node();
            obstacles.Clear();
        }
    }
}
