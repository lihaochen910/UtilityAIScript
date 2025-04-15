using System.Collections.Generic;
using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI {

	/// <summary>
	/// Only scores if all child Appraisals score above the threshold
	/// </summary>
	public class AllOrNothingConsideration< T > : BaseConsideration< T > {
		
		public readonly float Threshold;
		
		private ImmutableArray< IAppraisal< T > > _appraisals = ImmutableArray< IAppraisal< T > >.Empty;
		
		public AllOrNothingConsideration( float threshold = 0 ) {
			Threshold = threshold;
		}
		
		public AllOrNothingConsideration( float threshold, IAppraisal< T >[] appraisals ) {
			Threshold = threshold;
			_appraisals = appraisals.ToImmutableArray();
		}
		
		public AllOrNothingConsideration< T > AddAppraisal( IAppraisal< T > appraisal ) {
			_appraisals = _appraisals.Add( appraisal );
			return this;
		}
		
		public override float GetScore( T context ) {
#if DEBUG
			var sum = 0f;
			var returned = false;
			for ( var i = 0; i < _appraisals.Length; i++ ) {
				var score = _appraisals[ i ].GetScore( context );
				CacheDebugUnitScore( _appraisals[ i ], score );
				if ( score < Threshold ) {
					returned = true;
				}
				else {
					sum += score;
				}
			}

			if ( returned ) {
#if DEBUG
				CacheResultScore( 0 );
#endif
				return 0f;
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
					return 0;
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
			indent?.Invoke(); stringBuilder.AppendLine( $" Threshold: {Threshold}" );
				level++;
				foreach ( var appraisal in _appraisals ) {
					var score = TryGetDebugUnitScore( appraisal );
					var ok = score >= Threshold;
					var okMark = ok ? DebugMarkOk : DebugMarkFail;
					if ( appraisal is BaseAppraisal< T > baseAppraisal ) {
						indent?.Invoke(); stringBuilder.AppendLine( $"{baseAppraisal.Notes} -> {score:0.00} {okMark}" );
					}
					else {
						indent?.Invoke(); stringBuilder.AppendLine( $"{appraisal.GetType().Name} -> {score:0.00}" );
					}
				}
				level--;
			level--;
		}
#endif
		
	}

}
