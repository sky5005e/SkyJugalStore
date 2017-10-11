using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HAAD.HPL.Base.Data
{
    /// <summary>
    /// Sandeep Sirohi
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterUtil<T> where T : class
    {
        #region .Members.
        /// <summary>
        /// The contains method
        /// </summary>
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains");
        /// <summary>
        /// The starts with method
        /// </summary>
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        /// <summary>
        /// The ends with method
        /// </summary>
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

        #endregion

        #region .Methods.

        /// <summary>
        /// Builds the expression filter.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="fieldInfo">The field information.</param>
        private static void BuildExpressionFilter(Expression exp, ParameterExpression param, FieldInformation fieldInfo)
        {
            if (exp == null)
                exp = GetExpression(param, fieldInfo);
            else
                exp = Expression.AndAlso(exp, GetExpression(param, fieldInfo));

        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="FieldInfo">The field information.</param>
        /// <returns></returns>
        private static Expression GetExpression(ParameterExpression param, FieldInformation FieldInfo)
        {
            try
            {
                MemberExpression member = Expression.Property(param, FieldInfo.Name);
                dynamic value = SetProperty(FieldInfo);
                if (value != null)
                {
                    var constant = Expression.Constant(value);
                    if (FieldInfo.Operator.ToString() == "eq" && FieldInfo.PropertyInfo.PropertyType.IsGenericType && FieldInfo.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var type = Nullable.GetUnderlyingType(FieldInfo.PropertyInfo.PropertyType);
                        var converted = Expression.Convert(constant, type);
                        Expression left = member;
                        left = Expression.Convert(left, type);
                        Expression right = Expression.Constant(value);

                        return Expression.Equal(left, right);
                    }
                    switch (FieldInfo.Operator.ToString())
                    {
                        case "eq":
                            return Expression.Equal(member, constant);

                        case "neq":
                            return Expression.NotEqual(member, constant);

                        case "gt":
                            return Expression.GreaterThan(member, constant);

                        case "lt":
                            return Expression.LessThan(member, constant);

                        case "gteq":
                            return Expression.GreaterThanOrEqual(member, constant);

                        case "lteq":
                            return Expression.LessThanOrEqual(member, constant);

                        case "StartsWith":
                            return Expression.Call(member, startsWithMethod, constant);

                        case "EndsWith":
                            return Expression.Call(member, endsWithMethod, constant);

                        case "IsNull":
                            return Expression.Equal(member, null);

                        case "NotIsNull":
                            return Expression.NotEqual(member, null);

                        case "Contains":
                            return Expression.Call(member, containsMethod, constant);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        /// <summary>
        /// Gets the filter expression.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="sortby">The sortby.</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetFilterExpression(string filter, out List<FieldInformation> PagerInfo)
        {
            if (string.IsNullOrEmpty(filter))
            {
                PagerInfo = null;
                return null;
            }
            else
            {
                List<FieldInformation> list = GetProportyInfo(filter, out PagerInfo);
                if (list.Count > 0)
                {
                    ParameterExpression param = Expression.Parameter(typeof(T), "t");
                    Expression _Expression = null;
                    foreach (var fieldInfo in list)
                    {
                        var exp = GetExpression(param, fieldInfo);
                        if (exp != null)
                        {
                            if (_Expression == null)
                                _Expression = exp;
                            else
                            {
                                _Expression = Expression.AndAlso(_Expression, exp);
                            }
                        }
                    }
                    return Expression.Lambda<Func<T, bool>>(_Expression, param);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the model expression.
        /// </summary>
        /// <param name="Model">The model.</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetModelExpression(T Model)
        {
            // [GetModelExpression] This function is to filter using model. Currerntly not updated but may require later for alternative filtering.
            // var properties = Model.GetType().GetProperties().Where(pi => pi.GetValue(Model) is string).Select(pi => new { Name = (string)pi.Name, Value = (string)pi.GetValue(Model) });
            var properties = Model.GetType().GetProperties().
                Where(x => x.GetValue(Model) != null && !x.Name.Equals("Take") &&
                !x.Name.Equals("Page") &&
                !x.Name.Equals("PageSize") &&
                !x.Name.Equals("Total")).
                Select(pi => new { Name = (string)pi.Name, Value = pi.GetValue(Model), ValueType = pi.PropertyType.Name });
            if (properties.Count() > 0)
            {
                var param = Expression.Parameter(typeof(T), "q");
                Expression filterExpression = null;
                foreach (var item in properties)
                {
                    Expression exp = null;
                    switch (item.ValueType)
                    {
                        case "Char":
                            exp = Expression.Call(Expression.Property(param, item.Name), containsMethod, Expression.Constant(item.Value));
                            break;
                        case "Boolean":
                            exp = Expression.Equal(Expression.Property(param, item.Name), Expression.Constant(item.Value));
                            break;
                        case "Byte":
                        case "Double":
                        case "Int32":
                        case "Int64":
                            if (((int)item.Value) != 0)
                                exp = Expression.Equal(Expression.Property(param, item.Name), Expression.Constant(item.Value));
                            break;
                        case "DateTime":
                            break;
                    }

                    if (filterExpression == null)
                        filterExpression = exp;
                    else
                        filterExpression = Expression.AndAlso(filterExpression, exp);
                }
                var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, param);
                return lambda;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the proporty information.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        private static List<FieldInformation> GetProportyInfo(string filter, out List<FieldInformation> PagerInfo)
        {
            List<FieldInformation> list = new List<FieldInformation>();
            List<FieldInformation> pageList = new List<FieldInformation>();
            filter = filter.Replace("~and~", "&");// Replace & 
            var q = filter.Split('&'); // split
            foreach (var item in q)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string _operator = item.Split('~', '~')[1]; // split ~
                    string manipulateString = item.Replace("~", "").Replace(_operator, "=").Replace("'", "");
                    string _propName = manipulateString.Split('=').FirstOrDefault();
                    string _value = manipulateString.Split('=').LastOrDefault();
                    PropertyInfo prop = typeof(T).GetProperty(_propName);
                    // set field
                    FieldInformation field = new FieldInformation();
                    field.Name = _propName;
                    field.Operator = _operator;
                    field.PropertyInfo = prop;
                    field.Value = _value;
                    if (item.Contains("PageSize~") || item.Contains("PageNumber~") || item.Contains("SortBy~") || item.Contains("SortOrder~"))
                    {
                        pageList.Add(field);
                    }
                    else if (prop != null)
                    {
                        list.Add(field);
                    }
                }
            }
            PagerInfo = pageList;
            return list;
        }
       
        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="FieldInfo">The field information.</param>
        /// <returns></returns>
        private static dynamic SetProperty(FieldInformation FieldInfo)
        {

            if (FieldInfo.PropertyInfo != null)
            {
                string PropertyType = FieldInfo.PropertyInfo.PropertyType.Name;
                var propertyType = FieldInfo.PropertyInfo.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && propertyType.GetGenericArguments().Length > 0)
                {
                    PropertyType = Nullable.GetUnderlyingType(propertyType).Name;
                }
                switch (PropertyType)
                {
                    case "Boolean":
                        return Convert.ToBoolean(FieldInfo.Value);
                    case "Byte":
                        return Convert.ToByte(FieldInfo.Value);
                    case "Char":
                        return Convert.ToChar(FieldInfo.Value);
                    case "DateTime":
                        System.Globalization.CultureInfo enGB = new System.Globalization.CultureInfo("en-GB");
                        return Convert.ToDateTime(FieldInfo.Value, enGB);
                    case "Double":
                        return Convert.ToDouble(FieldInfo.Value);
                    case "Int32":
                        return Convert.ToInt32(FieldInfo.Value);
                    case "Int64":
                        return Convert.ToInt64(FieldInfo.Value);
                    default:
                        return Convert.ToString(FieldInfo.Value);
                }
            }
            else
                return null;
        }
        
        #endregion
    }
}
