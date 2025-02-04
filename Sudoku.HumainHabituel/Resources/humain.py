import numpy as np
from timeit import default_timer
import sys
sys.stdout.reconfigure(encoding='utf-8')


if 'instance' not in locals():
    instance = np.array([
        [0,0,0,0,9,4,0,3,0],
        [0,0,0,5,1,0,0,0,7],
        [0,8,9,0,0,0,0,4,0],
        [0,0,0,0,0,0,2,0,8],
        [0,6,0,2,0,1,0,5,0],
        [1,0,2,0,0,0,0,0,0],
        [0,7,0,0,0,0,5,2,0],
        [9,0,0,0,6,5,0,0,0],
        [0,4,0,9,7,0,0,0,0]
    ], dtype=int)


class SudokuSolver:
    def __init__(self, puzzle):
        self.puzzle = puzzle.copy()
        self.solved = False

    def get_possible_values(self, row, col):
        
        if self.puzzle[row, col] != 0:
            return []
       
        used = set(self.puzzle[row, :]) 
        used.update(self.puzzle[:, col])  
       
       
        start_row, start_col = 3 * (row // 3), 3 * (col // 3)
        used.update(self.puzzle[start_row:start_row+3, start_col:start_col+3].flatten())
        return [num for num in range(1, 10) if num not in used]

    def mrv_heuristic(self):
       
        min_possible = float('inf')
        target = None
        for row in range(9):
            for col in range(9):
                if self.puzzle[row, col] == 0:
                    possible = self.get_possible_values(row, col)
                    if len(possible) < min_possible:
                        min_possible = len(possible)
                        target = (row, col)
        return target

    def lcv_heuristic(self, row, col):
       
        possible = self.get_possible_values(row, col)
        if not possible:
            return []
        constraints = []
        for val in possible:
            count = 0
           
            for c in range(9):  # Ligne
                if c != col and self.puzzle[row, c] == 0 and val in self.get_possible_values(row, c):
                    count += 1
            for r in range(9):  # Colonne
                if r != row and self.puzzle[r, col] == 0 and val in self.get_possible_values(r, col):
                    count += 1
            start_r, start_c = 3*(row//3), 3*(col//3)  # Carré
            for r in range(start_r, start_r+3):
                for c in range(start_c, start_c+3):
                    if (r != row or c != col) and self.puzzle[r, c] == 0 and val in self.get_possible_values(r, c):
                        count += 1
            constraints.append((val, count))
       
        constraints.sort(key=lambda x: x[1])
        return [val for val, _ in constraints]

    def naked_single(self):
        
        progress = False
        for row in range(9):
            for col in range(9):
                if self.puzzle[row, col] == 0:
                    possible = self.get_possible_values(row, col)
                    if len(possible) == 1:
                        self.puzzle[row, col] = possible[0]
                        progress = True
        return progress

    def solve_sudoku(self):
       
       
        while self.naked_single():
            pass
        if np.all(self.puzzle != 0): 
            self.solved = True
            return True
        
        cell = self.mrv_heuristic()
        if not cell:
            return False 
        row, col = cell
        
        for val in self.lcv_heuristic(row, col):
            if val not in self.get_possible_values(row, col):
                continue  
            saved_puzzle = self.puzzle.copy()
            self.puzzle[row, col] = val
            if self.solve_sudoku():
                return True
            self.puzzle = saved_puzzle 
        return False
    

    def solve(self):
       
        start = default_timer()
        if self.solve_sudoku():
            print("Solution trouvée:")
            #print(self.puzzle)
        else:
            print("Aucune solution trouvée.")
        execution = default_timer() - start
        print(f"Le temps de résolution est de : {execution * 1000:.2f} ms")


solver = SudokuSolver(instance)
solver.solve()
result = solver.puzzle