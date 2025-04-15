namespace Pixpil.AI.UtilityAIScript.Ast {

    /// <summary>
    /// 表示评估定义的AST节点
    /// </summary>
    public class AppraisalDefine : Node {
        
        /// <summary>
        /// 评估表达式
        /// </summary>
        public Node Expression { get; internal set; }

        /// <summary>
        /// 获取评估定义的字符串表示
        /// </summary>
        public override string ToString() {
            return $"AppraisalDefine: {Expression}";
        }
        
    }

}
