using TMPro;
using UnityEngine;

public class ABBHighScores : MonoBehaviour
{

    private Program queue;
    private Nodo nodo;

    [SerializeField] private TextMeshProUGUI scoreText;


    private void Start()
    {
        nodo = null; 
     
        AddScore(nodo, 100);
        AddScore(nodo, 103);
        AddScore(nodo, 67);
        AddScore(nodo, 120);
        AddScore(nodo, 90);

        ShowScores();
    }






    public int Raiz()
    {
        return nodo.info;
    }

    public Nodo LeftSon()
    {
        return nodo.left;
    }

    public Nodo RightSon()
    {
        return nodo.right;
    }


    public void AddScore(Nodo r, int x)
    {
        if (r == null)
        {
            nodo = new Nodo
            {
                info = x
            };
        }


        else if (r.info > x)
        {
            if (r.left == null)
            {
                r.left = new Nodo
                {
                    info = x
                };
            }
            else
            {


                AddScore(r.left, x);
            }
        }
        else if (r.info < x)
        {
            if (r.right == null)
            {
                r.right = new Nodo
                {
                    info = x
                };

                
            }
            else
            {
                AddScore(r.right, x);
            }
        }
    }

    private void InOrder(Nodo z, System.Collections.Generic.List<int> scores)
    {
        if(z != null)
        {
            InOrder(z.left, scores);
            scores.Add(z.info);
            InOrder(z.right, scores);
        }
    }


    private void ShowScores()
    {
        var list = new System.Collections.Generic.List<int>();
        InOrder(nodo, list);


        list.Reverse();

        string result = "";
        foreach (var score in list)
        {
            result += score + "\n";
        }

        scoreText.text = result;
    }




}





public class Nodo
{
    public int info;
    public Nodo left;
    public Nodo right;
}
