using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Algo
{
    public class RecoContext
    {
        struct KeyPair
        {
            public KeyPair( User u1, User u2 )
            {
                U1 = u1;
                U2 = u2;
            }

            public readonly User U1;
            public readonly User U2;

            public override bool Equals( object obj )
            {
                if( !(obj is KeyPair) ) return false;
                KeyPair o = (KeyPair)obj;
                return (U1 == o.U1 && U2 == o.U2) || (U1 == o.U2 && U2 == o.U1);
            }

            public override int GetHashCode()
            {
                return U1.UserID ^ U2.UserID;
            }
        }

        IDictionary<KeyPair, double> _cache;
        public User[] Users { get; private set; }
        public Movie[] Movies { get; private set; }

        public RecoContext()
        {
            _cache = new Dictionary<KeyPair, double>();
        }

        public void LoadFrom( string folder )
        {
            Users = User.ReadUsers( Path.Combine( folder, "users.dat" ) );
            Movies = Movie.ReadMovies( Path.Combine( folder, "movies.dat" ) );
            User.ReadRatings( Users, Movies, Path.Combine( folder, "ratings.dat" ) );
        }

        public double PearsonSimilarity( User u1, User u2 )
        {
            double result = 0;
            if( _cache.TryGetValue( new KeyPair( u1, u2 ), out result ) ) return result;
            result = DoComputePearsonSimilarity( u1, u2 );

            return result;
        }

        private double DoComputePearsonSimilarity( User u1, User u2 )
        {
            if( u1 == u2 ) return 1.0;

            IEnumerable<Movie> common = u1.Ratings.Keys.Intersect( u2.Ratings.Keys );
            int commonCount = common.Count();

            if( commonCount == 0 ) return 0;

            double sum1 = 0;
            double sum2 = 0;

            double sumSquare1 = 0;
            double sumSquare2 = 0;

            double sumProd = 0;

            foreach( var m in common )
            {
                int r1 = u1.Ratings[m];
                int r2 = u2.Ratings[m];

                sum1 += r1;
                sum2 += r2;
                sumProd += (r1 * r2);
                sumSquare1 += (r1 * r1);
                sumSquare2 += (r2 * r2);
            }

            var numerator = sumProd - (sum1 * sum2) / commonCount;
            var denominator = Math.Sqrt( (sumSquare1 - (sum1 * sum1) / commonCount) * (sumSquare2 - (sum2 * sum2) / commonCount) );

            if( denominator < Double.Epsilon ) return 0;

            double s = numerator / denominator;

            if( s < -1 ) return -1.0;
            if( s > 1 ) return 1.0;

            return s;
        }
    }
}
