using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast {

    /// <summary>
    /// 条件列表修饰符
    /// </summary>
    public enum ConditionListModifier {
        /// <summary>
        /// 全部条件都满足
        /// </summary>
        All,

        /// <summary>
        /// 任意条件满足
        /// </summary>
        Any
    }


    /// <summary>
    /// 表示条件表达式的AST节点
    /// </summary>
    public class ConditionExpression : Node {
        
        /// <summary>
        /// 条件名称
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 条件修饰符（All或Any）
        /// </summary>
        public ConditionListModifier Modifier { get; internal set; }

        /// <summary>
        /// 条件项列表
        /// </summary>
        public List< ConditionItem > Items { get; } = new ();

        /// <summary>
        /// 获取条件表达式的字符串表示
        /// </summary>
        public override string ToString() {
            return $"{Name}: {Modifier}[{Items.Count} items]";
        }
        
    }

}
