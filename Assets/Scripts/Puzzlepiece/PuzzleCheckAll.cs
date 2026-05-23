using System;
using UnityEngine;
using System.Collections.Generic;

public class PuzzleCheckAll : MonoBehaviour
{
    public List<PuzzlePiece> allPieces;
    public GameObject page;

    private void Start()
    {
        page.gameObject.SetActive(false);
    }

    public bool IsAllComplete()
    {
        foreach (var piece in allPieces)
        {
            if (!piece.IsCorrect())
                return false;
        }
        return true;
    }

    void Update()
    {
        if (IsAllComplete())
        {
            page.gameObject.SetActive(true);
        }
    }
}