using System.IO;


namespace Pixpil.AI.UtilityAIScript.Ast;

public readonly struct SourceLocation {
	
	#region Constants and Fields

	/// <summary>
	/// Filename source.
	/// </summary>
	public readonly string FileSource;

	/// <summary>
	/// Absolute position in the file.
	/// </summary>
	public readonly int Position;

	/// <summary>
	/// Line in the file (1-based).
	/// </summary>
	public readonly int Line;

	/// <summary>
	/// Column in the file (1-based).
	/// </summary>
	public readonly int Column;

	#endregion


	#region Constructors and Destructors

	/// <summary>
	/// Initializes a new instance of the <see cref="SourceLocation"/> struct.
	/// </summary>
	/// <param name="fileSource">The file source.</param>
	/// <param name="position">The position.</param>
	/// <param name="line">The line.</param>
	/// <param name="column">The column.</param>
	public SourceLocation( string fileSource, int position, int line, int column ) {
		FileSource = fileSource;
		Position = position;
		Line = line;
		Column = column;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SourceLocation"/> struct.
	/// </summary>
	/// <param name="position">The position.</param>
	/// <param name="line">The line.</param>
	/// <param name="column">The column.</param>
	public SourceLocation( int position, int line, int column )
		: this() {
		Position = position;
		Line = line;
		Column = column;
	}

	/// <inheritdoc/>
	public override string ToString() {
		return ToString( false );
	}

	public string ToString( bool useShortFileName ) {
		if ( useShortFileName ) {
			return string.Format( "[{0}]({1},{2})", Path.GetFileName( FileSource ) ?? string.Empty, Line, Column );
		}
		
		return string.Format( "[{0}]({1},{2})", FileSource ?? string.Empty, Line, Column );
	}

	#endregion
	
}
