using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	public int chunkCountX = 4, chunkCountZ = 3;
	private int cellCountX = 6, cellCountZ = 6;

	public HexCell cellPrefab;
	private HexCell[] cells;

	public Text labelPrefab;

	public HexGridChunk chunkPrefab;
	private HexGridChunk[] chunks;

	public Color defaultColor = Color.white;

	public Texture2D noiseSource;

	private void Awake() {
		HexMetrics.noiseSource = noiseSource;

		cellCountX = chunkCountX * HexMetrics.CHUNK_SIZE_X;
		cellCountZ = chunkCountZ * HexMetrics.CHUNK_SIZE_Z;

		CreateChunks();
		CreateCell();
	}

	private void OnEnable() {
		HexMetrics.noiseSource = noiseSource;
	}

	private void CreateChunks() {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	private void CreateCell() {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	private void CreateCell(int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.INNER_RADIUS * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.OUTER_RADIUS * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.Color = defaultColor;

		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
			} else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
		}

		Text label = Instantiate<Text>(labelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();

		cell.uiRect = label.rectTransform;
		cell.Elevation = 0;
		
		AddCellToChunk(x, z, cell);
	}

	private void AddCellToChunk(int x, int z, HexCell cell) {
		int chunkX = x / HexMetrics.CHUNK_SIZE_X;
		int chunkZ = z / HexMetrics.CHUNK_SIZE_Z;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];
		
		int localX = x - chunkX * HexMetrics.CHUNK_SIZE_X;
		int localZ = z - chunkZ * HexMetrics.CHUNK_SIZE_Z;
		chunk.AddCell(localX + localZ * HexMetrics.CHUNK_SIZE_X, cell);
	}
	
	public HexCell GetCell(Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		return cells[index];
	}
	
	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		
		return cells[x + z * cellCountX];
	}
}
