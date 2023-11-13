using System;
using UnityEngine;

public class TerrainHeightFinder : MonoBehaviour
{
    public Terrain[] terrains; // Liste aller Terrains
    public float currentTerrainHeight;

    private void Start()
    {
        // Finde alle Terrains im Spiel und speichere sie im 'terrains'-Array
        terrains = Terrain.activeTerrains;
    }

    public float GetTerrainHeightAtPosition(Vector3 position)
    {
        float terrainHeight = float.MinValue; // Mindesthöhe des Terrains initialisieren

        foreach (Terrain terrain in terrains)
        {
            // Konvertiere die Position in die lokale Koordinaten des Terrains
            Vector3 terrainLocalPosition = position - terrain.transform.position;

            // Berechne die Höhe des Terrains an der Position des Objekts
            currentTerrainHeight = terrain.SampleHeight(position);
            if (currentTerrainHeight > terrainHeight)
            {
                terrainHeight = currentTerrainHeight;
            }
            // Zeige die Höhe in der Konsole an (kann entfernt werden, wenn nicht benötigt)
            //Debug.Log("Höhe des Terrains: " + terrainHeight);
        }


        return terrainHeight;
    }
}