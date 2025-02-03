using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.PSO
{
    public class PSOSolver : ISudokuSolver
    {
        private const int PopulationSize = 100;
        private const int MaxIterations = 1000;
        private const double InertiaWeight = 0.729;
        private const double CognitiveWeight = 1.494;
        private const double SocialWeight = 1.494;
        private Random _random = new Random();

        public SudokuGrid Solve(SudokuGrid s)
        {
            List<Particle> swarm = InitializeSwarm(s);
            Particle globalBest = GetBestParticle(swarm);

            for (int iter = 0; iter < MaxIterations; iter++)
            {
                foreach (var particle in swarm)
                {
                    UpdateVelocity(particle, globalBest);
                    UpdatePosition(particle, s);
                    UpdateBestPosition(particle);
                }

                Particle bestInSwarm = GetBestParticle(swarm);
                if (bestInSwarm.BestFitness < globalBest.BestFitness)
                {
                    globalBest = bestInSwarm;
                }

                if (globalBest.BestFitness == 0) // Found a valid solution
                {
                    break;
                }
            }

            return globalBest.BestPosition;
        }

        private List<Particle> InitializeSwarm(SudokuGrid s)
        {
            List<Particle> swarm = new List<Particle>();
            for (int i = 0; i < PopulationSize; i++)
            {
                swarm.Add(new Particle(s, _random));
            }
            return swarm;
        }

        private Particle GetBestParticle(List<Particle> swarm)
        {
            return swarm.OrderBy(p => p.BestFitness).First();
        }

        private void UpdateVelocity(Particle particle, Particle globalBest)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (particle.IsFixed[i, j]) continue;

                    double r1 = _random.NextDouble();
                    double r2 = _random.NextDouble();
                    particle.Velocity[i, j] = InertiaWeight * particle.Velocity[i, j] +
                                              CognitiveWeight * r1 * (particle.BestPosition.Cells[i, j] - particle.CurrentPosition.Cells[i, j]) +
                                              SocialWeight * r2 * (globalBest.BestPosition.Cells[i, j] - particle.CurrentPosition.Cells[i, j]);
                }
            }
        }

        private void UpdatePosition(Particle particle, SudokuGrid s)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (!particle.IsFixed[i, j])
                    {
                        int newValue = (int)Math.Round(particle.CurrentPosition.Cells[i, j] + particle.Velocity[i, j]);
                        particle.CurrentPosition.Cells[i, j] = Math.Clamp(newValue, 1, 9);
                    }
                }
            }
            particle.CurrentFitness = ComputeFitness(particle.CurrentPosition);
        }

        private void UpdateBestPosition(Particle particle)
        {
            if (particle.CurrentFitness < particle.BestFitness)
            {
                particle.BestPosition = new SudokuGrid { Cells = (int[,])particle.CurrentPosition.Cells.Clone() };
                particle.BestFitness = particle.CurrentFitness;
            }
        }

        private int ComputeFitness(SudokuGrid s)
        {
            int conflicts = 0;
            for (int i = 0; i < 9; i++)
            {
                conflicts += CountConflicts(s, i, true);
                conflicts += CountConflicts(s, i, false);
            }
            return conflicts;
        }

        private int CountConflicts(SudokuGrid s, int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            int conflicts = 0;
            for (int i = 0; i < 9; i++)
            {
                int value = isRow ? s.Cells[index, i] : s.Cells[i, index];
                if (value != 0 && !seen.Add(value))
                {
                    conflicts++;
                }
            }
            return conflicts;
        }
    }

    public class Particle
    {
        public SudokuGrid CurrentPosition { get; set; }
        public SudokuGrid BestPosition { get; set; }
        public double[,] Velocity { get; set; }
        public int CurrentFitness { get; set; }
        public int BestFitness { get; set; }
        public bool[,] IsFixed { get; set; }

        public Particle(SudokuGrid initialGrid, Random random)
        {
            CurrentPosition = new SudokuGrid { Cells = (int[,])initialGrid.Cells.Clone() };
            BestPosition = new SudokuGrid { Cells = (int[,])initialGrid.Cells.Clone() };
            Velocity = new double[9, 9];
            IsFixed = new bool[9, 9];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (CurrentPosition.Cells[i, j] != 0)
                    {
                        IsFixed[i, j] = true;
                    }
                    else
                    {
                        CurrentPosition.Cells[i, j] = random.Next(1, 10);
                    }
                    Velocity[i, j] = random.NextDouble() * 2 - 1;
                }
            }

            CurrentFitness = ComputeFitness();
            BestFitness = CurrentFitness;
        }

        private int ComputeFitness()
        {
            int conflicts = 0;
            for (int i = 0; i < 9; i++)
            {
                conflicts += CountConflicts(i, true);
                conflicts += CountConflicts(i, false);
            }
            return conflicts;
        }

        private int CountConflicts(int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            int conflicts = 0;
            for (int i = 0; i < 9; i++)
            {
                int value = isRow ? CurrentPosition.Cells[index, i] : CurrentPosition.Cells[i, index];
                if (value != 0 && !seen.Add(value))
                {
                    conflicts++;
                }
            }
            return conflicts;
        }
    }
}
