using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI {

	/// <summary>
	/// Scores by summing the score of all child Appraisals
	/// </summary>
	public class SumOfChildrenConsideration< T > : BaseConsideration< T > {
		
		private ImmutableArray< IAppraisal< T > > _appraisals = ImmutableArray< IAppraisal< T > >.Empty;


		public SumOfChildrenConsideration< T > AddAppraisal( IAppraisal< T > appraisal ) {
			_appraisals = _appraisals.Add( appraisal );
			return this;
		}

		public override float GetScore( T context ) {
			var score = 0f;
			for ( var i = 0; i < _appraisals.Length; i++ ) {
				var appraisalScore = _appraisals[ i ].GetScore( context );
				score += appraisalScore;
#if DEBUG
				CacheDebugUnitScore( _appraisals[ i ], appraisalScore );
#endif
			}

#if DEBUG
			CacheResultScore( score );
#endif
			return score;
		}
		
#if DEBUG
		public override void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {
			level++;
			indent?.Invoke(); stringBuilder.AppendLine( $"{Name} = {TryGetCachedResultScore():0.00} ({DebugTypeName()})" );
			level++;
			foreach ( var appraisal in _appraisals ) {
				var score = TryGetDebugUnitScore( appraisal );
				var preUnary = score > 0 ? "+" : string.Empty;
				if ( appraisal is BaseAppraisal< T > baseAppraisal ) {
					indent?.Invoke(); stringBuilder.AppendLine( $"{baseAppraisal.Notes} -> {preUnary}{score:0.00}" );
				}
				else {
					indent?.Invoke(); stringBuilder.AppendLine( $"{appraisal.GetType().Name} -> {preUnary}{score:0.00}" );
				}
			}
			level--;
			level--;
		}
#endif

	}

}
