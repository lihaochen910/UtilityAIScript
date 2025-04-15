namespace Pixpil.AI.UtilityAIScript.Ast;

public readonly struct SourceSpan {


	#region Constants and Fields

	/// <summary>
	/// Location of this span.
	/// </summary>
	public readonly SourceLocation Location;

	/// <summary>
	/// Length of this span.
	/// </summary>
	public readonly int Length;

	#endregion


	#region Constructors and Destructors

	/// <summary>
	/// Initializes a new instance of the <see cref="SourceSpan"/> struct.
	/// </summary>
	/// <param name="location">
	/// The location.
	/// </param>
	/// <param name="length">
	/// The length.
	/// </param>
	public SourceSpan( SourceLocation location, int length ) {
		Location = location;
		Length = length;
	}

	public SourceSpan UpdateFileSource( string fileSource ) {
		return new SourceSpan( new SourceLocation(
			fileSource,
			Location.Position,
			Location.Line,
			Location.Column
		), Length );
	}

	/// <inheritdoc/>
	public override string ToString() {
		return string.Format( "{0}", Location );
	}

	#endregion


}
