namespace GameAIPos
{
    public class NormalSysDef : Node
    {
        public double difficulty;

        public NormalSysDef(int x, int y, double difficulty) : base(x, y, NodeType.NormalSysDef)
        {
            this.difficulty = difficulty;
        }
    }
}