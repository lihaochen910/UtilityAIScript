using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Irony.Parsing;


namespace Pixpil.AI.UtilityAIScript;

public static class GrammarAnalyzer {

    public static void AnalyzeGrammar( Grammar grammar, Action< string > printFunc = null ) {
        printFunc ??= Console.WriteLine;

        printFunc( "=== Grammar Analysis ===" );

        // 获取Grammar中的所有非终结符
        var nonTerminals = GetAllNonTerminals( grammar );

        // 输出所有未命名的非终结符
        var unnamedNonTerminals = nonTerminals.Where( nt => nt.Name.StartsWith( "Unnamed" ) ).ToList();
        printFunc( $"Found {unnamedNonTerminals.Count} unnamed non-terminals:" );

        foreach ( var nonTerminal in unnamedNonTerminals ) {
            printFunc( $"  - {nonTerminal.Name}" );

            // 输出规则信息
            if ( nonTerminal.Rule != null ) {
                printFunc( $"    Rule: {nonTerminal.Rule}" );
                printFunc(
                    $"    Child Terms: {string.Join( ", ", GetChildTerms( nonTerminal ).Select( t => t.Name ) )}" );
            }

            // 检查AST配置
            var astConfig = nonTerminal.AstConfig;
            printFunc( $"    NodeType: {( astConfig.NodeType == null ? "null" : astConfig.NodeType.Name )}" );
            printFunc( $"    NodeCreator: {( astConfig.DefaultNodeCreator == null ? "null" : "set" )}" );
        }
    }

    private static List< NonTerminal > GetAllNonTerminals( Grammar grammar ) {
        // 使用反射获取Grammar中的所有非终结符
        var languageField = typeof( BnfTerm ).GetField( "GrammarData", BindingFlags.NonPublic | BindingFlags.Instance );
        var grammarData = languageField.GetValue( grammar.Root ) as GrammarData;

        var nonTerminalsField =
            grammarData.GetType().GetField( "NonTerminals", BindingFlags.Public | BindingFlags.Instance );
        var nonTerminalsCollection = nonTerminalsField.GetValue( grammarData ) as System.Collections.IEnumerable;

        var result = new List< NonTerminal >();
        foreach ( NonTerminal nt in nonTerminalsCollection ) {
            result.Add( nt );
        }

        return result;
    }

    private static List< BnfTerm > GetChildTerms( NonTerminal nonTerminal ) {
        // 使用反射获取非终结符的子项
        var result = new List< BnfTerm >();

        if ( nonTerminal.Rule != null ) {
            var ruleType = nonTerminal.Rule.GetType();

            // 检查是否是序列规则
            if ( ruleType.Name == "SequenceRule" || ruleType.Name == "AlternationRule" ) {
                var termsField = ruleType.GetField( "_terms", BindingFlags.NonPublic | BindingFlags.Instance );
                var terms = termsField.GetValue( nonTerminal.Rule ) as System.Collections.IEnumerable;

                foreach ( BnfTerm term in terms ) {
                    result.Add( term );
                }
            }

            // 单个项目规则
            else if ( ruleType.Name == "SimpleRule" ) {
                var termField = ruleType.GetField( "_term", BindingFlags.NonPublic | BindingFlags.Instance );
                var term = termField.GetValue( nonTerminal.Rule ) as BnfTerm;

                result.Add( term );
            }
        }

        return result;
    }

}
