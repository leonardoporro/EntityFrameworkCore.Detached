﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Detached.EntityFramework.Events
{
    public class WeakDelegate<TEventArgs>
    {
        private delegate void OpenEventHandler(object target, object sender, TEventArgs e);

        private static readonly ConcurrentDictionary<MethodInfo, OpenEventHandler> _openHandlerCache =
            new ConcurrentDictionary<MethodInfo, OpenEventHandler>();

        private static OpenEventHandler CreateOpenHandler(MethodInfo method)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var sender = Expression.Parameter(typeof(object), "sender");
            var e = Expression.Parameter(typeof(TEventArgs), "e");

            if (method.IsStatic)
            {
                var expr = Expression.Lambda<OpenEventHandler>(
                    Expression.Call(
                        method,
                        sender, e),
                    target, sender, e);
                return expr.Compile();
            }
            else
            {
                var expr = Expression.Lambda<OpenEventHandler>(
                    Expression.Call(
                        Expression.Convert(target, method.DeclaringType),
                        method,
                        sender, e),
                    target, sender, e);
                return expr.Compile();
            }
        }

        private readonly WeakReference _weakTarget;
        private readonly MethodInfo _method;
        private readonly OpenEventHandler _openHandler;

        public WeakDelegate(Delegate handler)
        {
            _weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
            _method = handler.GetMethodInfo();
            _openHandler = _openHandlerCache.GetOrAdd(_method, CreateOpenHandler);
        }

        public bool Invoke(object sender, TEventArgs e)
        {
            object target = null;
            if (_weakTarget != null)
            {
                target = _weakTarget.Target;
                if (target == null)
                    return false;
            }
            _openHandler(target, sender, e);
            return true;
        }

        public bool IsMatch(EventHandler<TEventArgs> handler)
        {
            return ReferenceEquals(handler.Target, _weakTarget?.Target)
                && handler.GetMethodInfo().Equals(_method);
        }
    }
}