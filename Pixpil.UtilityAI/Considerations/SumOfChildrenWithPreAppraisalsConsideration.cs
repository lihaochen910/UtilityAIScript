using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI;

public enum PreAppraisalsCheckMode : byte {
	AllRequired,
	AnySuffice
}

/// <summary>
/// Appraisals中所有IAppraisal子项必须大于Threshold, 才返回OptionalAppraisals字段中计算总和, 否则返回0.
/// </summary>
public class SumOfChildrenWithPreAppraisalsConsideration< T > : BaseConsideration< T > {
	
	public readonly float Threshold;
	public readonly PreAppraisalsCheckMode PreCheckMode = PreAppraisalsCheckMode.AllRequired;
	private ImmutableArray< IAppraisal< T > > _appraisals = ImmutableArray< IAppraisal< T > >.Empty;
	private ImmutableArray< IAppraisal< T > > _optionalAppraisals = ImmutableArray< IAppraisal< T > >.Empty;

	public SumOfChildrenWithPreAppraisalsConsideration() {}

	public SumOfChildrenWithPreAppraisalsConsideration( float threshold, PreAppraisalsCheckMode preCheckMode ) {
		Threshold = threshold;
		PreCheckMode = preCheckMode;
	}

	public SumOfChildrenWithPreAppraisalsConsideration( float threshold, PreAppraisalsCheckMode preCheckMode, IAppraisal< T >[] appraisals, IAppraisal< T >[] optionalAppraisals = null ) {
		Threshold = threshold;
		PreCheckMode = preCheckMode;
		_appraisals = appraisals.ToImmutableArray();
		_optionalAppraisals = optionalAppraisals != null ? optionalAppraisals.ToImmutableArray() : ImmutableArray< IAppraisal< T > >.Empty;
	}

	public SumOfChildrenWithPreAppraisalsConsideration< T > AddAppraisal( IAppraisal< T > appraisal ) {
		_appraisals = _appraisals.Add( appraisal );
		return this;
	}
	
	public SumOfChildrenWithPreAppraisalsConsideration< T > AddOptionalAppraisal( IAppraisal< T > appraisal ) {
		_optionalAppraisals = _optionalAppraisals.Add( appraisal );
		return this;
	}

	
	public override float GetScore( T context ) {
#if DEBUG
		var sum = 0f;
		var preCheckPassed = PreCheckMode is PreAppraisalsCheckMode.AllRequired ? true : false;
		for ( var i = 0; i < _appraisals.Length; i++ ) {
			var score = _appraisals[ i ].GetScore( context );
			CacheDebugUnitScore( _appraisals[ i ], score );
			if ( PreCheckMode is PreAppraisalsCheckMode.AllRequired ) {
				if ( score < Threshold ) {
					preCheckPassed = false;
				}
			}
			else {
				if ( score >= Threshold ) {
					preCheckPassed = true;
				}
			}
		}
		
		foreach ( var optionalApp in _optionalAppraisals ) {
			var score = optionalApp.GetScore( context );
			CacheDebugUnitScore( optionalApp, score );
			sum += score;
		}
		
		if ( !preCheckPassed ) {
			CacheResultScore( 0f );
			return 0f;
		}
		
		CacheResultScore( sum );
		return sum;
#else
		var sum = 0f;
		var preCheckPassed = PreCheckMode is PreAppraisalsCheckMode.AllRequired ? true : false;
		for ( var i = 0; i < _appraisals.Length; i++ ) {
			var score = _appraisals[ i ].GetScore( world, entity );
			if ( PreCheckMode is PreAppraisalsCheckMode.AllRequired ) {
				if ( score < Threshold ) {
					return 0f;
				}
			}
			else {
				if ( score >= Threshold ) {
					preCheckPassed = true;
				}
			}
		}

		if ( !preCheckPassed ) {
			return 0f;
		}

		foreach ( var optionalApp in OptionalAppraisals ) {
			var score = optionalApp.GetScore( world, entity );
			sum += score;
		}

		return sum;
#endif
	}
	
	public override string ToString() {
		if ( string.IsNullOrEmpty( Name ) ) {
			return $"Consideration: -> {Action}";
		}
		return $"Consideration: {Name}";
	}
	
#if DEBUG
	public override void DebugDump( System.Text.StringBuilder stringBuilder, Action indent, ref int level ) {
		level++;
		indent?.Invoke(); stringBuilder.AppendLine( $"{Name} = {TryGetCachedResultScore():0.00} ({DebugTypeName()})" );
		indent?.Invoke(); stringBuilder.AppendLine( $" Threshold: {Threshold:0.00}" );
		indent?.Invoke(); stringBuilder.AppendLine( $" PreCheckMode: {PreCheckMode}" );
		indent?.Invoke(); stringBuilder.AppendLine( " Pre:" );
		level++;
		foreach ( var appraisal in _appraisals ) {
			var score = TryGetDebugUnitScore( appraisal );
			var okMark = score >= Threshold ? DebugMarkOk : DebugMarkFail;
			if ( appraisal is BaseAppraisal< T > baseAppraisal ) {
				indent?.Invoke(); stringBuilder.AppendLine( $"{baseAppraisal.Notes} -> {score:0.00} {okMark}" );
			}
			else {
				indent?.Invoke(); stringBuilder.AppendLine( $"{appraisal.GetType().Name} -> {score:0.00} {okMark}" );
			}
		}
		level--;
		
		indent?.Invoke(); stringBuilder.AppendLine( "Sum:" );
		level++;
		foreach ( var appraisal in _optionalAppraisals ) {
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
