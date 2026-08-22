using System;
using UnityEngine;

public interface IHighlightable
{
    void OnHighlighted();
    void OnDeHighlighted();
    void GiveItemToPlayer();
}