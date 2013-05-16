﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie
{
    public class Invoke : MethodBehavior
    {
        public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
        {
            try
            {
                bool isDeclaredAsync = method.Async();

                if (isDeclaredAsync && method.Void())
                    ThrowForUnsupportedAsyncVoid();

                bool invokeReturned = false;
                object result = null;
                try
                {
                    result = method.Invoke(instance, null);
                    invokeReturned = true;
                }
                catch (TargetInvocationException ex)
                {
                    exceptions.Add(ex.InnerException);
                }

                if (invokeReturned && isDeclaredAsync)
                {
                    var task = (Task)result;
                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        exceptions.Add(ex.InnerExceptions.First());
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        static void ThrowForUnsupportedAsyncVoid()
        {
            throw new NotSupportedException(
                "Async void methods are not supported. Declare async methods with a " +
                "return type of Task to ensure the task actually runs to completion.");
        }
    }
}