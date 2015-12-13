using System;
using OpenTK;

namespace Duality.Utility
{
	public class RectanglePacker
	{
		private Node[] _nodes;
		private Node _root;
		private int _nextFreeNodeIndex;
		
		/// <summary>
		/// Always square textures currently
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		/// How much, if any, padding to apply around each rectangle
		/// </summary>
		public int Padding { get; set; }

		public RectanglePacker(int size)
		{
			Size = size;
			Padding = 1;

			ClearNodes();
			_root = GetNewRoot();
		}

		/// <summary>
		/// Takes a rectangle of width by height and finds the first available position within the node tree where it will fit.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public Vector2 Pack(int width, int height)
		{
			var node = Insert(_root.NodeId, width + (Padding * 2), height + (Padding * 2));
			return node == Node.None ? new Vector2(-1, -1) : new Vector2(node.Rect.X, node.Rect.Y);
		}

		public void Resize(int size)
		{
			Size = size;
			ClearNodes();
			_root = GetNewRoot();
		}

		private Node Insert(int nodeId, int width, int height)
		{
			var node = _nodes[nodeId];

			if (node.IsLeaf == false)
			{
				var newNode = Insert(node.Left, width, height);
				if (newNode != Node.None)
					return newNode;

				return Insert(node.Right, width, height);
			}
			
			if (node.Occupied)
				return Node.None;

			var insertArea = new Rect(0, 0, width, height).Area;
			var nodeArea = node.Rect.Area;
			if (insertArea > nodeArea)
				return Node.None;

			if (Math.Abs(insertArea - nodeArea) < 0.0001f)
			{
				_nodes[nodeId].Occupied = true;
				return node;
			}

			if (_nodes.Length < _nextFreeNodeIndex + 2 + 1)
			{
				Array.Resize(ref _nodes, _nodes.Length * 2);
			}

			node.IsLeaf = false;
			node.Left = ++_nextFreeNodeIndex;
			node.Right = ++_nextFreeNodeIndex;

			_nodes[nodeId] = node;
			_nodes[node.Left].NodeId = node.Left;
			_nodes[node.Right].NodeId = node.Right;

			_nodes[node.Left].IsLeaf = true;
			_nodes[node.Right].IsLeaf = true;

			var deltaW = node.Rect.W - width;
			var deltaH = node.Rect.H - height;

			if (deltaW > deltaH)
			{
				_nodes[node.Left].Rect = new Rect(node.Rect.X, node.Rect.Y, width, node.Rect.H);
				_nodes[node.Right].Rect = new Rect(node.Rect.X + width, node.Rect.Y, deltaW, node.Rect.H);
			}
			else
			{
				_nodes[node.Left].Rect = new Rect(node.Rect.X, node.Rect.Y, node.Rect.W, height);
				_nodes[node.Right].Rect = new Rect(node.Rect.X, node.Rect.Y + height, node.Rect.W, deltaH);
			}

			return Insert(node.Left, width, height);
		}

		private Node GetNewRoot()
		{
			_nodes[0] = new Node {Rect = new Rect(0, 0, Size, Size), IsLeaf = true};
			return _nodes[0];
		}

		private void ClearNodes()
		{
			_nodes = new Node[128];
			for (var i = 0; i < _nodes.Length; i++)
			{
				_nodes[i] = new Node();
			}
		}

		private struct Node : IEquatable<Node>
		{
			public int NodeId;
			public int Left;
			public int Right;
			public Rect Rect;
			public bool IsLeaf;
			public bool Occupied;

			public static Node None = new Node{NodeId = -1};
			
			public bool Equals(Node other)
			{
				return NodeId == other.NodeId;
			}

			public static bool operator ==(Node a, Node b)
			{
				return a.NodeId == b.NodeId;
			}

			public static bool operator !=(Node a, Node b)
			{
				return !(a == b);
			}
		}
	}
}