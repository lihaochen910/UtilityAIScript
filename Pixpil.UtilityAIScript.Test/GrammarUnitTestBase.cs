using Irony.Parsing;
using Pixpil.AI.UtilityAI;
using Xunit.Abstractions;


namespace Pixpil.AI.UtilityAIScript.Test;

public abstract class GrammarUnitTestBase {

	private readonly UtilityAIGrammar _grammar;
	private readonly Parser _parser;
	// private readonly UtilityAIParser _parser = new ();
	private readonly ITestOutputHelper _output;

	protected GrammarUnitTestBase( ITestOutputHelper output ) {
		_grammar = new UtilityAIGrammar();
		_parser = new Parser( new LanguageData( _grammar ) );
		_output = output;
	}

	protected void Log( string message ) => _output.WriteLine( message );
	protected void Log( string message, params object[] args ) => _output.WriteLine( message, args );

	protected void TestParseSource( string script ) {
		var tree = _parser.Parse( script );
		
		void PrintSource() {
			var lineNo = 1;
			foreach ( var line in tree.SourceText.Split( "\n" ) ) {
				Log($"{lineNo}\t{line}" );
				lineNo++;
			}
		}
		
		void PrintNode( ParseTreeNode node, int level ) {
			string indent = new string( ' ', level * 2 );
			string nodeText = node.Term.Name;

			if ( node.Token != null ) {
				nodeText += $" [{node.Token.ValueString}]";
			}

			Log( $"{indent}{nodeText}" );

			foreach ( var child in node.ChildNodes ) {
				PrintNode( child, level + 1 );
			}
		}
		
		if ( tree.ParserMessages.Count > 0 ) {
			if ( tree.SourceText.Length < 0xff ) {
				PrintSource();
			}
			Log( "failed:" );
			foreach ( var error in tree.ParserMessages ) {
				Log( $"\t{error.Location}: [{error.Level}] {error.Message}" );
			}
			foreach ( var token in tree.Tokens ) {
				Log( $"{token}, " );
			}

			Log("\n-----------------------------------");
			Assert.Fail();
		}

		PrintNode( tree.Root, 0 );
	}

}
