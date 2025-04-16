using System.Collections.Immutable;


namespace Pixpil.AI.UtilityAI {

	/// <summary>
	/// the root of UtilityAI.
	/// </summary>
	public abstract class Reasoner< T > {

		public IConsideration< T > DefaultConsideration { get; internal set; } = new FixedScoreConsideration< T >();

		protected ImmutableArray< IConsideration< T > > _considerations = ImmutableArray< IConsideration< T > >.Empty;


		public IAction< T > Select( T context ) {
			var consideration = SelectBestConsideration( context );
			if ( consideration != null ) {
				return consideration.Action;
			}

			return null;
		}


		protected abstract IConsideration< T > SelectBestConsideration( T context );


		public Reasoner< T > AddConsideration( IConsideration< T > consideration ) {
			_considerations = _considerations.Add( consideration );
			return this;
		}


		public Reasoner< T > SetDefaultConsideration( IConsideration< T > defaultConsideration ) {
			DefaultConsideration = defaultConsideration;
			return this;
		}
		
#if DEBUG
		protected string DebugTypeName() {
			var input = GetType().Name;
			if ( input.EndsWith( "Reasoner`1", StringComparison.OrdinalIgnoreCase ) ) {
				return input.Substring( 0, input.Length - "Reasoner`1".Length ).TrimEnd();
			}

			return input;
		}
		
		public string DebugDump( System.Text.StringBuilder stringBuilder = null ) {
			var builder = stringBuilder ?? new System.Text.StringBuilder();

			var level = 0;
			void PrintIndent() {
				for ( var i = 0; i < level * 2; i++ ) {
					builder.Append( " " );
				}
			}
			
			builder.AppendLine( $"{DebugTypeName()}" );
			level++;
				if ( DefaultConsideration is BaseConsideration< T > baseConsideration ) {
					PrintIndent(); builder.AppendLine( $"default:" );
					baseConsideration.DebugDump( builder, PrintIndent, ref level );
				}
			level--;
				
				builder.AppendLine();
				
				ImmutableArray< IConsideration< T > > considerations = _considerations;
				if ( this is HighestScoreReasoner< T > ) {
					considerations = _considerations.Sort(
						( ca, cb ) => {
							if ( ca is BaseConsideration< T > cab &&
								 cb is BaseConsideration< T > cbb ) {
								return cab.TryGetCachedResultScore() > cbb.TryGetCachedResultScore() ? -1 : 1;
							}
							
							return 0;
						} );
				}
				else if ( this is LowestScoreReasoner< T > ) {
					considerations = _considerations.Sort(
						( ca, cb ) => {
							if ( ca is BaseConsideration< T > cab &&
								 cb is BaseConsideration< T > cbb ) {
								return cab.TryGetCachedResultScore() > cbb.TryGetCachedResultScore() ? 1 : -1;
							}
							
							return 0;
					} );
				}

				foreach ( var consideration in considerations ) {
					if ( consideration == DefaultConsideration ) {
						continue;
					}
					
					if ( consideration is BaseConsideration< T > baseConsiderationSub ) {
						baseConsiderationSub.DebugDump( builder, PrintIndent, ref level );
					}
					
					builder.AppendLine();
				}
				
			level--;
			
			return builder.ToString();
		}
#endif
		
	}

}
