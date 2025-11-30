using System;
using System.Collections.Generic;
using System.Text;

namespace FactoorSharp.FacturXDocumentationRenderer
{
    public static class TreeExtensions
    {
        public static void Traverse<T>(
            this T root,
            Func<T, IEnumerable<T>> childrenSelector,
            Action<T> action)
        {
            action(root);

            foreach (var child in childrenSelector(root))
            {
                child.Traverse(childrenSelector, action);
            }
        } // !Traverse()
    }
}
