// Created by Sakri Koskimies (Github: Saggre) on 03/11/2019

using System;

namespace EconSim.Collections
{
    public static class Util
    {

        /// <summary>
        /// Iterate each element starting from (startX, startY) and continuing outwards in a spiral.
        /// Makes finding the closest tile of a certain type a lot faster.
        /// Winds counterclockwise.
        /// </summary>
        public static void ForEachHelical<T>(this T[,] array, int startX, int startY, Action<T> action)
        {
            /*
             *     ... 11 10
                7 7 7 7 6 10
                8 3 3 2 6 10
                8 4 . 1 6 10
                8 4 5 5 5 10
                8 9 9 9 9 9
             */

            // First element
            action(array[startX, startY]);

            int maxTurns = Math.Max(
                Math.Max(startX, startY),
                Math.Max(array.GetLength(0) - startX, array.GetLength(1) - startY)
                );

            int turns = 1;
            for (int t = 0; t < maxTurns; t++)
            {
                int awind = turns * 2 - 1;
                int xCoord, yCoord;

                // Check whether the coord is inside the plane and execute the action if it is
                void CheckAndExecute(int x, int y)
                {
                    if (x < array.GetLength(0) && y < array.GetLength(1) && x >= 0 && y >= 0)
                    {
                        action(array[x, y]);
                    }
                }

                // Bottom - iterate through the row horizontally
                for (int x = 0; x < awind; x++)
                {
                    xCoord = startX - turns + 2 + x;
                    yCoord = startY + turns - 1;

                    CheckAndExecute(xCoord, yCoord);
                }

                // Right - iterate through the column vertically
                for (int y = 0; y < awind; y++)
                {
                    xCoord = startX + turns;
                    yCoord = startY + turns - 2 - y;

                    CheckAndExecute(xCoord, yCoord);
                }

                // Top
                for (int x = 0; x < awind + 1; x++)
                {
                    xCoord = startX + turns - 1 - x;
                    yCoord = startY - turns;

                    CheckAndExecute(xCoord, yCoord);
                }

                // Left
                for (int y = 0; y < awind + 1; y++)
                {
                    xCoord = startX - turns;
                    yCoord = startY - turns + 1 + y;

                    CheckAndExecute(xCoord, yCoord);
                }

                turns++;
            }
        }
    }
}