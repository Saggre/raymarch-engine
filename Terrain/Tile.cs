using Microsoft.Xna.Framework;

namespace EconSim.Terrain
{
    public class Tile
    {
        private Vector2 _position;
        private CardinalNeighborCollection<Tile> _neighbors;

        public CardinalNeighborCollection<Tile> Neighbors => _neighbors;

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public Tile(CardinalNeighborCollection<Tile> neighbors)
        {
            _neighbors = neighbors;
        }
    }
}