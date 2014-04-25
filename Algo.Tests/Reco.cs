using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace Algo.Tests
{
    [TestFixture]
    public class Reco
    {
        static string _badDataPath = Path.Combine( TestHelper.SolutionFolder, @"ThirdParty\MovieData\MovieLens\" );
        static string _goodDataPath = Path.Combine( TestHelper.SolutionFolder, @"ThirdParty\MovieData\" );

        [Test]
        public void CorrectData()
        {
            Dictionary<int, Movie> firstMovies;
            Dictionary<int, List<Movie>> duplicateMovies;
            Movie.ReadMovies( Path.Combine( _badDataPath, "movies.dat" ), out firstMovies, out duplicateMovies );
            int idMovieMin = firstMovies.Keys.Min();
            int idMovieMax = firstMovies.Keys.Max();
            Console.WriteLine( "{3} Movies from {0} to {1}, {2} duplicates.", idMovieMin, idMovieMax, duplicateMovies.Count, firstMovies.Count );

            Dictionary<int, User> firstUsers;
            Dictionary<int, List<User>> duplicateUsers;
            User.ReadUsers( Path.Combine( _badDataPath, "users.dat" ), out firstUsers, out duplicateUsers );
            int idUserMin = firstUsers.Keys.Min();
            int idUserMax = firstUsers.Keys.Max();
            Console.WriteLine( "{3} Users from {0} to {1}, {2} duplicates.", idUserMin, idUserMax, duplicateUsers.Count, firstUsers.Count );

            Dictionary<int,string> badLines;
            int nbRating = User.ReadRatings( Path.Combine( _badDataPath, "ratings.dat" ), firstUsers, firstMovies, out badLines );
            Console.WriteLine( "{0} Ratings: {1} bad lines.", nbRating, badLines.Count );

            Directory.CreateDirectory( _goodDataPath );
            // Saves Movies
            using( TextWriter w = File.CreateText( Path.Combine( _goodDataPath, "movies.dat" ) ) )
            {
                int idMovie = 0;
                foreach( Movie m in firstMovies.Values )
                {
                    m.MovieID = ++idMovie;
                    w.WriteLine( "{0}::{1}::{2}", m.MovieID, m.Title, String.Join( "|", m.Categories ) );
                }
            }

            // Saves Users
            string[] occupations = new string[]{
                "other", 
                "academic/educator", 
                "artist", 
                "clerical/admin",
                "college/grad student",
                "customer service",
                "doctor/health care",
                "executive/managerial",
                "farmer",
                "homemaker",
                "K-12 student",
                "lawyer",
                "programmer",
                "retired",
                "sales/marketing",
                "scientist",
                "self-employed",
                "technician/engineer",
                "tradesman/craftsman",
                "unemployed",
                "writer" };
            using( TextWriter w = File.CreateText( Path.Combine( _goodDataPath, "users.dat" ) ) )
            {
                int idUser = 0;
                foreach( User u in firstUsers.Values )
                {
                    u.UserID = ++idUser;
                    string occupation;
                    int idOccupation;
                    if( int.TryParse( u.Occupation, out idOccupation )
                        && idOccupation >= 0
                        && idOccupation < occupations.Length )
                    {
                        occupation = occupations[idOccupation];
                    }
                    else occupation = occupations[0];
                    w.WriteLine( "{0}::{1}::{2}::{3}::{4}", u.UserID, u.Male ? 'M' : 'F', u.Age, occupation, "US-" + u.ZipCode );
                }
            }
            // Saves Rating
            using( TextWriter w = File.CreateText( Path.Combine( _goodDataPath, "ratings.dat" ) ) )
            {
                foreach( User u in firstUsers.Values )
                {
                    foreach( var r in u.Ratings )
                    {
                        w.WriteLine( "{0}::{1}::{2}", u.UserID, r.Key.MovieID, r.Value );
                    }
                }
            }
        }

        [Test]
        public void ReadMovieData()
        {
            RecoContext c = new RecoContext();
            c.LoadFrom( _goodDataPath );
            for( int i = 0; i < c.Users.Length; ++i )
                Assert.That( c.Users[i].UserID, Is.EqualTo( i + 1 ) );
            for( int i = 0; i < c.Movies.Length; ++i )
                Assert.That( c.Movies[i].MovieID, Is.EqualTo( i + 1 ) );
        }

        [Test]
        public void TestEuclidianDistance()
        {
            var c = new RecoContext();
            c.LoadFrom( _goodDataPath );
            var u1 = c.Users[0];
            var u2 = c.Users[1];
            var u3 = c.Users[2];

            Assert.That( User.EuclidianDistance( u1, u1 ), Is.EqualTo( Double.NaN ) );
            Assert.That( User.EuclidianDistance( u2, u2 ), Is.EqualTo( 0 ) );
            Assert.That( User.EuclidianDistance( u3, u3 ), Is.EqualTo( 0 ) );
        }

        [Test]
        public void TestPearsonCorrelation()
        {
            var c = new RecoContext();
            c.LoadFrom( _goodDataPath );
            foreach( var u1 in c.Users )
            {
                foreach( var u2 in c.Users )
                {
                    double s = c.PearsonSimilarity( u1, u2 );
                    Assert.That( s >= -1 && s <= 1 );
                    //Assert.That( u1 != u2 || s == 1, "u1 == u2 => s == 1" );
                    Assert.That( s == c.PearsonSimilarity( u2, u1 ) );
                }
            }
        }
    }
}
