using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Leyer.Entities;

public class GameRating
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; }

    public int? Rating { get; set; } // You can change this to a float if needed
    public DateTime RatedAt { get; set; }
}
