﻿using UnityEngine;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;
	[SerializeField] private HexCell[] neighbors = new HexCell[6];

	public Vector3 Position {
		get { return transform.localPosition; }
	}

	private int elevation = int.MinValue;

	public int Elevation {
		get { return elevation; }
		set {
			if (elevation == value) {
				return;
			}

			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.ELEVATION_STEP;
			position.y +=
				(HexMetrics.SampleNoise(position).y * 2f - 1f)
				* HexMetrics.ELEVATION_PERTURB_STRENGTH;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;

			if (
				hasOutgoingRiver &&
				elevation < GetNeighbor(outgoingRiver).elevation
			) {
				RemoveOutgoingRiver();
			}
			if (
				hasIncomingRiver &&
				elevation > GetNeighbor(incomingRiver).elevation
			) {
				RemoveIncomingRiver();
			}

			Refresh();
		}
	}

	private Color color;

	public Color Color {
		get { return color; }
		set {
			if (color == value) {
				return;
			}
			color = value;
			Refresh();
		}
	}

	public RectTransform uiRect;

	public HexGridChunk chunk;

	private bool hasIncomingRiver, hasOutgoingRiver;
	private HexDirection incomingRiver, outgoingRiver;

	public bool HasIncomingRiver {
		get { return hasIncomingRiver; }
	}

	public bool HasOutgoingRiver {
		get { return hasOutgoingRiver; }
	}

	public HexDirection IncomingRiver {
		get { return incomingRiver; }
	}

	public HexDirection OutgoingRiver {
		get { return outgoingRiver; }
	}

	public bool HasRiver {
		get { return HasIncomingRiver || HasOutgoingRiver; }
	}

	public bool HasRiverBeginOrEnd {
		get { return HasIncomingRiver != HasOutgoingRiver; }
	}

	public bool HasRiverThroughEdge(HexDirection direction) {
		return
			hasIncomingRiver && incomingRiver == direction ||
			hasOutgoingRiver && outgoingRiver == direction;
	}

	public void RemoveRiver() {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}

	public void RemoveOutgoingRiver() {
		if (!hasOutgoingRiver) return;

		hasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(outgoingRiver);
		neighbor.hasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveIncomingRiver() {
		if (!hasIncomingRiver) return;

		hasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(incomingRiver);
		neighbor.hasOutgoingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void SetOutgoingRiver(HexDirection direction) {
		if (hasOutgoingRiver && outgoingRiver == direction) return;

		HexCell neighbor = GetNeighbor(direction);
		if (!neighbor || elevation < neighbor.elevation) return;

		RemoveOutgoingRiver();
		if (hasIncomingRiver && incomingRiver == direction) {
			RemoveIncomingRiver();
		}

		hasOutgoingRiver = true;
		outgoingRiver = direction;
		RefreshSelfOnly();

		neighbor.RemoveIncomingRiver();
		neighbor.hasIncomingRiver = true;
		neighbor.incomingRiver = direction.Opposite();
		neighbor.RefreshSelfOnly();
	}

	private void Refresh() {
		if (!chunk) return;

		chunk.Refresh();
		foreach (HexCell neighbor in neighbors) {
			if (neighbor != null && neighbor.chunk != chunk) {
				neighbor.chunk.Refresh();
			}
		}
	}

	private void RefreshSelfOnly() {
		chunk.Refresh();
	}

	public HexCell GetNeighbor(HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor(HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public HexEdgeType GetEdgeType(HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

	public HexEdgeType GetEdgeType(HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
	}
}
