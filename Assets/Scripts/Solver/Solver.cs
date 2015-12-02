﻿using UnityEngine;
using System.Collections.Generic;

public class BoardState{
	public Vector2 move;
	public SolverBoard board;
	public int parent;
}

public class Solver : MonoBehaviour {
    string matrix;
    int length;

	public int minMoves = -1;

	bool solved = false;
	int nextLevel = 1;
	int currentLevel = 0;
	List<List<BoardState>> statesByLevel = new List<List<BoardState>>();

	public Solver(string _matrix){
		matrix = _matrix;
		length = Mathf.CeilToInt(Mathf.Sqrt(_matrix.Length));
	}

	void AddLevel(){
		statesByLevel.Add(new List<BoardState>());
		currentLevel++;
		nextLevel++;
	}

	void AddBoardToSolve(SolverBoard b, int x, int y, int _parent){
		statesByLevel[nextLevel].Add(new BoardState{
			move = new Vector2(x,y), 
			board = b,
			parent = _parent
		});
	}

    public void Solve() {
		SolverBoard board = new SolverBoard(matrix);

		statesByLevel = new List<List<BoardState>>();
		statesByLevel.Add(new List<BoardState>());
		statesByLevel.Add(new List<BoardState>());
		nextLevel = 1;
		currentLevel = 0;

		statesByLevel[currentLevel].Add(new BoardState{
			move = new Vector2(0,0), 
			board = board,
			parent = -1				
		});

		while(!solved){
			for(int b = 0 ; b < statesByLevel[currentLevel].Count ; ++b){
				SolverBoard currentBoard = statesByLevel[currentLevel][b].board;

				for(int x = currentBoard.minX; x < currentBoard.maxX; ++x) {
					for(int y = currentBoard.minY; y < currentBoard.maxY; ++y) {
						SolverBoard aux = new SolverBoard(currentBoard);
						
						aux.ApplyMove(x,y);

						if(statesByLevel.Count > 2){
							//int grandFather = statesByLevel[currentLevel][b].parent
							bool exist = false;
							for(int i = 0 ; i < statesByLevel.Count-1 && !exist ; ++i){
								for(int j = 0 ; j < statesByLevel[i].Count && !exist ; ++j){
									if(SolverBoard.Compare(aux, statesByLevel[i][j].board))
										exist = true;									
								}
							}

							if(!exist)
								AddBoardToSolve(aux, x, y, b);
						}
						else{
							AddBoardToSolve(aux, x, y, b);
						}

//						AddBoardToSolve(aux, x, y, b);

						if(aux.solved){
							//PUZZLE SOLVED!!!
							solved = true;
						}
					}
				}
			}

			if(!solved){				
				AddLevel();
			}
		}

		minMoves = statesByLevel.Count -1;

		int nSolutions = 0; 
		for(int i = 0 ; i < statesByLevel[minMoves].Count ; ++i){
			if(statesByLevel[minMoves][i].board.solved){
				nSolutions++;

				BoardState b = statesByLevel[minMoves][i];
				int level = minMoves;
				string solution = "";
				while (b.parent != -1){
					solution = "(" + b.move.x + "," + b.move.y + ")" + solution;
					level--;
					b = statesByLevel[level][b.parent];
				}

				Debug.Log("Solution " + (nSolutions+1) + ": " + solution);
			}
		}

		Debug.Log("Min Moves: " + minMoves) ;
		Debug.Log("NSolutions: " + nSolutions) ;

//        _min = 0;
//        _moves = "";
    }

	public void PrintResult(){

	}
    
    // Use this for initialization
	void Start () {
//		string m = "001101110010010100101100001000000010";
		string m = "1110000101100100";
		SolverBoard b = new SolverBoard(m);

		Debug.Log(b.ToString());

		matrix = m;
		length = Mathf.CeilToInt(Mathf.Sqrt(m.Length));

		statesByLevel = new List<List<BoardState>>();

		Solve();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
