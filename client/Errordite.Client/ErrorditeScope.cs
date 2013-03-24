using System;
using System.Collections.Generic;

namespace Errordite.Client
{
#if NET2
    public delegate T Func<T>();
#endif

    /// <summary>
    /// Wrapping a region of code in an ErrorditeScope (with a using block) allows custom
    /// data to be passed to Errordite for any Exception thrown within the block (on the same thread).
    /// 
    /// Usage:
    /// 
    /// using (var errorditeScope = new ErrorditeScope(() => new Dictionary&lt;string, string>){
    ///     { "Key1", someValue },
    ///     { "Key2", someOtherValue }
    ///     )
    /// {
    ///     //application code
    /// 
    ///     //this would normally be in a catch block
    ///     SomethingThatCallsErrorditeClientReportException(); 
    /// 
    ///     errorditeScope.Complete();   
    /// }
    /// 
    /// These can be nested. As long as you call complete in the inner blocks you will only get
    /// the data from blocks in scope when the exception is thrown.
    /// </summary>
    public class ErrorditeScope : IDisposable
    {
        [ThreadStatic] 
        private static Func<Dictionary<string, string>> _getData;
        [ThreadStatic]
        private static int _nestingLevel;
        private readonly Func<Dictionary<string, string>> _getDataUpOne;

        /// <summary>
        /// The data for the current scope.  Used by ErrorditeClient when
        /// sending the exception.
        /// </summary>
        public static Dictionary<string, string> Data
        {
            get { return _getData != null ? _getData() : null; }
        }

        /// <summary>
        /// ctor for initialising the scope with the ability to get some data from the scope.
        /// </summary>
        /// <param name="getData">Func that when called returns a dictionary of data
        /// for sending to Errordite.</param>
        public ErrorditeScope(Func<Dictionary<string, string>> getData)
        {
            _nestingLevel++;

            if (_getData == null)
                _getData = getData;
            else
            {
                _getDataUpOne = _getData;

                _getData = () =>
                {
                    var ret = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> pair in _getDataUpOne())
                        ret.Add(pair.Key, pair.Value);

                    foreach (var kvp in getData())
                        if (!ret.ContainsKey(kvp.Key))
                            ret.Add(kvp.Key, kvp.Value);

                    return ret;
                };
            }
        }

        /// <summary>
        /// In a nested scope, prevents the data from this scope being sent to Errordite.  Hence
        /// should be called just before the using block finishes.
        /// </summary>
        public void Complete()
        {
            _getData = _getDataUpOne;
        }

        /// <summary>
        /// Marks the end of the scope.  Would normally be called implicitly by instantiating the
        /// ErrorditeScope with a using block.  
        /// </summary>
        public void Dispose()
        {
            _nestingLevel--;

            if (_nestingLevel == 0)
                _getData = null;
        }
    }
}