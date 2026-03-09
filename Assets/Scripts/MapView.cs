using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class MapView : MonoBehaviour
{
    public Action<Vector2Int> onTileCreate;
    public Action<Vector2Int> onTileHide;
    public Action<Vector2Int> onTileRemove;

    public GeoLocation center;
    private Vector2 centerXY;
    public Vector2 center0;
    public int zoomLevel;
    public int buffer;

    private HashSet<Vector2Int> neddedTiles = new();

    public Transform cam;

    public bool test;
    private void Start()
    {
        zoomLevel = 17;
        buffer = 2;
        if (test)
        {
            center = new GeoLocation(56.45269f, 84.97226f);
            InitilizeCenter();
        }
        else
            StartCoroutine(InitializeLocation());
    }
    public void ModifyZoom(int isAdding)
    {
        if (isAdding == 1 && zoomLevel == 19 ||
            isAdding == -1 && zoomLevel == 9)
            return;
        center = LocationTransform.XYToLatLon(centerXY, zoomLevel);
        zoomLevel += isAdding;
        foreach (var tile in neddedTiles)
        {
            onTileRemove?.Invoke(tile);
        }
        InitilizeCenter();
    }
    private IEnumerator InitializeLocation()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
        }
        Input.location.Start();

        float timeout = 10f;
        while (Input.location.status == LocationServiceStatus.Initializing && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Невозможно определить местоположение.");
            center = new GeoLocation(56.468733f, 84.945077f);
            yield break;
        }
        else if (Input.location.status == LocationServiceStatus.Stopped ||
            Input.location.status == LocationServiceStatus.Failed || timeout <= 0){
            Debug.LogWarning("GPS недоступен");
            center = new GeoLocation(56.468733f, 84.945077f);
        }
        else
        {
            var loc = Input.location.lastData;
            center = new GeoLocation(loc.latitude, loc.longitude);
        }
        InitilizeCenter();
    }
    public void Update()
    {
        UpdateTiles();
    }
    public void UpdateTiles()
    {
        centerXY.x = center0.x + (int)(cam.position.x / 2.56f);
        centerXY.y = center0.y - (int)(cam.position.y / 2.56f);

        int xStart = (int)centerXY.x - buffer;
        int xEnd = (int)centerXY.x + buffer;
        int yStart = (int)centerXY.y - buffer - 1;
        int yEnd = (int)centerXY.y + buffer + 1;

        HashSet<Vector2Int> neededTiles = new HashSet<Vector2Int>();

        for (int x = xStart; x <= xEnd; x++)
        {
            for (int y = yStart; y <= yEnd; y++)
            {
                Vector2Int key = new(x, y);
                neededTiles.Add(key);

                if (!neddedTiles.Contains(key))
                    onTileCreate?.Invoke(key);
            }
        }

        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var key in neddedTiles)
            if (!neededTiles.Contains(key))
                onTileHide?.Invoke(key);

        neddedTiles = neededTiles;
    }
    private void InitilizeCenter()
    {
        if (center.latitude == 0 && center.longitude == 0)
            return;

        centerXY = LocationTransform.LatLonToXY(center, zoomLevel);
        center0 = centerXY;
        cam.transform.position = new Vector3(0, 0, -10);
    }
}
