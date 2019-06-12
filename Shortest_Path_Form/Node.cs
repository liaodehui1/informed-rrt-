using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shortest_Path_Form
{
    public class Node
    {
        //public List<Node> Children { get; private set; }
        //public double DistanceFromGoal { get; set; }
        public double DistanceFromRoot { get; set; }
        public Node Parent { get; set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public Node()
        {
            DistanceFromRoot = 0;
            Parent = null;
            //Children = null;
        }
        public Node(int x, int y)
        {
            X = x;
            Y = y;
            DistanceFromRoot = 0;
            Parent = null;
            //Children = null;
        }
    }
}
