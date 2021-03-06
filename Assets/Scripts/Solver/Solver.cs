﻿using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class BoardState{
	public Vector2 move;
	public SolverBoard board;
	public int parent;
}

public class SolverClass {
	private ManualResetEvent _doneEvent;


	public string matrix;
	public int length;
	
	public int minMoves = -1;
	public string solution = "";
	
	public bool solved = false;
	public bool externalSolved = false;
	public int externalMin = 10000;

	int nextLevel = 1;
	int currentLevel = 0;
	public List<List<BoardState>> statesByLevel = null;//new List<List<BoardState>>();
	
	public SolverClass(string _matrix, ManualResetEvent mre = null){
		matrix = _matrix;
		length = Mathf.CeilToInt(Mathf.Sqrt(_matrix.Length));

		statesByLevel = new List<List<BoardState>>();

		_doneEvent = mre;
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
	
	public void Solve(int cenas = 0) {
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
		
		//		List<Thread> pool = new List<Thread>();
		//		for(int i = 0 ; i < 8 ; ++i){
		//			pool.Add(new Thread());
		//		}
		
		while(!solved){
			for(int b = 0 ; b < statesByLevel[currentLevel].Count ; ++b){
				SolverBoard currentBoard = statesByLevel[currentLevel][b].board;
				
				SolveCurrentBoard(currentBoard, b);

				if(externalSolved){
					_doneEvent.Set();
					return;
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
		_doneEvent.Set();
	}
	
	public void SolveCurrentBoard(SolverBoard currentBoard, int b){
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

					minMoves = statesByLevel.Count -1;
					
							
					BoardState b2 = statesByLevel[nextLevel][statesByLevel[nextLevel].Count-1];
					int level = minMoves;

					while (b2.parent != -1){
						solution = "(" + b2.move.x + "," + b2.move.y + ")" + solution;
						level--;
						b2 = statesByLevel[level][b2.parent];
					}

//					Debug.Log("Solution " + (nSolutions+1) + ": " + solution);

//					
					_doneEvent.Set();
					return;
				}
			}
		}
	}
	
	public void PrintResult(){
		
	}
}

public class Solver : MonoBehaviour{
	// Use this for initialization
	void Start () {
		//		string m = "1000000011100001100100000000000010100000011000001000010001110010";//8x8
		string m = "001101110010010100101100001000000010";//6x6
//		string m = "1110000101100100";//4x4
		SolverBoard b = new SolverBoard(m);
		
		Debug.Log(b.ToString());

		SolverClass solver = new SolverClass(m);
//		List<Thread> threads = new List<Thread>();

		List<SolverClass> solvers = new List<SolverClass>();
		ManualResetEvent[] doneEvents = new ManualResetEvent[m.Length];


		int ac = 0;
		for(int x = 0 ; x < solver.length ; ++x){
			for(int y = 0 ; y < solver.length ; ++y){
				b.ApplyMove(x,y);

				doneEvents[ac] = new ManualResetEvent(false);

				solvers.Add(new SolverClass(b.matrix_str, doneEvents[ac]));

//				threads.Add(new Thread(() => solvers[solvers.Count-1].Solve()));

				ThreadPool.QueueUserWorkItem((object a)=>solvers[solvers.Count-1].Solve(0));

				ac++;
			}
		}

		bool waitALittleMore = false;

		int min = 1000000;

		do{
			WaitHandle.WaitAll(doneEvents);

			for(int i = 0 ; i < solvers.Count ; ++i){
				if(solvers[i].solved){
					if(solvers[i].minMoves < min){
						min = solvers[i].minMoves;
//						solvers[i].
					}
				}
			}

			waitALittleMore = false;

			for(int i = 0 ; i < solvers.Count && !waitALittleMore ; ++i){
				if(solvers[i].statesByLevel.Count -1 < min){
					waitALittleMore = true;
				}
			}
		}while(waitALittleMore);

		for(int i = 0 ; i < solvers.Count ; ++i){
			solvers[i].externalSolved = true;
		}

		Debug.Log("Ended");
		Debug.Log("Min = " + min);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
