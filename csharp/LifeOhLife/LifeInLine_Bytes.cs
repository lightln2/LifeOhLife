﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// Uses upper-, center- and lower lines instead of temporary buffer
    /// </summary>
    public class LifeInLine_Bytes : LifeJourney
    {
        byte[] field = new byte[WIDTH * HEIGHT];

        byte[] upperLineSumOf2 = new byte[WIDTH];
        byte[] upperLineSumOf3 = new byte[WIDTH];
        byte[] centerLineSumOf2 = new byte[WIDTH];
        byte[] centerLineSumOf3 = new byte[WIDTH];
        byte[] lowerLineSumOf2 = new byte[WIDTH];
        byte[] lowerLineSumOf3 = new byte[WIDTH];

        public override bool Get(int x, int y) => field[y * WIDTH + x] == 1;

        public override void Set(int x, int y, bool value) => field[y * WIDTH + x] = (byte)(value ? 1 : 0);

        public override void Step()
        {
            int nextLineIndex = WIDTH;

            for (int x = 1; x < WIDTH - 1; x++)
            {
                centerLineSumOf2[x] = centerLineSumOf3[x] = 0;
                lowerLineSumOf2[x] = (byte)(field[nextLineIndex + x - 1] + field[nextLineIndex + x + 1]);
                lowerLineSumOf3[x] = (byte)(lowerLineSumOf2[x] + field[nextLineIndex + x]);
            }

            for (int y = 1; y < HEIGHT - 1; y++)
            {
                byte[] temp2 = upperLineSumOf2;
                byte[] temp3 = upperLineSumOf3;
                upperLineSumOf2 = centerLineSumOf2;
                upperLineSumOf3 = centerLineSumOf3;
                centerLineSumOf2 = lowerLineSumOf2;
                centerLineSumOf3 = lowerLineSumOf3;
                lowerLineSumOf2 = temp2;
                lowerLineSumOf3 = temp3;

                nextLineIndex += WIDTH;

                int left;
                int center = 0;
                int right = field[nextLineIndex + 1];
                for (int x = 1; x < WIDTH - 1; x++)
                {
                    left = center;
                    center = right;
                    right = field[nextLineIndex + x + 1];
                    lowerLineSumOf2[x] = (byte)(left + right);
                    lowerLineSumOf3[x] = (byte)(left + center + right);

                    int neighbours = upperLineSumOf3[x] + centerLineSumOf2[x] + lowerLineSumOf3[x];
                    bool alive = centerLineSumOf3[x] - centerLineSumOf2[x] == 1;
                    bool aliveAndTwoNeighbours = alive && neighbours == 2;

                    field[nextLineIndex - WIDTH + x] = (byte)(aliveAndTwoNeighbours | neighbours == 3 ? 1 : 0);
                }
            }
        }
    }

}
