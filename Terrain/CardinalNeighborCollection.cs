using System;

namespace EconSim.Terrain
{
    public class CardinalNeighborCollection<T>
    {
        public enum FourDirection
        {
            North = 0,
            South = 1,
            West = 2,
            East = 3
        }

        private T _north;
        private T _south;
        private T _east;
        private T _west;

        public T North => _north;

        public T South => _south;

        public T East => _east;

        public T West => _west;

        public CardinalNeighborCollection() { }

        public CardinalNeighborCollection(T north, T south, T east, T west)
        {
            _north = north;
            _south = south;
            _east = east;
            _west = west;
        }

        public void SetCardinalNeighbor(in T neighbor, in FourDirection direction)
        {
            switch (direction)
            {
                case FourDirection.North:
                    _north = neighbor;
                    break;
                case FourDirection.South:
                    _south = neighbor;
                    break;
                case FourDirection.East:
                    _east = neighbor;
                    break;
                case FourDirection.West:
                    _west = neighbor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

    }
}