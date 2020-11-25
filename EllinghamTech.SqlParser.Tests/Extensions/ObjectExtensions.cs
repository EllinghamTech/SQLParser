using System;
using System.Linq;
using System.Reflection;

namespace EllinghamTech.SqlParser.Tests.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Similar to the Ruby-style object.send(:method, *params).  Allows access to a non-public instance methods
        /// on an object by using Reflection.
        ///
        /// Also works for instance fields and properties.  When there is no method params, returns the value.
        /// When there is one, sets the value.
        ///
        /// This method breaks the whole idea of class visibility and is only designed so that the tests can access
        /// and test private/protected functionality.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="methodParams"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static object Send(this object obj, string methodName, params object[]? methodParams)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Method name cannot be null or empty");

            MethodInfo method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field = obj.GetType().GetField(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo property = obj.GetType().GetProperty(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            // Only one can be non-null
            object[] choices = {method, field, property};

            if (choices.Count(a => a != null) > 1)
                throw new AmbiguousMatchException("Found multiple possible methods, properties or fields");

            if (method != null)
                return method.Invoke(obj, methodParams);

            methodParams ??= new object[] { };

            if (field != null)
            {
                if (methodParams.Length == 0)
                    return field.GetValue(obj);

                if (methodParams.Length == 1)
                {
                    field.SetValue(obj, methodParams[0]);
                    return null;
                }

                throw new Exception("Unknown operation on field");
            }

            if (property != null)
            {
                if (methodParams.Length == 0)
                    return property.GetValue(obj);

                if (methodParams.Length == 1)
                {
                    property.SetValue(obj, methodParams[0]);
                    return null;
                }

                throw new Exception("Unknown operation on properties");
            }

            throw new Exception("Cannot find method, property or field on object");
        }
    }
}
