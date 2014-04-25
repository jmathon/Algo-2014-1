using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Reco
{
    /// <summary>
    /// Friend or foe
    /// </summary>
    public class BestFUser
    {
        public BestFUser( User u, double similarity )
        {
            FUser = u;
            Similarity = similarity;
        }

        public readonly User FUser;
        public readonly Double Similarity;
    }
}
