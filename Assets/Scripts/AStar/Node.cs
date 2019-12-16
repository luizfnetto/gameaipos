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
        public NodeType tipo;

        public Node(int x, int y, NodeType tipo)
        {
            this.x = x;
            this.y = y;
            this.tipo = tipo;
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
