using UnityEngine;
using System.Collections.Generic;
public class BallStack
{

    //Arreglo donde se guardan las pelotas
    GameObject[] balls;

    //Cantidad de pelotas en la pila

    int ballsCount;

    int index;


    public void Init(int quantity)
    {
        ballsCount = quantity;
        balls = new GameObject[quantity];
        index = 0;
    }


   public bool Push(GameObject ballPrefab)
    {
        if (!FullStack())
        {
            balls[index] = ballPrefab;
            index++;
            return true;

        }

        return false;
     
    }


   public GameObject Pop()
    {
        if (!EmptyStack())
        {
            index--;                  // Reducimos el índice
            return balls[index];      // Devolvemos el último elemento
        }
        return null;
    }


    public bool FullStack()
    {
        return index == ballsCount;
    }


    public bool EmptyStack()
    {
        return index == 0;
    }


}
