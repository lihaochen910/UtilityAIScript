namespace Pixpil.AI.UtilityAI {

	/// <summary>
	/// The Consideration with the lowest score is selected
	/// </summary>
	public class LowestScoreReasoner< T > : Reasoner< T > {
		
		protected override IConsideration< T > SelectBestConsideration( T context ) {
			var lowestScore = DefaultConsideration.GetScore( context );
			IConsideration< T > consideration = null;
			for ( var i = 0; i < _considerations.Length; i++ ) {
				var score = _considerations[ i ].GetScore( context );
				if ( score < lowestScore ) {
					lowestScore = score;
					consideration = _considerations[ i ];
				}
			}

			if ( consideration == null ) {
				return DefaultConsideration;
			}

			return consideration;
		}
		
	}

}