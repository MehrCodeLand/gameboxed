using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Leyer.Entities
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Navigation property
        public ICollection<GameRating> GameRatings { get; set; }
        public ICollection<FavoriteGame> FavoritedBy { get; set; }
    }
}
