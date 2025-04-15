using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast {

    /// <summary>
    /// 表示条件块的AST节点（包含多个条件表达式）
    /// </summary>
    public class ConditionBlock : Node {

        /// <summary>
        /// 条件表达式列表
        /// </summary>
        public List< ConditionExpression > ConditionExpressions { get; } = new ();

        /// <summary>
        /// 获取条件块的字符串表示
        /// </summary>
        public override string ToString() {
            return $"ConditionBlock ({ConditionExpressions.Count} expressions)";
        }

    }

}
