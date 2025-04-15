using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示Reasoner定义的AST节点
    /// </summary>
    public class ReasonerDefine : Node
    {
        /// <summary>
        /// Reasoner类型符号（如 +, -, > 或自定义类型名称）
        /// </summary>
        public string ReasonerType { get; internal set; }
        
        /// <summary>
        /// 是否是内置Reasoner类型
        /// </summary>
        public bool IsBuiltIn { get; internal set; }
        
        /// <summary>
        /// 考虑项定义列表
        /// </summary>
        public List<Node> Considerations { get; } = new ();
        
        /// <summary>
        /// 获取Reasoner定义的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"Reasoner[{ReasonerType}] ({Considerations.Count} considerations)";
        }
    }
} 