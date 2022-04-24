using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Script that deals with how a players rank interacts with the stage globe
 */
public class RankingLevels : MonoBehaviour
{
    //Placeholder for whatever metric ends up being the players actual rank
    public int ranking = 1;

    //The level (rank) that this part of the globe coresponds to
    public int levelNumber = 1;

    //Materials for when the rank is not your rank and is your rank respectivly
    public Material closedMaterial, openMaterial;
    void Update()
    {
        //Grabbing the currect material render of the object
        MeshRenderer my_renderer = GetComponent<MeshRenderer>();
        if (my_renderer != null)
        {
            Material my_material = my_renderer.material;
        }

        //Swapping the materials
        if (ranking == levelNumber)
        {
            my_renderer.material = openMaterial;
        }
        else {
            my_renderer.material = closedMaterial;
        }
    }
}
