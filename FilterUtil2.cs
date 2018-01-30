using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TestFilter
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
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        /// <summary>
        /// 
        /// </summary>
        private static MethodInfo ToLowerMethod = typeof(String).GetMethod("ToLower", Type.EmptyTypes);

        /// <summary>
        /// 
        /// </summary>
        private static MethodInfo IndexOfMethod = typeof(string).GetMethod("IndexOf", new[] { typeof(string), typeof(StringComparison) });
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
        public static Expression<Func<T, bool>> FilterByCode<T>(List<int> codes, string FieldName)
        {
            var methodInfo = typeof(List<int>).GetMethod("Contains", new Type[] { typeof(int) });
            var list = Expression.Constant(codes);
            var param = Expression.Parameter(typeof(T), "t");
            var value = Expression.Property(param, FieldName);
            var body = Expression.Call(list, methodInfo, value);

            // j => codes.Contains(j.Code)
            return Expression.Lambda<Func<T, bool>>(body, param);
        }
        /// <summary>
        /// Where IN clause
        /// </summary>
        /// <param name="param"></param>
        /// <param name="FieldInfo"></param>
        /// <returns></returns>
        private static Expression GetInExpression(ParameterExpression param, FieldInformation FieldInfo)
        {
            try
            {
                List<int> numbersArrary = FieldInfo.Value.Split(',').Select(n => Convert.ToInt32(n)).ToList();
                var methodInfo = typeof(List<int>).GetMethod("Contains", new Type[] { typeof(int) });
                var list = Expression.Constant(numbersArrary);
                var value = Expression.Property(param, $"{FieldInfo.Name}Id");
                var expr = Expression.Call(list, methodInfo, value);
                return expr;
            }
            catch (Exception ex)
            {
                HicomLogger.LogError(ex);
                return null;
            }
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
                    // For Date compare
                    if (FieldInfo.PropertyInfo.PropertyType == typeof(DateTime))
                    {
                        constant = Expression.Constant(Convert.ToDateTime(constant).Date, typeof(DateTime));
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
                            var dynamicExpression = Expression.Call(member, ToLowerMethod);
                            var valueToLower = ((string)value).ToLower(); //FieldInfo.Value.ToLower();
                            return Expression.Call(dynamicExpression, containsMethod, Expression.Constant(valueToLower));
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
                    foreach (var fieldInfo in list.Where(q=>q.Operator != "multiselect"))
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
                    //
                    foreach (var fieldInfo in list.Where(q => q.Operator == "multiselect"))
                    {
                        var exp = GetInExpression(param, fieldInfo);
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
                    string manipulateString = item.Replace($"~{_operator}~", "=").Replace("'", "");
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
                    //else if(_operator == "multiselect")
                    //{

                    //}
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
