using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast {

    /// <summary>
    /// 表示考虑项块的AST节点（包含评估和行动定义）
    /// </summary>
    public class ConsiderationBlock : Node {

        /// <summary>
        /// 块内的项目列表
        /// </summary>
        public List< Node > Items { get; } = new ();

        /// <summary>
        /// 评估表达式（如果有）
        /// </summary>
        public List< AppraisalDefine > AppraisalDefines { get; } = new ();

        /// <summary>
        /// 行动定义（如果有）
        /// </summary>
        public ActionDefine ActionDefine { get; internal set; }

        /// <summary>
        /// 获取考虑项块的字符串表示
        /// </summary>
        public override string ToString() => $"ConsiderationBlock ({Items.Count} items)";
    }

}