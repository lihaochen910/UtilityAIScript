namespace Pixpil.AI.UtilityAI;

public abstract class BaseConsideration< T > : IConsideration< T > {

	public string Name { get; set; }

	public IAction< T > Action { get; set; }
	
	public abstract float GetScore( T context );
	
#if DEBUG
	private float _considerationResultScore;
	private readonly Dictionary< IAppraisal< T >, float > _unitScores = new ();
	
	protected void CacheDebugUnitScore( IAppraisal< T > appraisal, float score ) => _unitScores[ appraisal ] = score;
	
	protected void CacheResultScore( float score ) => _considerationResultScore = score;

	public Dictionary< IAppraisal< T >, float > GetDebugUnitScoresDictionary() => _unitScores;

	public float TryGetCachedResultScore() => _considerationResultScore;

	public float? TryGetDebugUnitScore( IAppraisal< T > appraisal ) {
		if ( _unitScores.TryGetValue( appraisal, out var value ) ) {
			return value;
		}

		return null;
	}

	public void ClearDebugUnitScoreCache() {
		_unitScores.Clear();
		_considerationResultScore = 0f;
	}

	// protected const string DebugMarkOk = "\u2713";
	protected const string DebugMarkOk = "ok";
	// protected const string DebugMarkFail = "\u2717";
	protected const string DebugMarkFail = "x";


	public string DebugTypeName() {
		var input = GetType().Name;
		if ( input.EndsWith( "Consideration`1", StringComparison.OrdinalIgnoreCase ) ) {
			return input.Substring( 0, input.Length - "Consideration`1".Length ).TrimEnd();
		}

		return input;
	}

	public virtual void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {}
#endif

}
