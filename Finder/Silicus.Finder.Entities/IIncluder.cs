﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace Silicus.Finder.Entities
{
    public interface IIncluder
    {
        IQueryable<T> Include<T, TProperty>(IQueryable<T> source, Expression<Func<T, TProperty>> path) where T : class;
    }
}