using System;
using System.IO;
using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;


namespace Pixpil.AI.UtilityAIScript;

public class UtilityAIParser {

	private readonly UtilityAIGrammar _grammar;
	private readonly LanguageData _languageData;
	private readonly AstContext _astContext;
	private readonly Parser _parser;

	public UtilityAIGrammar Grammar => _grammar;

	public LanguageData LanguageData => _languageData;

	public UtilityAIParser() {
		_grammar = new UtilityAIGrammar();
		_languageData = new LanguageData( _grammar );
		_astContext = new AstContext( _languageData );
		_parser = new Parser( _languageData );

		_astContext.DefaultNodeType = typeof( AstNode );
	}

	/// <summary>
	/// 解析文本
	/// </summary>
	/// <param name="text">输入代码文本</param>
	/// <returns>解析树</returns>
	public ParseTree Parse( string text ) => _parser.Parse( text );

	/// <summary>
	/// 解析文件
	/// </summary>
	/// <param name="filePath">文件路径</param>
	/// <returns>解析树</returns>
	public ParseTree ParseFile( string filePath ) {
		string text = File.ReadAllText( filePath );
		return Parse( text );
	}
	
	public void ParseAndPrint( string text ) {
		var parseTree = _parser.Parse( text );
		PrintParseTree( parseTree );
	}

	/// <summary>
	/// 打印解析树
	/// </summary>
	/// <param name="tree">解析树</param>
	public void PrintParseTree( ParseTree tree ) {

		void PrintSource() {
			var lineNo = 1;
			foreach ( var line in tree.SourceText.Split( '\n' ) ) {
				Console.WriteLine($"{lineNo}\t{line}" );
				lineNo++;
			}
		}
		
		if ( tree.ParserMessages.Count > 0 ) {
			if ( tree.SourceText.Length < 0xff ) {
				PrintSource();
			}
			Console.WriteLine( "failed:" );
			foreach ( var error in tree.ParserMessages ) {
				Console.WriteLine( $"\t{error.Location}: [{error.Level}] {error.Message}" );
			}
			foreach ( var token in tree.Tokens ) {
				Console.Write( $"{token}, " );
			}

			Console.WriteLine("\n-----------------------------------");
			return;
		}

		// PrintSource();
		
		Console.WriteLine( "pass!" );
		PrintNode( tree.Root, 0 );
		Console.WriteLine("-----------------------------------");
	}

	private void PrintNode( ParseTreeNode node, int level ) {
		string indent = new string( ' ', level * 2 );
		string nodeText = node.Term.Name;

		if ( node.Token != null ) {
			nodeText += $" [{node.Token.ValueString}]";
		}

		Console.WriteLine( $"{indent}{nodeText}" );

		foreach ( var child in node.ChildNodes ) {
			PrintNode( child, level + 1 );
		}
	}
	
}
