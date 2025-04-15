using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示Reasoner块的AST节点（包含一组考虑项定义）
    /// </summary>
    public class ReasonerBlock : Node
    {
        /// <summary>
        /// 考虑项定义列表
        /// </summary>
        public List<Node> ConsiderationDefinitions { get; private set; } = new List<Node>();
        
        /// <summary>
        /// 获取Reasoner块的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"ReasonerBlock ({ConsiderationDefinitions.Count} considerations)";
        }
    }
} 