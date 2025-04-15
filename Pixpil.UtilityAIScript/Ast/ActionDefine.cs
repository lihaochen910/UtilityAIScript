namespace Pixpil.AI.UtilityAIScript.Ast
{
    /// <summary>
    /// 表示行动定义的AST节点
    /// </summary>
    public class ActionDefine : Node
    {
        /// <summary>
        /// 行动名称
        /// </summary>
        public string ActionName { get; internal set; }
        
        /// <summary>
        /// 获取行动定义的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"-> {ActionName}";
        }
    }
} 