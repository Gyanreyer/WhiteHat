using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue
{
    List<NavNode> list;

    public PriorityQueue()
    {
        list = new List<NavNode>();
    }

    public void Enqueue(NavNode data)
    {
        list.Add(data);
        int index = list.Count - 1;

        bool sorted = false;
        while (!sorted)
        {
            int parentIndex = ((index - 1) / 2);
            if (list[parentIndex].Priority > list[index].Priority)
            {
                NavNode tempData = list[parentIndex];
                list[parentIndex] = list[index];
                list[index] = tempData;
                index = parentIndex;
            }
            else
            {
                sorted = true;
            }
        }
    }
    public NavNode Dequeue()
    {
        if (list.Count == 0)
            return null;

        NavNode dat = list[0];
        list[0] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        int index = 0;

        bool sorted = false;
        while (!sorted)
        {
            int leftChildIndex = (2 * index) + 1;
            int rightChildIndex = (2 * index) + 2;

            if (index >= list.Count)
            {
                break;
            }

            NavNode tempData = list[index];

            if (rightChildIndex < list.Count && leftChildIndex < list.Count && list[index].Priority > list[leftChildIndex].Priority && list[index].Priority > list[rightChildIndex].Priority)
            {
                // Bigger than both
                if (list[leftChildIndex].Priority < list[rightChildIndex].Priority)
                {
                    list[index] = list[leftChildIndex];
                    list[leftChildIndex] = tempData;
                    index = leftChildIndex;
                }
                else
                {
                    list[index] = list[rightChildIndex];
                    list[rightChildIndex] = tempData;
                    index = rightChildIndex;
                }
            }
            else if (leftChildIndex < list.Count && list[index].Priority > list[leftChildIndex].Priority)
            {
                // Just bigger than left
                list[index] = list[leftChildIndex];
                list[leftChildIndex] = tempData;
                index = leftChildIndex;
            }
            else if (rightChildIndex < list.Count && list[index].Priority > list[rightChildIndex].Priority)
            {
                // Just bigger than right
                list[index] = list[rightChildIndex];
                list[rightChildIndex] = tempData;
                index = rightChildIndex;
            }
            else
            {
                sorted = true;
            }
        }//end of loop
        return dat;
    }

    private void Sort()
    {
        int index = list.Count - 1;

        bool sorted = false;
        while (!sorted)
        {
            int parentIndex = ((index - 1) / 2);
            if (list[parentIndex].Priority > list[index].Priority)
            {
                NavNode tempData = list[parentIndex];
                list[parentIndex] = list[index];
                list[index] = tempData;
                index = parentIndex;
            }
            else
            {
                sorted = true;
            }
        }
    }

    public void Remove(NavNode v)
    {
        list.Remove(v);
        if (list.Count > 0) 
        Sort();
    }

    public NavNode Peek()
    {
        if (list.Count > 0)
            return list[0];
        else
            return null;
    }

    public bool IsEmpty()
    {
        return list.Count == 0;
    }

    public bool Contains(NavNode v)
    {
        return list.Contains(v);
    }
}
