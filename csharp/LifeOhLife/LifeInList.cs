using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// only processes live cells and their neighbours
    /// </summary>
    public class LifeInList : LifeJourney
    {
        List<int> liveCells = new List<int>();
        byte[] field = new byte[WIDTH * HEIGHT];
        List<int> nextLiveCells = new List<int>();
        List<int> processedCells = new List<int>();
        bool[] processed = new bool[WIDTH * HEIGHT];

        public override bool Get(int i, int j) => field[j * WIDTH + i] == 1;

        public override void Set(int i, int j, bool value)
        {
            if (value && (i == 0 || i == WIDTH - 1)) throw new ArgumentException("i");
            if (value && (j == 0 || j == HEIGHT - 1)) throw new ArgumentException("j");
            if (Get(i, j) == value) return;
            field[j * WIDTH + i] = (byte) (value? 1 : 0);
            if (value) liveCells.Add(j * WIDTH + i);
            else liveCells.Remove(j * WIDTH + i);
                //throw new InvalidOperationException("Deleting a cell is not available in this version");
        }

        public override void Clear()
        {
            foreach (int cell in liveCells) field[cell] = 0;
            liveCells.Clear();
        }

        private void ProcessLiveCell(int cell)
        {
            if (processed[cell]) return;
            processedCells.Add(cell);
            processed[cell] = true;

            int x = cell % WIDTH;
            int y = cell / WIDTH;
            if (x == 0 || x == WIDTH - 1) return;
            if (y == 0 || y == HEIGHT - 1) return;

            int neighbours =
                field[cell - WIDTH - 1] + field[cell - WIDTH] + field[cell - WIDTH + 1] +
                field[cell - 1] + field[cell + 1] +
                field[cell + WIDTH - 1] + field[cell + WIDTH] + field[cell + WIDTH + 1];
            if(neighbours == 2 || neighbours == 3)
            {
                nextLiveCells.Add(cell);
            }
        }

        private void ProcessDeadCell(int cell)
        {
            if (field[cell] == 1) return; // cell is alive, will be processed in the main loop

            if (processed[cell]) return; // already processed
            processedCells.Add(cell);
            processed[cell] = true;

            int x = cell % WIDTH;
            int y = cell / WIDTH;
            if (x == 0 || x == WIDTH - 1) return;
            if (y == 0 || y == HEIGHT - 1) return;

            int neighbours =
                field[cell - WIDTH - 1] + field[cell - WIDTH] + field[cell - WIDTH + 1] +
                field[cell - 1] + field[cell + 1] +
                field[cell + WIDTH - 1] + field[cell + WIDTH] + field[cell + WIDTH + 1];
            if (neighbours == 3)
            {
                nextLiveCells.Add(cell);
            }
        }

        public override void Step()
        {
            foreach(int cell in liveCells)
            {
                ProcessLiveCell(cell);

                ProcessDeadCell(cell - WIDTH - 1);
                ProcessDeadCell(cell - WIDTH);
                ProcessDeadCell(cell - WIDTH + 1);
                ProcessDeadCell(cell - 1);
                ProcessDeadCell(cell + 1);
                ProcessDeadCell(cell + WIDTH - 1);
                ProcessDeadCell(cell + WIDTH);
                ProcessDeadCell(cell + WIDTH + 1);
            }

            foreach (int cell in liveCells) field[cell] = 0;

            foreach (int cell in processedCells) processed[cell] = false;
            processedCells.Clear();

            foreach (int cell in nextLiveCells) field[cell] = 1;

            List<int> temp = liveCells;
            liveCells = nextLiveCells;
            nextLiveCells = temp;
            nextLiveCells.Clear();
        }
    }
}
