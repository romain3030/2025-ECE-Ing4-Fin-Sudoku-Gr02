using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.PSOSolvers
{
    public class PSOSimpleSolver : ISudokuSolver
    {
        private const int PopulationSize = 50;
        private const int MaxIterations = 1000;
        private static readonly Random RandomGen = new Random();

        public SudokuGrid Solve(SudokuGrid grid)
        {
            List<Particle> swarm = InitializeSwarm(grid);
            Particle bestGlobal = swarm.OrderBy(p => p.Fitness).First();

            for (int iter = 0; iter < MaxIterations; iter++)
            {
                foreach (var particle in swarm)
                {
                    particle.UpdateVelocity(bestGlobal);
                    particle.UpdatePosition(grid);
                    particle.EvaluateFitness();

                    if (particle.Fitness == 0)
                    {
                        Console.WriteLine($"Solution found at iteration {iter}");
                        return particle.ToSudokuGrid();
                    }
                }

                bestGlobal = swarm.OrderBy(p => p.Fitness).First();
                if (bestGlobal.Fitness == 0)
                {
                    Console.WriteLine($"Solution found at iteration {iter}");
                    break;
                }

                Console.WriteLine($"Iteration {iter}: Best fitness = {bestGlobal.Fitness}");
            }

            Console.WriteLine("PSOSimpleSolver: " + MaxIterations + " iterations");
            return bestGlobal.ToSudokuGrid();
        }

        private List<Particle> InitializeSwarm(SudokuGrid grid)
        {
            List<Particle> swarm = new List<Particle>();
            for (int i = 0; i < PopulationSize; i++)
            {
                swarm.Add(new Particle(grid));
            }
            return swarm;
        }
    }

    public class Particle
    {
        public SudokuGrid Position { get; set; }
        public SudokuGrid BestPosition { get; set; }
        public double Fitness { get; set; }
        public double[,] Velocity { get; set; }
        private bool[,] IsImmutable { get; set; }
        private static readonly Random Random = new Random();

        public Particle(SudokuGrid grid)
        {
            Position = new SudokuGrid();
            BestPosition = new SudokuGrid();
            Velocity = new double[9, 9];
            IsImmutable = new bool[9, 9];
            Initialize(grid);
        }

        private void Initialize(SudokuGrid grid)
        {
            for (int i = 0; i < 9; i++)
            {
                var missing = Enumerable.Range(1, 9).Except(GetRowValues(grid, i)).ToList();
                for (int j = 0; j < 9; j++)
                {
                    if (grid.Cells[i, j] == 0)
                    {
                        Position.Cells[i, j] = missing.OrderBy(x => Random.Next()).First();
                        missing.Remove(Position.Cells[i, j]);
                        IsImmutable[i, j] = false;
                    }
                    else
                    {
                        Position.Cells[i, j] = grid.Cells[i, j];
                        IsImmutable[i, j] = true;
                    }
                    BestPosition.Cells[i, j] = Position.Cells[i, j];
                }
            }
            EvaluateFitness();
        }

        private IEnumerable<int> GetRowValues(SudokuGrid grid, int row)
        {
            return Enumerable.Range(0, 9).Select(col => grid.Cells[row, col]);
        }

        public void UpdateVelocity(Particle bestGlobal)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (!IsImmutable[i, j])
                    {
                        Velocity[i, j] = 0.3 * Velocity[i, j]
                            + 1.5 * Random.NextDouble() * (BestPosition.Cells[i, j] - Position.Cells[i, j])
                            + 1.5 * Random.NextDouble() * (bestGlobal.Position.Cells[i, j] - Position.Cells[i, j]);
                    }
                }
            }
        }

        public void UpdatePosition(SudokuGrid grid)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (!IsImmutable[i, j])
                    {
                        int newValue = (int)Math.Round(Position.Cells[i, j] + Velocity[i, j]);
                        newValue = Math.Max(1, Math.Min(9, newValue));

                        if (IsValidValue(grid, i, j, newValue))
                        {
                            Position.Cells[i, j] = newValue;
                        }
                        else
                        {
                            var possibleValues = Enumerable.Range(1, 9).Where(val => IsValidValue(Position, i, j, val)).ToList();
                            if (possibleValues.Any())
                                Position.Cells[i, j] = possibleValues[Random.Next(possibleValues.Count)];
                        }
                    }
                }
            }
            EvaluateFitness();
        }

        public void EvaluateFitness()
        {
            Fitness = CalculateConflicts(Position);
        }

        private double CalculateConflicts(SudokuGrid grid)
        {
            double conflicts = 0;

            // Vérifier les lignes
            for (int i = 0; i < 9; i++)
            {
                conflicts += 9 - GetRowValues(grid, i).Distinct().Count();
                conflicts += 9 - Enumerable.Range(0, 9).Select(col => grid.Cells[col, i]).Distinct().Count();
            }

            // Vérifier les régions 3x3
            for (int box = 0; box < 9; box++)
            {
                int rowStart = (box / 3) * 3;
                int colStart = (box % 3) * 3;
                conflicts += 9 - Enumerable.Range(0, 9)
                    .Select(index => grid.Cells[rowStart + index / 3, colStart + index % 3])
                    .Distinct().Count();
            }

            return conflicts;
        }

        private bool IsValidValue(SudokuGrid grid, int row, int col, int value)
        {
            if (Enumerable.Range(0, 9).Any(i => grid.Cells[row, i] == value || grid.Cells[i, col] == value))
                return false;

            int regionRow = (row / 3) * 3;
            int regionCol = (col / 3) * 3;
            return !Enumerable.Range(0, 9).Any(i => grid.Cells[regionRow + i / 3, regionCol + i % 3] == value);
        }

        public SudokuGrid ToSudokuGrid()
        {
            return Position;
        }
    }
}
