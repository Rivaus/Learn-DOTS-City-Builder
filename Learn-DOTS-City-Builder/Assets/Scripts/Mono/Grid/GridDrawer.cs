using quentin.tran.common;
using UnityEngine;

namespace quentin.tran.mono.grid
{
    [RequireComponent(typeof(MeshFilter))]
    public class GridDrawer : MonoBehaviour
    {
        private MeshFilter filter;

        private void Awake()
        {
            this.filter = GetComponent<MeshFilter>();

            Vector3[] vertices = new Vector3[2 * GridProperties.GRID_SIZE];
            int[] indices = new int[2 * GridProperties.GRID_SIZE];

            for (int i = 0; i < GridProperties.GRID_SIZE; i += 2)
            {
                vertices[i] = new Vector3(i * GridProperties.GRID_CELL_SIZE, 0, 0) / 2f;
                vertices[i + 1] = new Vector3(i * GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE * GridProperties.GRID_SIZE) / 2f;

                indices[i] = i;
                indices[i + 1] = i + 1;
            }

            for (int i = 0; i < GridProperties.GRID_SIZE; i += 2)
            {
                int j = GridProperties.GRID_SIZE + i;

                vertices[j] = new Vector3(0, 0, i * GridProperties.GRID_CELL_SIZE) / 2f;
                vertices[j + 1] = new Vector3(GridProperties.GRID_CELL_SIZE * GridProperties.GRID_SIZE, 0, i * GridProperties.GRID_CELL_SIZE) / 2f;

                indices[j] = j;
                indices[j + 1] = j + 1;
            }

            Mesh grid = new();
            grid.SetVertices(vertices);
            grid.SetIndices(indices, MeshTopology.Lines, 0);

            this.filter.sharedMesh = grid;
        }
    }
}

