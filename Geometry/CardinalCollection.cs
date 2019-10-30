// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System.Collections;
using System.Collections.Generic;
using EconSim.Geometry.CardinalCollection;

namespace EconSim.Geometry
{
    public static class CardinalCollectionHelper
    {
        public static CardinalDirection Opposite(this CardinalDirection direction)
        {
            var newDir = ((int)direction + 2) % 4;
            return (CardinalDirection)newDir;
        }

        public static CornerDirection Opposite(this CornerDirection direction)
        {
            var newDir = ((int)direction + 2) % 4;
            return (CornerDirection)newDir;
        }
    }

    namespace CardinalCollection
    {

        public enum CardinalDirection
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3
        }

        public enum CornerDirection
        {
            UpperLeft = 0,
            UpperRight = 1,
            LowerRight = 2,
            LowerLeft = 3
        }

    }

    public class CardinalCollection<T> : IEnumerable<T>
    {
        private T north;
        private T south;
        private T east;
        private T west;

        public T North
        {
            get => north;
            set => north = value;
        }

        public T South
        {
            get => south;
            set => south = value;
        }

        public T East
        {
            get => east;
            set => east = value;
        }

        public T West
        {
            get => west;
            set => west = value;
        }

        /// <summary>
        /// Get a value based on its cardinal direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public T Get(CardinalDirection direction)
        {
            switch (direction)
            {
                case CardinalDirection.North:
                    return north;
                case CardinalDirection.South:
                    return south;
                case CardinalDirection.East:
                    return east;
                case CardinalDirection.West:
                    return west;
            }

            return North;
        }

        /// <summary>
        /// Get a value based on its corner direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public T Get(CornerDirection direction)
        {
            return Get((CardinalDirection)direction);
        }

        /// <summary>
        /// Set a value based on its cardinal direction
        /// </summary>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        public void Set(T value, CardinalDirection direction)
        {
            switch (direction)
            {
                case CardinalDirection.North:
                    north = value;
                    break;
                case CardinalDirection.South:
                    south = value;
                    break;
                case CardinalDirection.East:
                    east = value;
                    break; ;
                case CardinalDirection.West:
                    west = value;
                    break;
            }
        }

        /// <summary>
        /// Set a value based on its corner direction
        /// </summary>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        public void Set(T value, CornerDirection direction)
        {
            Set(value, (CardinalDirection)direction);
        }

        public CardinalCollection() { }

        /// <summary>
        /// Clockwise
        /// </summary>
        /// <param name="north"></param>
        /// <param name="east"></param>
        /// <param name="south"></param>
        /// <param name="west"></param>
        public CardinalCollection(T north, T east, T south, T west)
        {
            this.north = north;
            this.south = south;
            this.east = east;
            this.west = west;
        }

        /// <summary>
        /// How many directions have an item assigned
        /// </summary>
        /// <returns></returns>
        public int Count()
        {

            int count = 0;

            if (north != null)
            {
                count++;
            }

            if (east != null)
            {
                count++;
            }

            if (south != null)
            {
                count++;
            }

            if (west != null)
            {
                count++;
            }

            return count;

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (north != null)
            {
                yield return north;
            }

            if (east != null)
            {
                yield return east;
            }

            if (south != null)
            {
                yield return south;
            }

            if (west != null)
            {
                yield return west;
            }
        }
    }
}