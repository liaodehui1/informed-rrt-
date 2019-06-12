using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace Shortest_Path_Form
{
    class InformedRRTStar
    {
        public int maxIterations { get; set; }//最大迭代次数
        public double driveParameter { get; set; }//默认点与点间长度
        public double rewireRange { get; set; }//默认圆的半径（查找潜在父节点）
        public static bool reachedGoal { get; set; }//是否抵达终点
        public List<Node> Tree { get; set; }//树
        Form1 form1;//Form1实例
        Graphics g{ get; set; }//绘图区
        Node startNode { get; set; }//起点
        Node goalNode { get; set; }//终点
        public List<Obstacle> obstacles;//障碍物列表
        public double c_min { get; set; }//椭圆焦距
        public double c_max { get; set; }//椭圆长轴
        public double cos { set; get; }
        public double sin { get; set; }
        public InformedRRTStar(Form1 form1)
        {
            this.form1 = form1;
            startNode = form1.startNode;
            goalNode = form1.goalNode;
            obstacles = form1.obstacles;
            Tree = new List<Node>();
            Tree.Add(startNode);
            maxIterations = 5000;
            driveParameter = 20;
            rewireRange = 30;
            reachedGoal = false;
            //g = Graphics.FromImage(form1.myBitmap);// Bitmap myBitmap = new Bitmap(boundaryXlimits,boundaryYlimits);
            g = form1.g;//g = Graphics.FromImage(myBitmap);
            c_min = getDistance(startNode, goalNode);
            cos = (goalNode.X - startNode.X) / c_min;
            sin = (goalNode.Y - startNode.Y) / c_min;
            c_max = c_min + 400;//初始化c_max，400为随机给定的值
        }
        //informed rrt*
        public void runPlaner()
        {
            bool check = true;//第一次设置终点的父节点的标识
            int iterations = 0;
            Node randomNode = getHeuristicNode();//椭圆内获取随机点
            while (iterations < maxIterations)
            {
                if(!reachedGoal)//未抵达终点
                {
                    iterations++;
                    Node closestTreeNode = findClosestTreeNode(randomNode);//寻找离随机点的最近树节点
                    Node newNode = getNewNode(randomNode, closestTreeNode);//newNode为离closestTreeNode点距离不大于driveParameter的点
                    if (isValidNode(closestTreeNode,newNode))//判断两点直线是否可通行
                    {
                        appendTree(newNode, closestTreeNode);//将newNode加入Tree
                        //画随机点和连线
                        g.FillEllipse(new SolidBrush(Color.Green), newNode.X, form1.boundaryYlimits - newNode.Y, form1.pointRadius, form1.pointRadius);
                        g.DrawLine(new Pen(Color.Black, 2), newNode.X, form1.boundaryYlimits - newNode.Y, closestTreeNode.X, form1.boundaryYlimits - closestTreeNode.Y);
                        form1.myPictureBox.Image = form1.myBitmap;//在myPictureBox上呈现myBitmap
                        form1.myPictureBox.Refresh();//刷新myPictureBox
                    }
                    randomNode = getHeuristicNode();
                    Node leafNode = Tree.Last<Node>();//获取Tree中最后一个结点
                    reachedGoal = (isGoalReached(leafNode) && isValidNode(leafNode, goalNode));//判断是否将达终点以及可否通行
                }
                else
                {
                    if (check)
                    {
                        Node goalNodeParent = Tree.Last<Node>();
                        appendTree(goalNode, goalNodeParent);//将终点加入Tree
                        check = false;
                    }
                    iterations++;
                    Node closestTreeNode = findClosestTreeNode(randomNode);
                    Node newNode = getNewNode(randomNode, closestTreeNode);
                    Node rewiredParent = rewireRRTTree(newNode, closestTreeNode);//添加新节点，为了创建更小长度的路径
                    if (isValidNode(rewiredParent, newNode))
                    {
                        appendTree(newNode, rewiredParent);
                        g.FillEllipse(new SolidBrush(Color.DarkOrange), newNode.X, form1.boundaryYlimits - newNode.Y, form1.pointRadius, form1.pointRadius);
                        g.DrawLine(new Pen(Color.Black, 2), newNode.X, form1.boundaryYlimits - newNode.Y, rewiredParent.X, form1.boundaryYlimits - rewiredParent.Y);
                        form1.myPictureBox.Image = form1.myBitmap;
                        form1.myPictureBox.Refresh();
                    }
                    //changeParentOfAllTreeNode();
                    changeParentOfGoalNode();//改变终点的父节点，选取长度最小的路径
                    c_max = goalNode.DistanceFromRoot;//设置c_max为此时路径的长度
                    Console.WriteLine(c_max);
                    randomNode = getHeuristicNode();
                }
            }
            //到达最大迭代次数
            if (!reachedGoal&&iterations>=maxIterations)//未找到结点
            {
                Tree.Clear();//清空Tree
                MessageBox.Show("RRTStar Planner could not find a path");
            }
            else
            {
                Point[] points = getPoints();//获取路径上的点
                g.DrawLines(new Pen(Color.Yellow, 8), points);
                form1.myPictureBox.Image = form1.myBitmap;
                form1.myPictureBox.Refresh();
                g.Dispose();
            }
        }
        //获取随机点
        Node getRandomNode()
        {
            Node randomNode;
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int x = random.Next(0, form1.boundaryXlimits);
            int y = random.Next(0, form1.boundaryYlimits);
            randomNode = new Node(x, y);
            return randomNode;
        }
        //寻找离随机点最近的treeNode
        Node findClosestTreeNode(Node randomNode)
        {
            double minDist = double.MaxValue;
            Node closestNode=new Node();
            foreach(Node treeNode in Tree)
            {
                double dist = getDistance(randomNode, treeNode);
                if(dist<minDist)
                {
                    closestNode = treeNode;
                    minDist = dist;
                }
            }
            return closestNode;
        }
        //计算两点间距离
        double getDistance(Node randomNode,Node treeNode)
        {
            double distance = Math.Sqrt((randomNode.X - treeNode.X) * (randomNode.X - treeNode.X) + (randomNode.Y - treeNode.Y) * (randomNode.Y - treeNode.Y));
            return distance;
        }
        //获取限定长度内的点
        Node getNewNode(Node randomNode, Node closestTreeNode)
        {
            if(getDistance(randomNode,closestTreeNode)>driveParameter)
            {
                List<double> v = new List<double>();//向量v
                v.Add(randomNode.X - closestTreeNode.X);
                v.Add(randomNode.Y - closestTreeNode.Y);
                double distance = getDistance(randomNode, closestTreeNode);
                List<double> u = new List<double>();//向量u为v的单位化
                u.Add(v.First<double>() / distance);
                u.Add(v.Last<double>() / distance);
                randomNode= new Node((int)(closestTreeNode.X + driveParameter * u.First<double>()), (int)(closestTreeNode.Y + driveParameter * u.Last<double>()));
            }
            return randomNode;
        }
        //碰撞检测
        bool isValidNode(Node treeNode,Node newNode)
        {
            if(isOutPictureBox(newNode))//点在规定范围外
            {
                return false;
            }
            if (obstacles != null)//没障碍
            {
                foreach (Obstacle obstacle in obstacles)
                {
                    Point[] points = obstacle.points;//获取障碍角结点

                    for (int i = 0; i < points.Length; i++)
                    {
                        if (i != points.Length - 1)
                        {
                            if (isIntersect(treeNode, newNode, points[i], points[i + 1]))//会遇到障碍物
                                return false;
                        }
                        else
                        {
                            if (isIntersect(treeNode, newNode, points[i], points[0]))
                                return false;
                        }
                    }
                }
            }
            return true;
        }
        //newNode结点超出panel范围
        bool isOutPictureBox(Node newNode)
        {
            //boundaryXlimits为最大X，boundaryYlimits为最大Y
            if (newNode.X < 0 || newNode.Y < 0 || newNode.X >form1.boundaryXlimits  || newNode.Y>form1.boundaryYlimits)
                return true;
            else
                return false;
        }
        //treeNode与newNode连线是否通过了障碍物
        bool isIntersect(Node treeNode,Node newNode,Point point1,Point point2)
        {
            Point point3 = new Point(treeNode.X, treeNode.Y);
            Point point4 = new Point(newNode.X, newNode.Y);
            int resultOfTreeNode = judgeSameSide(point3, point1, point2);//判断treeNode在point1和point2连线的哪边
            int resultOfNewNode = judgeSameSide(point4, point1, point2);//判断newNode在point1和point2连线的哪边
            if ((resultOfTreeNode > 0 && resultOfNewNode > 0) || (resultOfTreeNode < 0 && resultOfNewNode < 0))//treeNode与newNode在同侧
                return false;
            else
            {
                int resultOfPoint1 = judgeSameSide(point1, point3, point4);
                int resultOfPoint2 = judgeSameSide(point2, point3, point4);
                if ((resultOfPoint1 > 0 && resultOfPoint2 > 0) || (resultOfPoint1 < 0 && resultOfPoint2 < 0))//point1与point2在同侧
                    return false;
                else
                    return true;
            }
        }
        //判断point1相对于point2和point3连线的位置
        int judgeSameSide(Point point1,Point point2,Point point3)
        {
            return ((point1.Y - point2.Y) * (point3.X - point2.X) - (point1.X - point2.X) * (point3.Y - point2.Y));
        }
        //将可行结点加入Tree
        void appendTree(Node validNode,Node validNodeParent)
        {
            validNode.Parent = validNodeParent;//设置父节点
            double distanceOfParent = validNodeParent.DistanceFromRoot;
            double dist = getDistance(validNode, validNodeParent);
            validNode.DistanceFromRoot = dist + distanceOfParent;//设置validNode到起点的路径长度
            //if (validNode.X == goalNode.X && validNode.Y == goalNode.Y)//判断validNode是否在goalNode位置
            //    goalNode = validNode;
            Tree.Add(validNode);//加入validNode结点
        }
        //判断结点是否将要到达终点
        bool isGoalReached(Node leafNode)
        {
            double dist = getDistance(leafNode, goalNode);
            if(dist <= driveParameter)
                return true;
            else
                return false;
        }
        ////改变所有树节点的父节点
        //void changeParentOfAllTreeNode()
        //{
        //    Node rewiredParent;
        //    foreach (Node treeNode in Tree)
        //    {
        //        if (treeNode.Parent == null)
        //            continue;
        //        rewiredParent = rewireRRTTree(treeNode, treeNode.Parent);
        //        if ((treeNode.Parent.X != rewiredParent.X) && (treeNode.Y != rewiredParent.Y) && isValidNode(rewiredParent, treeNode))
        //            treeNode.Parent = rewiredParent;
        //    }
        //    foreach(Node treeNode in Tree)
        //    {
        //        if (treeNode.Parent == null)
        //            continue;
        //        double distanceOfParent = treeNode.Parent.DistanceFromRoot;
        //        double dist = getDistance(treeNode, treeNode.Parent);
        //        treeNode.DistanceFromRoot = dist + distanceOfParent;
        //    }
        //}
        //优化路径
        void changeParentOfGoalNode()
        {
            Node rewiredParent = rewireRRTTree(goalNode, goalNode.Parent);//寻找路径长度更小的终点潜在父节点
            if((rewiredParent.X!=goalNode.Parent.X)&&(rewiredParent.Y!=goalNode.Parent.Y)&&isValidNode(goalNode,rewiredParent))
            {
                goalNode.Parent = rewiredParent;
                goalNode.DistanceFromRoot = rewiredParent.DistanceFromRoot + getDistance(rewiredParent, goalNode);
            }

        }
        //寻找代价最小路径
        Node rewireRRTTree(Node newNode,Node currentParent)
        {
            List<Node> rewireNodes =new List<Node>();
            //寻找在半径为rewireRange的圆中newNode的潜在父节点
            foreach (Node treeNode in Tree)
            {
                if(getDistance(treeNode,newNode)< rewireRange)
                {
                    rewireNodes.Add(treeNode);
                }
            }
            double parentCost = currentParent.DistanceFromRoot;
            double dist = getDistance(newNode, currentParent);
            double minCost = parentCost + dist;
            //在潜在父节点中寻找路径长度最小的父节点
            foreach(Node rnd in rewireNodes)
            {
                double rndParentCost = rnd.DistanceFromRoot;
                dist = getDistance(newNode, rnd);
                double rewireNodeCost = rndParentCost + dist;
                if(rewireNodeCost< minCost)
                {
                    currentParent = rnd;
                    minCost = rewireNodeCost;
                }
            }
            return currentParent;
        }
        //椭圆内获取随机点
        Node getHeuristicNode()
        {
            double a = c_max / 2;
            double b = Math.Sqrt(c_max * c_max - c_min * c_min)/2;
            Random random = new Random(Guid.NewGuid().GetHashCode());//使用Guid.NewGuid().GetHashCode()作为种子，随机数将不同
            int flag = (random.Next()%2==0)?1:(-1);
            int x = flag*random.Next((int)a);
            flag = (random.Next()%2==0) ? 1 : (-1);
            int y = flag*random.Next((int)(Math.Sqrt((a * a * b * b - b * b * x * x) /(a * a))));
            int X = (int)(x * cos - y * sin + (goalNode.X + startNode.X) / 2);
            int Y = (int)(x * sin + y * cos + (goalNode.Y + startNode.Y) / 2);
            return (new Node(X, Y));
        }
        //将路径中的Node转化为Point[]
        Point[] getPoints()
        {
            int index = 0;
            Node currentNode = goalNode;
            //获取路径上结点总数
            while (currentNode.Parent != null)
            {
                index++;
                currentNode = currentNode.Parent;
            }
            index++;
            Point[] points = new Point[index];
            currentNode = goalNode;
            for(int i=0;i<index;i++)
            {
                points[i].X = currentNode.X;
                points[i].Y = form1.boundaryYlimits - currentNode.Y;
                currentNode = currentNode.Parent;
            }
            return points;
        }
    }
}