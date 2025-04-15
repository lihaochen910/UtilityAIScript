using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI {

	/// <summary>
	/// Scores by summing child Appraisals until a child scores below the threshold
	/// </summary>
	public class ThresholdConsideration< T > ( float threshold ) : BaseConsideration< T > {
		
		public readonly float Threshold = threshold;
		
		private ImmutableArray< IAppraisal< T > > _appraisals = ImmutableArray< IAppraisal< T > >.Empty;

		public ThresholdConsideration< T > AddAppraisal( IAppraisal< T > appraisal ) {
			_appraisals = _appraisals.Add( appraisal );
			return this;
		}

		
		public override float GetScore( T context ) {
#if DEBUG
			var sum = 0f;
			var returned = false;
			var returnedValue = sum;
			for ( var i = 0; i < _appraisals.Length; i++ ) {
				var score = _appraisals[ i ].GetScore( context );
				CacheDebugUnitScore( _appraisals[ i ], score );
				if ( score < Threshold ) {
					returned = true;
					returnedValue = sum;
				}

				sum += score;
			}

			if ( returned ) {
#if DEBUG
				CacheResultScore( returnedValue );
#endif
				return returnedValue;
			}

#if DEBUG
			CacheResultScore( sum );
#endif
			return sum;
#else
			var sum = 0f;
			for ( var i = 0; i < _appraisals.Length; i++ ) {
				var score = _appraisals[ i ].GetScore( context );
				if ( score < Threshold ) {
					return sum;
				}

				sum += score;
			}

			return sum;
#endif
		}
		
#if DEBUG
		public override void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {
			level++;
			indent?.Invoke(); stringBuilder.AppendLine( $"{Name} = {TryGetCachedResultScore():0.00} ({DebugTypeName()})" );
			indent?.Invoke(); stringBuilder.AppendLine( $" Threshold: {Threshold:0.00}" );
			level++;
			foreach ( var appraisal in _appraisals ) {
				var score = TryGetDebugUnitScore( appraisal );
				var preUnary = score > 0 ? "+" : string.Empty;
				var okMark = score >= Threshold ? DebugMarkOk : DebugMarkFail;
				if ( appraisal is BaseAppraisal< T > baseAppraisal ) {
					indent?.Invoke(); stringBuilder.AppendLine( $"{baseAppraisal.Notes} -> {preUnary}{score:0.00} {okMark}" );
				}
				else {
					indent?.Invoke(); stringBuilder.AppendLine( $"{appraisal.GetType().Name} -> {preUnary}{score:0.00} {okMark}" );
				}
			}
			level--;
			level--;
		}
#endif
	}

}
