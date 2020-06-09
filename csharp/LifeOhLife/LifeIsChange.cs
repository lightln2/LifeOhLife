using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOhLife
{
    /// <summary>
    /// only processes changed cells and their neighbours
    /// </summary>
    public class LifeIsChange : LifeJourney
    {
        byte[] currentField = new byte[WIDTH * HEIGHT];
        byte[] nextField = new byte[WIDTH * HEIGHT];
        List<int> currentChangedCells = new List<int>(WIDTH * HEIGHT);
        List<int> nextChangedCells = new List<int>(WIDTH * HEIGHT);
        List<int> processedCellsList = new List<int>(WIDTH * HEIGHT);
        bool[] processedCellsMap = new bool[WIDTH * HEIGHT];

        public override bool Get(int i, int j) => currentField[j * WIDTH + i] == 1;

        public override void Set(int i, int j, bool value)
        {
            if (value && (i == 0 || i == WIDTH - 1)) throw new ArgumentException("i");
            if (value && (j == 0 || j == HEIGHT - 1)) throw new ArgumentException("j");
            int cell = j * WIDTH + i;
            currentField[cell] = (byte) (value ? 1 : 0);
            currentChangedCells.Add(cell);
        }

        private void ProcessCell(int cell)
        {
            if (processedCellsMap[cell]) return;

            int x = cell % WIDTH;
            int y = cell / WIDTH;
            if (x == 0 || x == WIDTH - 1) return;
            if (y == 0 || y == HEIGHT - 1) return;

            processedCellsMap[cell] = true;
            processedCellsList.Add(cell);

            bool alive = currentField[cell] == 1;

            int neighbours =
                currentField[cell - WIDTH - 1] + currentField[cell - WIDTH] + currentField[cell - WIDTH + 1] +
                currentField[cell - 1] + currentField[cell + 1] +
                currentField[cell + WIDTH - 1] + currentField[cell + WIDTH] + currentField[cell + WIDTH + 1];
            bool nextAlive = (alive & (neighbours == 2 || neighbours == 3)) || (!alive && neighbours == 3);
         
            nextField[cell] = (byte)(nextAlive ? 1 : 0);

            if(nextField[cell] != currentField[cell]) nextChangedCells.Add(cell);
        }

        public override void Step()
        {
            foreach(int cell in currentChangedCells)
            {
                ProcessCell(cell - WIDTH - 1);
                ProcessCell(cell - WIDTH);
                ProcessCell(cell - WIDTH + 1);
                ProcessCell(cell - 1);
                ProcessCell(cell);
                ProcessCell(cell + 1);
                ProcessCell(cell + WIDTH - 1);
                ProcessCell(cell + WIDTH);
                ProcessCell(cell + WIDTH + 1);
            }

            foreach (int cell in processedCellsList) processedCellsMap[cell] = false;
            //Console.WriteLine($"Processed {processedCellsList.Count} cells because of {currentChangedCells.Count} changed cells");
            processedCellsList.Clear();

            List<int> tempCells = currentChangedCells;
            currentChangedCells = nextChangedCells;
            nextChangedCells = tempCells;

            byte[] tempField = currentField;
            currentField = nextField;
            nextField = tempField;

            nextChangedCells.Clear();
        }
    }
}
