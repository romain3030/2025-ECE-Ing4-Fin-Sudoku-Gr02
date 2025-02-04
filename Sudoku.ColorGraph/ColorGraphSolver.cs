using Sudoku.Shared;
using System;
using System.Collections.Generic;

namespace Sudoku.FastSolver
{
    public class BacktrackingSolver : ISudokuSolver
    {
        private const int GRID_SIZE = 9;

        public SudokuGrid Solve(SudokuGrid grid)
        {
            int[,] sudoku = grid.Cells;

            if (SolveSudoku(sudoku))
            {
                SudokuGrid solvedGrid = new SudokuGrid();
                solvedGrid.Cells = sudoku;
                return solvedGrid;
            }
            else
            {
                Console.WriteLine("❌ Aucune solution trouvée !");
                return grid;
            }
        }

        private bool SolveSudoku(int[,] grid)
        {
            (int row, int col) = FindUnassignedCell(grid);
            if (row == -1) return true; // Grille complète

            for (int num = 1; num <= GRID_SIZE; num++)
            {
                if (IsSafe(grid, row, col, num))
                {
                    grid[row, col] = num;

                    if (SolveSudoku(grid))
                        return true;

                    grid[row, col] = 0; // Backtrack
                }
            }
            return false;
        }

        private (int, int) FindUnassignedCell(int[,] grid)
        {
            int minOptions = int.MaxValue;
            (int, int) bestCell = (-1, -1);

            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    if (grid[row, col] == 0)
                    {
                        int options = CountPossibleValues(grid, row, col);
                        if (options < minOptions)
                        {
                            minOptions = options;
                            bestCell = (row, col);
                        }
                    }
                }
            }
            return bestCell;
        }

        private int CountPossibleValues(int[,] grid, int row, int col)
        {
            bool[] possible = new bool[GRID_SIZE + 1];
            for (int num = 1; num <= GRID_SIZE; num++)
                possible[num] = true;

            for (int i = 0; i < GRID_SIZE; i++)
            {
                possible[grid[row, i]] = false; // Vérifie la ligne
                possible[grid[i, col]] = false; // Vérifie la colonne
            }

            int boxRowStart = (row / 3) * 3;
            int boxColStart = (col / 3) * 3;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    possible[grid[boxRowStart + i, boxColStart + j]] = false; // Vérifie le bloc

            int count = 0;
            for (int num = 1; num <= GRID_SIZE; num++)
                if (possible[num]) count++;

            return count;
        }

        private bool IsSafe(int[,] grid, int row, int col, int num)
        {
            for (int x = 0; x < GRID_SIZE; x++)
                if (grid[row, x] == num || grid[x, col] == num)
                    return false;

            int boxRowStart = (row / 3) * 3;
            int boxColStart = (col / 3) * 3;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (grid[boxRowStart + i, boxColStart + j] == num)
                        return false;

            return true;
        }
    }
}
