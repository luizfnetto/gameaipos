using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAIPos
{
    public class Node
    {
        public int x, y;
        public NodeType type;

        public Node(int x, int y, NodeType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }

    public enum NodeType
    {
        NoiseField,
        PlainField,
        Scrambled,
        InitialConn,
        LocalMainFrame,
        StrongSysDef,
        NormalSysDef
    }
}
