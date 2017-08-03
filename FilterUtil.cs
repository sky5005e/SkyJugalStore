https://localhost:44331/api/UserGroup/Filters?Filter=Name~eq~'Somya'~and~GroupId~gt~2&SortBy='o'

https://localhost:44331/api/UserGroup/Filters?Filter=Name~eq~'Administrator'~and~GroupId~gt~2&SortBy='o'
 
 string q1 = @"?FilterBy=Name~eq~'Somya'~and~GroupId~gt~2&SortBy=o";
             var t1 = await Utility.GetResultAsync<List<Group>>($"UserGroup/Filters{q1}");
            string q2 = @"?FilterBy=Name~ct~'a'~and~GroupId~gt~2&SortBy=o";

		[Route("Filters")]
        [HttpGet]
        public IHttpActionResult Filters(string FilterBy, string SortBy)
        {
            Expression<Func<Group, bool>> exp = FilterUtil<Group>.GetFilterExpression(FilterBy, SortBy);
            var list = unitOfWork.Repository<Group>().GetManyQueryable(exp);
            return Ok(list);
        }
		
		/// <summary>
        /// generic method to get many record on the basis of a condition but query able.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual List<TEntity> GetManyQueryable(Expression<Func<TEntity, bool>> where)
        {
            return dbSet.Where(where).ToList();
        }
		
		
		
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace Hicom.Core.API.Common
{
    public class FilterUtil<T> where T : class
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains");
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        public static Expression<Func<T, bool>> GetFilterExpression(string filter, string sortby)
        {
            if (string.IsNullOrEmpty(filter))
                return null;
            else
            {
                ParameterExpression param = Expression.Parameter(typeof(T), "t");
                Expression exp = null;
                List<FieldInformation> list = GetProportyInfo(filter);
                foreach (var fieldInfo in list)
                {
                    if (exp == null)
                        exp = GetExpression(param, fieldInfo);
                    else
                        exp = Expression.AndAlso(exp, GetExpression(param, fieldInfo));

                }
                return Expression.Lambda<Func<T, bool>>(exp, param);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static List<FieldInformation>  GetProportyInfo(string filter)
        {
            List<FieldInformation> list = new List<FieldInformation>();
            filter = filter.Replace("~and~", "&");// Replace & 
            var q = filter.Split('&'); // split
            foreach (var item in q)
            {
                string _operator = item.Split('~', '~')[1]; // split ~
                string manipulateString = item.Replace("~", "").Replace(_operator, "=").Replace("'", "");
                string _propName = manipulateString.Split('=').FirstOrDefault();
                string _value = manipulateString.Split('=').LastOrDefault();

                PropertyInfo prop = typeof(T).GetProperty(_propName);
                if (prop != null)
                {
                    FieldInformation field = new FieldInformation();
                    field.Name = _propName;
                    field.Operator = _operator;
                    field.PropertyInfo = prop;
                    field.Value = _value;
                    list.Add(field);
                }
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="param"></param>
        /// <param name="fieldInfo"></param>
        private static void BuildExpressionFilter(Expression exp, ParameterExpression param, FieldInformation fieldInfo)
        {
            if (exp == null)
                exp = GetExpression(param, fieldInfo);
            else
                exp = Expression.AndAlso(exp, GetExpression(param, fieldInfo));
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="FieldInfo"></param>
        /// <returns></returns>
        private static Expression GetExpression(ParameterExpression param, FieldInformation FieldInfo)
        {
            try
            {


                MemberExpression member = Expression.Property(param, FieldInfo.Name);

                dynamic value = SetProperty(FieldInfo);
                ConstantExpression constant = Expression.Constant(value);
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

                    case "ct":
                        return Expression.Call(member, containsMethod, constant);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        private static dynamic SetProperty(FieldInformation FieldInfo)
        {
            
            if (FieldInfo.PropertyInfo != null)
            {
                string PropertyType = FieldInfo.PropertyInfo.PropertyType.Name;
                switch (PropertyType)
                {
                    case "Int32":
                       return Convert.ToInt32(FieldInfo.Value);
                       
                    case "Int64":
                        return Convert.ToInt64(FieldInfo.Value);
                       
                    case "String":
                        return Convert.ToString(FieldInfo.Value);
                       
                    case "Byte":
                        return Convert.ToByte(FieldInfo.Value);
                       
                    case "Boolean":
                        return Convert.ToBoolean(FieldInfo.Value);
                       
                    case "DateTime":
                        return Convert.ToDateTime(FieldInfo.Value);
                       
                    default:
                        return Convert.ToSingle(FieldInfo.Value);
                      

                }
                
            }
            else
            {
                return null;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class FieldInformation
    {
        public string Name { get; set; }
        public string Operator { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public string Value { get; set; }
    }
}
