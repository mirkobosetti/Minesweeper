using System;
using UnityEngine;

public class Game : MonoBehaviour
{
	public int width = 16;
	public int height = 16;
	public int mineCount = 32;

	private Board board;
	private Cell[,] state;

	private bool gameOver;

	private void Awake()
	{
		board = GetComponentInChildren<Board>();
	}

	void Start()
	{
		NewGame();
	}

	private void NewGame()
	{
		state = new Cell[width, height];
		gameOver = false;

		GenerateCells();
		GenerateMines();
		GenerateNumbers();

		Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

		board.Draw(state);
	}

	private void GenerateCells()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Cell cell = new Cell()
				{
					position = new Vector3Int(x, y, 0),
					type = Cell.Type.Empty,
				};

				state[x, y] = cell;
				// state[x, y].revealed = true;
			}
		}
	}

	private void GenerateMines()
	{
		for (int mine = 0; mine < mineCount; mine++)
		{
			int x = 0;
			int y = 0;

			do
			{
				x = UnityEngine.Random.Range(0, width);
				y = UnityEngine.Random.Range(0, height);
			} while (state[x, y].type == Cell.Type.Mine);

			state[x, y].type = Cell.Type.Mine;
		}
	}

	private void GenerateNumbers()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				Cell cell = state[x, y];

				if (cell.type == Cell.Type.Mine) continue;


				cell.number = CountMines(x, y);

				if (cell.number > 0)
					cell.type = Cell.Type.Number;

				state[x, y] = cell;
			}
		}
	}

	private int CountMines(int cellX, int cellY)
	{
		int count = 0;

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0) continue;

				int checkedCellX = cellX + x;
				int checkedCellY = cellY + y;

				if (GetCellByPosition(checkedCellX, checkedCellY).type == Cell.Type.Mine) count++;
			}
		}

		return count;
	}

	private void Update()
	{
		if (gameOver) return;

		// right click for the flag
		if (Input.GetMouseButtonDown(1))
		{
			Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
			Cell cell = GetCellByPosition(cellPosition.x, cellPosition.y);

			if (cell.type == Cell.Type.Invalid || cell.revealed) return;

			cell.flagged = !cell.flagged;
			state[cellPosition.x, cellPosition.y] = cell;
			board.Draw(state);
		}

		// left click for reveal the cell
		else if (Input.GetMouseButtonDown(0))
		{
			Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
			Cell cell = GetCellByPosition(cellPosition.x, cellPosition.y);

			if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged) return;

			if (cell.type == Cell.Type.Empty) Flood(cell);

			//if (cell.type == Cell.Type.Mine) Explode(cell);

			cell.revealed = true;
			state[cellPosition.x, cellPosition.y] = cell;

			if (cell.type == Cell.Type.Mine) Explode(cell);

			board.Draw(state);
		}
	}

	private void Explode(Cell cell)
	{
		Debug.Log("Game Over");
		gameOver = true;

		cell.revealed = true;
		cell.exploded = true;

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				cell = state[x, y];

				if (cell.type == Cell.Type.Mine)
				{
					cell.revealed = true;
					state[x, y] = cell;
				}
			}
		}

		cell.exploded = true;
		state[cell.position.x, cell.position.y] = cell;
	}

	private void Flood(Cell cell)
	{
		if (cell.revealed || cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

		cell.revealed = true;
		state[cell.position.x, cell.position.y] = cell;

		if (cell.type == Cell.Type.Empty)
		{
			Flood(GetCellByPosition(cell.position.x - 1, cell.position.y));
			Flood(GetCellByPosition(cell.position.x + 1, cell.position.y));
			Flood(GetCellByPosition(cell.position.x, cell.position.y - 1));
			Flood(GetCellByPosition(cell.position.x, cell.position.y + 1));
		}
	}

	private Cell GetCellByPosition(int x, int y)
	{
		if (isValid(x, y)) return state[x, y];

		return new Cell()
		{
			type = Cell.Type.Invalid
		};
	}

	private bool isValid(int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height;
	}
}
