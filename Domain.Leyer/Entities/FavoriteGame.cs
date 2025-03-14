using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Leyer.Entities
{
    public class FavoriteGame
    {
        // Composite Key: UserId + GameId (configured in DbContext)
        public int UserId { get; set; }
        public User User { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }
    }
}
