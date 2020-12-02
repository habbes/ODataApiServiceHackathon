using System;
using System.Collections.Generic;
using System.Text;

namespace DataLib
{
    public class CallbackSingletonWrapper<TEntity> : ISingletonWrapper<TEntity>
    {
        Func<TEntity> _valueCallback;
        Action _removeCallback;
        Func<TEntity, TEntity> _setCallback;
        Func<TEntity, TEntity> _updateCallback;

        public CallbackSingletonWrapper(
            Func<TEntity> valueCallback,
            Action removeCallback,
            Func<TEntity, TEntity> setCallback,
            Func<TEntity, TEntity> updateCallback)
        {
            _valueCallback = valueCallback;
            _removeCallback = removeCallback;
            _setCallback = setCallback;
            _updateCallback = updateCallback;
        }

        public TEntity Value => _valueCallback();

        public void Remove()
        {
            _removeCallback();
        }

        public TEntity Set(TEntity entity) => _setCallback(entity);

        public TEntity Update(TEntity entity) => _updateCallback(entity);
    }
}
