using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Reco
{
    public class BestKeeper<T>
    {
        readonly IComparer<T> _comparer;
        readonly int _count;
        readonly IList<T> _candidates;

        public BestKeeper( int count, IComparer<T> comparer )
        {
            if( count <= 1 ) throw new ArgumentException( "Must be greater than 0", "count" );
            if( comparer == null ) throw new ArgumentNullException( "comparer" );
            _comparer = comparer;
            _count = count;
            _candidates = new List<T>();
        }

        public void AddCandidate( T candidate )
        {
            _candidates.Add( candidate );
        }

        public IReadOnlyList<T> GetBest()
        {
            return _candidates.OrderBy( x => x, _comparer ).Take( _count ).ToArray();
        }
    }
}
