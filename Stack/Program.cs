using System;
class Program
{
    static void Main(string[] args)
    {
        /* System.ArgumentOutOfRangeException: 'capacity ('-1') must be a non-negative value. Arg_ParamName_Name
         *    ArgumentOutOfRange_ActualValue'
         */
        //System.Collections.Generic.Ozan.Stack<int> stack2 = new System.Collections.Generic.Ozan.Stack<int>(-1);

        System.Collections.Generic.Ozan.Stack<int> stack = new System.Collections.Generic.Ozan.Stack<int>(5);
       /* stack.Peek();*/ // gives System.InvalidOperationException: 'Stack empty'
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Push(4);
        stack.Push(5);
        Console.WriteLine($"stack.Peek() is {stack.Peek()}");
        Console.WriteLine("Popping elements from the stack:");
        while (stack.Count > 0)
        {
            Console.WriteLine(stack.Pop());
        }
       /* stack.Pop(); */ // gives System.InvalidOperationException: 'Stack empty'

    }
}