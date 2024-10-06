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

            Vector3[] vertices = new Vector3[2 * 2 * (GridProperties.GRID_SIZE + 1)];
            int[] indices = new int[2 * 2 * (GridProperties.GRID_SIZE + 1)];

            for (int i = 0; i < GridProperties.GRID_SIZE + 1; i ++)
            {
                int indice = 2 * i;
                vertices[indice] = new Vector3(i * GridProperties.GRID_CELL_SIZE, 0, 0);
                vertices[indice + 1] = new Vector3(i * GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE * GridProperties.GRID_SIZE);

                indices[indice] = indice;
                indices[indice + 1] = indice + 1;
            }

            for (int i = 0; i < GridProperties.GRID_SIZE + 1; i ++)
            {
                int indice = 2 * i;
                int j = 2 * (GridProperties.GRID_SIZE + 1) + indice;

                vertices[j] = new Vector3(0, 0, i * GridProperties.GRID_CELL_SIZE); 
                vertices[j + 1] = new Vector3(GridProperties.GRID_CELL_SIZE * GridProperties.GRID_SIZE, 0, i * GridProperties.GRID_CELL_SIZE);

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

