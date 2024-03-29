﻿using System;
using System.Collections.Generic;

namespace Common
{
    // This queue implementation was based on the stack implementation on https://developerslogblog.wordpress.com/2019/07/23/how-to-implement-a-stack-in-c/
    // I made some adjustments to include things such as Count and adjusted some code to make it more readable to me, as with my stack implementation
    // I also had to add a front and rear position as I had to keep track of how many items had been removed from the front of the queue
    /// <summary>
    /// A FIFO data structure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Queue<T>
    {
        private T[] data; // Holds the data in the queue

        public int Count { get => rear - front; } // The number the number of items
        private int rear = 0; // Keeps track of the last item in the queue (the most recently added item)
        private int front = 0; // Keeps track of the first item in the queue (the next one to be popped)

        /// <summary>
        /// Create a queue starting with a size of <paramref name="initialSize"/>
        /// </summary>
        /// <param name="initialSize"></param>
        public Queue(int initialSize)
        {
            data = new T[initialSize]; // Initialise the data with the original size
        }

        /// <summary>
        /// Add a new item to the end of the queue
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            // If the queue is full, create a new array with double the length and copy across the data
            if (rear == data.Length)
            {
                // If the array is full because all the spaces are filled, double the length of the array
                // Otherwise, there is at least one space at the front of the array (as popping removes from the front), so don't resize the array, just shift everything to the front
                int newSize = Count == data.Length ? data.Length * 2 : data.Length;

                T[] resized = new T[newSize];
                Array.Copy(data, front, resized, 0, Count); // Only copy across the non-empty items in the queue (from front onwards)
                data = resized;

                // Correct the front and rear pointers
                rear = Count;
                front = 0;
            }

            // Add the item to the array and increment the Count for the next item
            data[rear++] = item;
        }

        /// <summary>
        /// Returns the first item in the queue and then removes it
        /// </summary>
        /// <returns>The first item in the queue</returns>
        public T Pop()
        {
            if (Count == 0) throw new IndexOutOfRangeException("Could not pop, the queue is empty"); // Throw an error if user tries to pop an empty queue
            return data[front++]; // Return the last item and decrement Count so the item is essential erased (it will be overritten on the next push)
        }

        /// <summary>
        /// Returns the first item in the queue without removing it
        /// </summary>
        /// <returns>The first item in the queue</returns>
        public T Peek()
        {
            if (Count == 0) throw new IndexOutOfRangeException("Could not peek, the queue is empty"); // Throw an error if user tries to peek an empty queue
            return data[front]; // Return the last item
        }

        /// <summary>
        /// Checks if the queue contains <paramref name="item"/>
        /// </summary>
        /// <param name="item">Item to find in the queue</param>
        /// <returns>True if the queue contains the item, false otherwise</returns>
        public bool Contains(T item)
        {
            // Loop through all the items in the queue
            for (int i = 0; i < Count; i++)
                if (EqualityComparer<T>.Default.Equals(data[i], item)) return true; // If item is equal, return true (using EqualityComparer to ensure use of IEquatable functions) 

            // Return false as no item in the queue are equal to the given item
            return false;
        }
    }
}
