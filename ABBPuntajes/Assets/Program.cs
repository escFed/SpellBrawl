
public class Program
{
    private int head = 0;
    private int tail = 0;
    private Nodo[] queue = new Nodo[10];

    public void Init()
    {
        head = 0;
        tail = 0;
    }

    public void Enqueue(Nodo value)
    {
        if (tail < queue.Length)
        {
            queue[tail] = value;
            tail++;
        }
      
    }

    public Nodo Dequeue()
    {
        if (head < tail)
        {
            Nodo value = queue[head];
            head++;
            return value;
        }
        else
        {
            
            return null;
        }
    }

    public int Count => tail - head;
}


