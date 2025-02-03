using Google.OrTools.ConstraintSolver;
using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.ORToolsSolvers2
{
    public class ORToolsLegacyCPSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Création du solveur de programmation par contraintes
            Solver solver = new Solver("SudokuSolver");

            // Définir les variables pour chaque cellule (9x9)
            IntVar[,] cells = new IntVar[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cells[i, j] = solver.MakeIntVar(1, 9, $"cell_{i}_{j}");
                }
            }

            // Ajouter des contraintes pour les lignes, colonnes et régions 3x3
            for (int i = 0; i < 9; i++)
            {
                // Contrainte pour chaque ligne : chaque cellule doit être unique
                solver.Add(cells[i, 0] + cells[i, 1] + cells[i, 2] + cells[i, 3] + cells[i, 4] + cells[i, 5] + cells[i, 6] + cells[i, 7] + cells[i, 8] == 45);
                solver.Add(solver.MakeAllDifferent(Enumerable.Range(0, 9).Select(j => cells[i, j]).ToArray()));
                // Contrainte pour chaque colonne : chaque cellule doit être unique
                solver.Add(cells[0, i] + cells[1, i] + cells[2, i] + cells[3, i] + cells[4, i] + cells[5, i] + cells[6, i] + cells[7, i] + cells[8, i] == 45);
                solver.Add(solver.MakeAllDifferent(Enumerable.Range(0, 9).Select(j => cells[j, i]).ToArray()));
            }

            // Ajouter des contraintes pour les régions 3x3
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    List<IntVar> boxCells = new List<IntVar>();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            boxCells.Add(cells[boxRow * 3 + i, boxCol * 3 + j]);
                        }
                    }
                    solver.Add(solver.MakeAllDifferent(boxCells.ToArray()));
                }
            }

            // Ajouter des contraintes pour les cases déjà remplies dans le Sudoku
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.Cells[i, j] != 0)
                    {
                        solver.Add(cells[i, j] == s.Cells[i, j]);
                    }
                }
            }

            // Utiliser le solveur pour résoudre le problème
            DecisionBuilder db = solver.MakePhase(cells.Cast<IntVar>().ToArray(), Solver.INT_VAR_SIMPLE, Solver.ASSIGN_MIN_VALUE);
            solver.NewSearch(db);

            // Si une solution est trouvée, mettre à jour la grille Sudoku
            if (solver.NextSolution())
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        s.Cells[i, j] = (int)cells[i, j].Value();
                    }
                }
            }
            else
            {
                // Si aucune solution n'a été trouvée, retourner une grille vide ou générer une exception
                throw new InvalidOperationException("No solution found for the Sudoku puzzle.");
            }

            solver.EndSearch();
            return s;
        }
    }
}