using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Collections
{
    public class CollectionsUtils
    {
        // Constants

        // Delegates

        // Events

        // Private Fields

        // Constructors

        // Private Properties

        // Protected Properties

        // Public Properties

        // Private Methods

        // Protected Methods

        // Public Methods
        public static List<T> CreateList<T>(params T[] elements)
        {
            return new List<T>(elements);
        }


        public static List<T> CreateEmptyList<T>(T element)
        {
            return new List<T>();
        }


        // Event Handlers

    }
}
