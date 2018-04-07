using System;

namespace HandyQuery.Language
{
    /// <summary>
    /// Allows to initialize value only when needed. Initializes only once and then is reused.
    /// <para> WARNING: NOT THREAD SAFE! </para>
    /// </summary>
    internal struct ValueLazy<TValue>
    {
        private readonly Func<TValue> _create;

        private TValue _value;

        private bool _hasValue;

        public TValue Value
        {
            get
            {
                if (_hasValue) return _value;

                _value = _create();
                _hasValue = true; // order could be changed thus this code is not thread safe.
                return _value;
            }
        }

        public ValueLazy(Func<TValue> create)
        {
            _create = create;
            _value = default;
            _hasValue = false;
        }
    }
}