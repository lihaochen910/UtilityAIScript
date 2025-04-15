using System;
using System.Linq;
using System.Text;
using Irony.Parsing;


namespace Pixpil.AI.UtilityAIScript.Exceptions;

public sealed class ScriptCompileException : Exception {
	
	public ParseTree ParseTree { get; }

	public ScriptCompileException( ParseTree parseTree ) {
		ParseTree = parseTree;
	}

	public string PrintMessage() {
		return string.Join( "\n", ParseTree.ParserMessages.Select( m => $"{m.Location}: {m.Message}" ) );
	}

	public string PrintTreeMessage() {
		var builder = new StringBuilder();
		PrintNode( ParseTree.Root, 0, str => builder.AppendLine( str ) );
		return builder.ToString();
	}
	
	private void PrintNode( ParseTreeNode node, int level, Action< string > printFunc = null ) {
		string indent = new string( ' ', level * 2 );
		string nodeText = node.Term.Name;

		if ( node.Token != null ) {
			nodeText += $" [{node.Token.ValueString}]";
		}

		printFunc?.Invoke( $"{indent}{nodeText}" );

		foreach ( var child in node.ChildNodes ) {
			PrintNode( child, level + 1, printFunc );
		}
	}
	
}
