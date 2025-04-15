namespace Pixpil.AI.UtilityAI {

	/// <summary>
	/// always returns a fixed score. Serves double duty as a default Consideration.
	/// </summary>
	public class FixedScoreConsideration< T > ( float score = 1f ) : BaseConsideration< T > {
		
		public readonly float Score = score;

		public override float GetScore( T context ) {
#if DEBUG
			CacheResultScore( Score );
#endif
			return Score;
		}
		
#if DEBUG
		public override void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {
			level++;
			indent?.Invoke(); stringBuilder.AppendLine( $"{Name} = {Score:0.00} ({DebugTypeName()})" );
			level--;
		}
#endif
	}

}