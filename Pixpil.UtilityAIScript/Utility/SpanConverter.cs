using Pixpil.AI.UtilityAIScript.Ast;


namespace Pixpil.AI.UtilityAIScript.Utility;

public static class SpanConverter {
	
	public static SourceLocation Convert( Irony.Parsing.SourceLocation sourceLocation ) {
		// return new SourceLocation(sourceLocation.SourceFilename, sourceLocation.Position, sourceLocation.Line, sourceLocation.Column);
		return new SourceLocation( string.Empty, sourceLocation.Position, sourceLocation.Line, sourceLocation.Column );
	}

	public static Irony.Parsing.SourceLocation Convert( SourceLocation sourceLocation ) {
		return new Irony.Parsing.SourceLocation( sourceLocation.Position, sourceLocation.Line, sourceLocation.Column );
	}

	public static SourceSpan Convert( Irony.Parsing.SourceSpan sourceSpan ) {
		return new SourceSpan( Convert( sourceSpan.Location ), sourceSpan.Length );
	}
	
}
