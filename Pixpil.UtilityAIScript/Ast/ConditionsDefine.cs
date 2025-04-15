using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示条件集合定义的AST节点
    /// </summary>
    public class ConditionsDefine : Node
    {
        /// <summary>
        /// 条件块节点（包含多个条件定义）
        /// </summary>
        public List<ConditionBlock> Conditions { get; } = new ();
        
        /// <summary>
        /// 获取条件集合定义的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"conditions: ({Conditions.Count} conditions)";
        }
    }
} 