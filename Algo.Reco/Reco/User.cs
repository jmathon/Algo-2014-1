using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Algo.Reco;

namespace Algo
{
    public partial class User
    {
        internal static string[] CellSeparator = new string[] { "::" };

        UInt16 _userId;
        byte _age;
        bool _male;
        string _occupation;
        string _zipCode;
        IReadOnlyList<BestFUser> _bestFUser = null;

        /// <summary>
        /// User information is in the file "users.dat" and is in the following
        /// format:
        /// UserID::Gender::Age::Occupation::Zip-code
        /// </summary>
        /// <param name="line"></param>
        User( string line )
        {
            string[] cells = line.Split( CellSeparator, StringSplitOptions.None );
            _userId = UInt16.Parse( cells[0] );
            _male = cells[1] == "M";
            _age = Byte.Parse( cells[2] );
            _occupation = String.Intern( cells[3] );
            _zipCode = String.Intern( cells[4] );
            Ratings = new Dictionary<Movie, int>();
        }

        static public User[] ReadUsers( string path )
        {
            List<User> u = new List<User>();
            using( TextReader r = File.OpenText( path ) )
            {
                string line;
                while( (line = r.ReadLine()) != null ) u.Add( new User( line ) );
            }
            return u.ToArray();
        }

        static public void ReadRatings( User[] users, Movie[] movies, string path )
        {
            using( TextReader r = File.OpenText( path ) )
            {
                string line;
                while( (line = r.ReadLine()) != null )
                {
                    string[] cells = line.Split( CellSeparator, StringSplitOptions.None );
                    int idUser = int.Parse( cells[0] );
                    int idMovie = int.Parse( cells[1] );
                    if( idMovie >= 0 && idMovie < movies.Length
                        && idUser >= 0 && idUser < users.Length )
                    {
                        users[idUser].Ratings.Add( movies[idMovie], int.Parse( cells[2] ) );
                    }
                }
            }
        }

        public int UserID { get { return (int)_userId; } set { _userId = (UInt16)value; } }

        public bool Male { get { return _male; } }

        public int Age { get { return (int)_age; } }

        public string Occupation { get { return _occupation; } }

        public string ZipCode { get { return _zipCode; } }

        public Dictionary<Movie, int> Ratings { get; private set; }

        class BestFUserComparer : IComparer<BestFUser>
        {
            public int Compare( BestFUser x, BestFUser y )
            {
                double delta = Math.Abs( x.Similarity ) - Math.Abs( y.Similarity );
                return delta > 0 ? 1 : delta < 0 ? -1 : 0;
            }
        }

        readonly static BestFUserComparer _fUserComparer = new BestFUserComparer();

        public IReadOnlyList<Movie> GetRecommendedMovies()
        {

        }

        public IReadOnlyList<BestFUser> GetBestFUsers( RecoContext ctx )
        {
            if( _bestFUser == null )
            {
                BestKeeper<BestFUser> bk = new BestKeeper<BestFUser>( 300, _fUserComparer );
                foreach( var u in ctx.Users )
                {
                    if( u != this )
                    {
                        double s = ctx.PearsonSimilarity( this, u );
                        bk.AddCandidate( new BestFUser( u, s ) );
                    }
                }
                _bestFUser = bk.GetBest();
            }
            return _bestFUser;
        }

        public double EuclidianSimilarityTo( User u )
        {
            return EuclidianSimilarity( this, u );
        }

        public static double EuclidianSimilarity( User u1, User u2 )
        {
            double d = EuclidianDistance( u1, u2 );
            if( Double.IsNaN( d ) ) return 0;
            return (1 / 1 + d);
        }

        public static double EuclidianDistance( User u1, User u2 )
        {
            double result = 0;
            var rating1 = u1.Ratings;
            var rating2 = u2.Ratings;

            int commonMovieCount = 0;
            foreach( var r1 in rating1 )
            {
                int note1 = 0;
                int note2 = 0;
                if( rating2.TryGetValue( r1.Key, out note2 ) )
                {
                    note1 = r1.Value;
                    result += Math.Pow( note1 - note2, 2 );
                    commonMovieCount++;
                }
            }
            if( commonMovieCount == 0 ) return Double.NaN;
            return Math.Sqrt( result );
        }
    }


}
