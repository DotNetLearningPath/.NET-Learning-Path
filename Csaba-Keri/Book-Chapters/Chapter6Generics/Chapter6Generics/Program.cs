using Chapter6Generics.Classes;
using Chapter6Generics.Enums;
using Chapter6Generics.Structs;

namespace Chapter6Generics;

internal static class Program
{
    private static void Main(string[] args)
    {
        // GenericDemo
        var obj1 = new GenericDemo<int>(10);
        var obj2 = new GenericDemo<string>("Hello World!");

        var t1 = obj1.GetType();
        Console.WriteLine(t1.Name);

        Console.WriteLine(t1
            .GetGenericArguments()
            .FirstOrDefault()
            ?.Name
        );

        var t2 = obj2.GetType();
        Console.WriteLine(t2.Name);

        Console.WriteLine(t2
            .GetGenericArguments()
            .FirstOrDefault()
            ?.Name
        );

        Console.WriteLine(obj1);
        Console.WriteLine(obj2);

        // Pair
        var pair1 = new Pair<int, int>(1, 2);
        var pair2 = new Pair<int, double>(1, 42.99);
        var pair3 = new Pair<string, bool>("true", true);

        // Shape -> Square, Circle
        var objSquare = new Classes.Square(10);
        Console.WriteLine($"The area of square is {objSquare.Area}");

        var objCircle = new Classes.Circle(7.5);
        Console.WriteLine($"The area of circle is {objCircle.Area}");

        // VARIANCE
        // Covariance
        IEnumerable<string> names = new List<string> { "Marius", "Ankit", "Raffaele" };

        /*
         * The former does not derive from the latter, but string is derived from object, 
         * and because T is covariant, we can assign names to objects.
         * 
         * However, this is only possible while using variant interfaces.
         */
        IEnumerable<object> objects = names;

        /*
         * Classes that implement variant interfaces are not variant themselves but invariant.
         * 
         * That means the following example, where we substitute List<T> for IEnumerable<T>,
         * will produce a compiler error because List<string> cannot be assigned to List<object>.
         */
        //List<object> objectsList = names;

        /*
         * Variance is not supported for value types.
         * 
         * IEnumerable<int> cannot be assigned to IEnumerable<object>
         */
        IEnumerable<int> numbers = new List<int> { 1, 1, 2, 3, 5, 8 };
        //objects = numbers;

        // Contravariance
        Classes.Square sqr1 = new(4);
        Classes.Square sqr2 = new(5);

        /*
         * However, the key to its definition is the in keyword with the type parameter that makes it contravariant.
         * 
         * Because of this, it is possible to pass IShape references where Square or Circle are expected.
         * That means we can safely pass IComparer<IShape> where IComparer<Square> is required. 
         * 
         * Had the IComparer<T> interface been invariant, passing ShapeComparer would result in a compiler error.
         * 
         * A compiler error is also issued, with the implementation shown here,
         * if we try to pass CircleComparer because Circle is not a lesser derived class than Square;
         * it is actually a sibling in the inheritance hierarchy.
         */
        Console.WriteLine(SquareComparison.IsBigger(sqr1, sqr2, new SquareComparer()));
        Console.WriteLine(SquareComparison.IsBigger(sqr1, sqr2, new ShapeComparer()));
        //Console.WriteLine(SquareComparison.IsBigger(sqr1, sqr2, new CircleComparer()));

        /* GENERIC METHODS
         * 
         * C# allows us to create generic methods that accept one or more generic type parameters.
         * We can create a generic method inside a generic class as well as a non-generic class.
         * Both static and non-static methods can be generic.
         * 
         * The rules for type inference are the same for all.
         * The type parameters must be declared after the method name and just before the parameter list,
         * within angle brackets, just like we did for types.
         */
        CompareObjects comps = new CompareObjects();
        Console.WriteLine(comps.Compare<int>(10, 10));
        Console.WriteLine(comps.Compare<double>(10.5, 10.8));
        Console.WriteLine(comps.Compare<string>("a", "a"));
        Console.WriteLine(comps.Compare<string>("a", "b"));

        // The compiler is able to infer the types from the arguments
        Console.WriteLine(comps.Compare(10, 10));
        Console.WriteLine(comps.Compare(10.5, 10.8));
        Console.WriteLine(comps.Compare("a", "a"));
        Console.WriteLine(comps.Compare("a", "b"));

        // TYPE PARAMETER CONSTRAINTS
        Point<int> point1 = new(3, 4);
        Point<double> point2 = new(3.12, 4.55);
        //Point<bool> point3 = new(true, false);
        //Point<string> point4 = new("alpha", "beta");

        var dictionary = new RestrictedDictionary<ShapeType, Shape>();
        var ellipsis = dictionary.Make<Ellipsis>(ShapeType.Rounded);
        var rectangle = dictionary.Make<Rectangle>(ShapeType.Sharp);
    }
}
