using System;
using Pixpil.AI.UtilityAI;


namespace Pixpil.AI.UtilityAIScript {

    /// <summary>
    /// Factory for creating common appraisals
    /// </summary>
    public static class AppraisalFactory< TContext > {
        /// <summary>
        /// Create a constant appraisal that always returns the same value
        /// </summary>
        public static IAppraisal< TContext > Constant( float value ) {
            return new ActionAppraisal< TContext >( _ => value );
        }

        /// <summary>
        /// Create an appraisal that returns 1 if the condition is true, 0 otherwise
        /// </summary>
        public static IAppraisal< TContext > ConditionAppraisal( Func< TContext, bool > condition ) {
            return new ActionAppraisal< TContext >( ctx => condition( ctx ) ? 1.0f : 0.0f );
        }

        /// <summary>
        /// Create an appraisal that returns trueValue if the condition is true, falseValue otherwise
        /// </summary>
        public static IAppraisal< TContext > TernaryAppraisal(
            Func< TContext, bool > condition,
            float trueValue,
            float falseValue ) {
            return new ActionAppraisal< TContext >( ctx => condition( ctx ) ? trueValue : falseValue );
        }

        /// <summary>
        /// Create an appraisal that linearly maps a property value from an input range to an output range
        /// </summary>
        public static IAppraisal< TContext > LinearMap(
            Func< TContext, float > propertyGetter,
            float inMin,
            float inMax,
            float outMin = 0.0f,
            float outMax = 1.0f,
            bool clamp = true ) {
            return new ActionAppraisal< TContext >( ctx => {
                float value = propertyGetter( ctx );
                float t = ( value - inMin ) / ( inMax - inMin );

                if ( clamp ) t = Math.Max( 0, Math.Min( 1, t ) );

                return outMin + t * ( outMax - outMin );
            } );
        }

        /// <summary>
        /// Create an appraisal that applies a curve to the input property value
        /// </summary>
        public static IAppraisal< TContext > CurveMapped(
            Func< TContext, float > propertyGetter,
            Func< float, float > curve,
            bool clamp = true ) {
            return new ActionAppraisal< TContext >( ctx => {
                float value = propertyGetter( ctx );
                float result = curve( value );

                if ( clamp ) result = Math.Max( 0, Math.Min( 1, result ) );

                return result;
            } );
        }

        /// <summary>
        /// Create an appraisal that returns 1 if the property value is in the specified range, 0 otherwise
        /// </summary>
        public static IAppraisal< TContext > InRange(
            Func< TContext, float > propertyGetter,
            float min,
            float max ) {
            return new ActionAppraisal< TContext >( ctx => {
                float value = propertyGetter( ctx );
                return ( value >= min && value <= max ) ? 1.0f : 0.0f;
            } );
        }

        /// <summary>
        /// Create an appraisal that returns the sum of all child appraisals
        /// </summary>
        public static IAppraisal< TContext > Sum( params IAppraisal< TContext >[] appraisals ) {
            return new ActionAppraisal< TContext >( ctx => {
                float sum = 0;
                foreach ( var appraisal in appraisals ) {
                    sum += appraisal.GetScore( ctx );
                }

                return sum;
            } );
        }

        /// <summary>
        /// Create an appraisal that returns the product of all child appraisals
        /// </summary>
        public static IAppraisal< TContext > Product( params IAppraisal< TContext >[] appraisals ) {
            return new ActionAppraisal< TContext >( ctx => {
                float product = 1;
                foreach ( var appraisal in appraisals ) {
                    product *= appraisal.GetScore( ctx );
                }

                return product;
            } );
        }

        /// <summary>
        /// Create an appraisal that returns the minimum of all child appraisals
        /// </summary>
        public static IAppraisal< TContext > Min( params IAppraisal< TContext >[] appraisals ) {
            return new ActionAppraisal< TContext >( ctx => {
                if ( appraisals.Length == 0 ) return 0;

                float min = float.MaxValue;
                foreach ( var appraisal in appraisals ) {
                    min = Math.Min( min, appraisal.GetScore( ctx ) );
                }

                return min;
            } );
        }

        /// <summary>
        /// Create an appraisal that returns the maximum of all child appraisals
        /// </summary>
        public static IAppraisal< TContext > Max( params IAppraisal< TContext >[] appraisals ) {
            return new ActionAppraisal< TContext >( ctx => {
                if ( appraisals.Length == 0 ) return 0;

                float max = float.MinValue;
                foreach ( var appraisal in appraisals ) {
                    max = Math.Max( max, appraisal.GetScore( ctx ) );
                }

                return max;
            } );
        }

        /// <summary>
        /// Create an appraisal that returns the average of all child appraisals
        /// </summary>
        public static IAppraisal< TContext > Average( params IAppraisal< TContext >[] appraisals ) {
            return new ActionAppraisal< TContext >( ctx => {
                if ( appraisals.Length == 0 ) return 0;

                float sum = 0;
                foreach ( var appraisal in appraisals ) {
                    sum += appraisal.GetScore( ctx );
                }

                return sum / appraisals.Length;
            } );
        }
    }

}
