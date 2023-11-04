using UnityEngine;

public class TerrainHeightFinder : MonoBehaviour
{
    public Terrain[] terrains; // Liste aller Terrains

    public float GetTerrainHeightAtPosition(Vector3 position)
    {
        float terrainHeight = float.MinValue; // Mindesthöhe des Terrains initialisieren

        foreach (Terrain terrain in terrains)
        {
            // Prüfen, ob die Position innerhalb des aktuellen Terrains liegt
            if (position.x >= terrain.transform.position.x && position.x <= terrain.transform.position.x + terrain.terrainData.size.x &&
                position.z >= terrain.transform.position.z && position.z <= terrain.transform.position.z + terrain.terrainData.size.z)
            {
                // Position auf die lokale Position des Terrains umrechnen
                Vector3 localPosition = position - terrain.transform.position;

                // Höhe des Terrains an der lokalen Position erhalten
                float height = terrain.SampleHeight(localPosition);

                // Wenn die Höhe größer als die bisherige gefunden Höhe ist, aktualisieren
                if (height > terrainHeight)
                {
                    terrainHeight = height;
                }
            }
        }

        return terrainHeight;
    }
}