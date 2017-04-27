namespace EngineName.Utils {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System.Threading;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Provides functionality for performing atomic operations.</summary>
public static class AtomicUtil {
    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>As an atomic operation, swaps <paramref name="a"/> to
    ///          <paramref name="b"/> if it is equal to
    ///          <paramref name="c"/>.</summary>
    /// <param name="a">A reference to the variable to change.</param>
    /// <param name="b">The vlaue to set <paramref name="a"/> to.</param>
    /// <param name="c">The value to compare <paramref name="a"/> to.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="a"/> was
    ///          set to <paramref name="c"/>.</returns>
    public static bool CAS(ref int a, int b, int c) {
        return Interlocked.CompareExchange(ref a, b, c) == c;
    }

    /// <summary>As an atomic operation, swaps <paramref name="a"/> to
    ///          <paramref name="b"/> if it is equal to
    ///          <paramref name="c"/>.</summary>
    /// <param name="a">A reference to the variable to change.</param>
    /// <param name="b">The vlaue to set <paramref name="a"/> to.</param>
    /// <param name="c">The value to compare <paramref name="a"/> to.</param>
    /// <typeparam name="T">Specifies the type of
    ///                     <paramref name="a"/>.</typeparam>
    /// <returns><see langword="true"/> if the value of <paramref name="a"/> was
    ///          set to <paramref name="c"/>.</returns>
    public static bool CAS<T>(ref T a, T b, T c) where T: class {
        return Interlocked.CompareExchange(ref a, b, c) == c;
    }
}

}
