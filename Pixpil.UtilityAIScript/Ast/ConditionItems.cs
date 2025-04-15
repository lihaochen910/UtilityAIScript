using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示条件项集合的AST节点
    /// </summary>
    public class ConditionItems : Node
    {
        /// <summary>
        /// 条件项列表
        /// </summary>
        public List<ConditionItem> Items { get; } = new ();
        
        /// <summary>
        /// 获取条件项集合的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"Items[{Items.Count}]";
        }
    }
}