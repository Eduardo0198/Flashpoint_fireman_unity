using System.Collections.Generic;
using UnityEngine;

// Requirement: calcula donde debe ir cada pieza del tablero (piso, paredes, puertas)
// y que cambio de estado hay entre dos snapshots. No instancia ni destruye nada:
// eso es responsabilidad de Framework/Controllers/BoardController.
public static class BoardLayoutRequirement
{
    public const float CELL_SIZE = 5f;

    public struct FloorPlacement
    {
        public Vector3 Position;
        public bool EsExterior; // true = aro de 1 celda fuera del edificio (spawn de bomberos)
    }

    public struct WallPlacement
    {
        public int Row;
        public int Col;
        public bool EsVertical;
        public WallState Estado;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public struct WallChange
    {
        public int Row;
        public int Col;
        public bool EsVertical;
        public WallState EstadoAnterior;
        public WallState EstadoNuevo;
    }

    public struct FireCellPlacement
    {
        public int Row;
        public int Col;
        public CellValue Estado;
        public Vector3 Position;
    }

    public struct FireCellChange
    {
        public int Row;
        public int Col;
        public CellValue EstadoAnterior;
        public CellValue EstadoNuevo;
    }

    // Grid fisico = aro exterior de 1 celda + interior jugable (filas x columnas).
    public static List<FloorPlacement> ComputeFloorPositions(GameState state)
    {
        var resultado = new List<FloorPlacement>();
        for (int physRow = 0; physRow <= state.filas + 1; physRow++)
        {
            for (int physCol = 0; physCol <= state.columnas + 1; physCol++)
            {
                bool esExterior = physRow == 0 || physRow == state.filas + 1
                    || physCol == 0 || physCol == state.columnas + 1;

                resultado.Add(new FloorPlacement
                {
                    Position = new Vector3(physCol * CELL_SIZE, 0f, physRow * CELL_SIZE),
                    EsExterior = esExterior,
                });
            }
        }
        return resultado;
    }

    public static List<WallPlacement> ComputeWallPlacements(GameState state)
    {
        var resultado = new List<WallPlacement>();

        // Aristas verticales: separan columna col y col+1 dentro de la fila interior "row".
        for (int row = 0; row < state.filas; row++)
        {
            for (int colBoundary = 0; colBoundary <= state.columnas; colBoundary++)
            {
                int estado = state.paredesVerticales[row * (state.columnas + 1) + colBoundary];
                if (estado == (int)WallState.Abierto) continue;

                resultado.Add(ComputeSingleWallPlacement(row, colBoundary, esVertical: true, state.columnas, (WallState)estado));
            }
        }

        // Aristas horizontales: separan fila row y row+1 dentro de la columna interior "col".
        for (int rowBoundary = 0; rowBoundary <= state.filas; rowBoundary++)
        {
            for (int col = 0; col < state.columnas; col++)
            {
                int estado = state.paredesHorizontales[rowBoundary * state.columnas + col];
                if (estado == (int)WallState.Abierto) continue;

                resultado.Add(ComputeSingleWallPlacement(rowBoundary, col, esVertical: false, state.columnas, (WallState)estado));
            }
        }

        return resultado;
    }

    public static WallPlacement ComputeSingleWallPlacement(int row, int col, bool esVertical, int columnas, WallState estado)
    {
        bool esPuerta = estado == WallState.PuertaCerrada || estado == WallState.PuertaAbierta;
        Quaternion rotacionVertical = esPuerta ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
        Quaternion rotacionHorizontal = esPuerta ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);

        if (esVertical)
        {
            int physRow = row + 1;
            float xLimite = (col + 0.5f) * CELL_SIZE;
            return new WallPlacement
            {
                Row = row,
                Col = col,
                EsVertical = true,
                Estado = estado,
                Position = new Vector3(xLimite, 0f, physRow * CELL_SIZE),
                Rotation = rotacionVertical,
            };
        }
        else
        {
            int physCol = col + 1;
            float zLimite = (row + 0.5f) * CELL_SIZE;
            return new WallPlacement
            {
                Row = row,
                Col = col,
                EsVertical = false,
                Estado = estado,
                Position = new Vector3(physCol * CELL_SIZE, 0f, zLimite),
                Rotation = rotacionHorizontal,
            };
        }
    }

    public static List<WallChange> ComputeWallDiff(
        int[] verticalAnterior, int[] verticalNuevo,
        int[] horizontalAnterior, int[] horizontalNuevo,
        int filas, int columnas)
    {
        var cambios = new List<WallChange>();

        for (int row = 0; row < filas; row++)
        {
            for (int colBoundary = 0; colBoundary <= columnas; colBoundary++)
            {
                int idx = row * (columnas + 1) + colBoundary;
                int anterior = verticalAnterior[idx];
                int nuevo = verticalNuevo[idx];
                if (anterior == nuevo) continue;

                cambios.Add(new WallChange
                {
                    Row = row,
                    Col = colBoundary,
                    EsVertical = true,
                    EstadoAnterior = (WallState)anterior,
                    EstadoNuevo = (WallState)nuevo,
                });
            }
        }

        for (int rowBoundary = 0; rowBoundary <= filas; rowBoundary++)
        {
            for (int col = 0; col < columnas; col++)
            {
                int idx = rowBoundary * columnas + col;
                int anterior = horizontalAnterior[idx];
                int nuevo = horizontalNuevo[idx];
                if (anterior == nuevo) continue;

                cambios.Add(new WallChange
                {
                    Row = rowBoundary,
                    Col = col,
                    EsVertical = false,
                    EstadoAnterior = (WallState)anterior,
                    EstadoNuevo = (WallState)nuevo,
                });
            }
        }

        return cambios;
    }

    public static List<FireCellPlacement> ComputeFireCellPlacements(GameState state)
    {
        var resultado = new List<FireCellPlacement>();

        for (int row = 0; row < state.filas; row++)
        {
            for (int col = 0; col < state.columnas; col++)
            {
                var estado = (CellValue)state.tablero[row * state.columnas + col];
                if (estado != CellValue.Humo && estado != CellValue.Fuego) continue;

                int physRow = row + 1;
                int physCol = col + 1;
                resultado.Add(new FireCellPlacement
                {
                    Row = row,
                    Col = col,
                    Estado = estado,
                    Position = new Vector3(physCol * CELL_SIZE, 0f, physRow * CELL_SIZE),
                });
            }
        }

        return resultado;
    }

    public static List<FireCellChange> ComputeFireDiff(int[] tableroAnterior, int[] tableroNuevo, int filas, int columnas)
    {
        var cambios = new List<FireCellChange>();

        for (int row = 0; row < filas; row++)
        {
            for (int col = 0; col < columnas; col++)
            {
                int idx = row * columnas + col;
                int anterior = tableroAnterior[idx];
                int nuevo = tableroNuevo[idx];
                if (anterior == nuevo) continue;

                cambios.Add(new FireCellChange
                {
                    Row = row,
                    Col = col,
                    EstadoAnterior = (CellValue)anterior,
                    EstadoNuevo = (CellValue)nuevo,
                });
            }
        }

        return cambios;
    }
}
