using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace LaunchPanel
{
    public static class ExtensionMethods
    {
        public static bool IsNotEmpty(this string src)
        {
            return !String.IsNullOrWhiteSpace(src);
        }

        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }

        public static int IndexOfFirst<T>(this IEnumerable<T> items, Func<T, bool> condition)
        {
            int idx = -1;

            foreach (T item in items)
            {
                ++idx;

                if (condition(item))
                {
                    break;
                }
            }

            return idx;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static void ForEachWithIndex<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            int n = 0;

            foreach (var item in collection)
            {
                action(item, n++);
            }
        }

        public static void ForEach(this int n, Action<int> action)
        {
            for (int i = 0; i < n; i++)
            {
                action(i);
            }
        }

        public static void ForEach(this (int a, int b) d2, Action<int, int> action)
        {
            for (int i = 0; i < d2.a; i++)
            {
                for (int j = 0; j < d2.b; j++)
                {
                    action(i, j);
                }
            }
        }

        public static void Match<T>(this T val, params (Func<T, bool> qualifier, Action<T> action)[] matches)
        {
            foreach (var match in matches)
            {
                if (match.qualifier(val))
                {
                    match.action(val);
                    break;
                }
            }
        }

        public static void MatchAll<T>(this T val, params (Func<T, bool> qualifier, Action<T> action)[] matches)
        {
            foreach (var match in matches)
            {
                if (match.qualifier(val))
                {
                    match.action(val);
                }
            }
        }

        public static U MatchReturn<T, U>(this T val, params (Func<T, bool> qualifier, Func<T, U> func)[] matches)
        {
            U ret = default(U);

            foreach (var match in matches)
            {
                if (match.qualifier(val))
                {
                    ret = match.func(val);
                    break;
                }
            }

            return ret;
        }

        public static U MatchReturn<T, U>(this T val, params (Func<T, bool> qualifier, Func<U> func)[] matches)
        {
            U ret = default(U);

            foreach (var match in matches)
            {
                if (match.qualifier(val))
                {
                    ret = match.func();
                    break;
                }
            }

            return ret;
        }

        public static U ReverseMatchReturn<T, U>(this T val, params (Func<T, bool> qualifier, Func<T, U> func)[] matches)
        {
            U ret = default(U);

            for (int n = matches.Length - 1; n >= 0; n--)
            {
                var match = matches[n];

                if (match.qualifier(val))
                {
                    ret = match.func(val);
                    break;
                }
            }

            return ret;
        }

        // https://stackoverflow.com/a/14548027/2276361
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            DataTable table = new DataTable();

            //special handling for value types and string
            if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)))
            {

                DataColumn dc = new DataColumn("Value");
                table.Columns.Add(dc);
                foreach (T item in data)
                {
                    DataRow dr = table.NewRow();
                    dr[0] = item;
                    table.Rows.Add(dr);
                }
            }
            else
            {
                PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));

                foreach (PropertyDescriptor prop in properties)
                {
                    if (prop.PropertyType == typeof(string) || prop.PropertyType.IsValueType)
                    {
                        Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        if (t.IsEnum)
                        {
                            t = typeof(string);
                        }

                        table.Columns.Add(prop.Name, t);
                    }
                }

                foreach (T item in data)
                {
                    DataRow row = table.NewRow();

                    foreach (PropertyDescriptor prop in properties)
                    {
                        Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        if (prop.PropertyType == typeof(string) || prop.PropertyType.IsValueType)
                        {
                            if (t.IsEnum)
                            {
                                row[prop.Name] = prop.GetValue(item).ToString();
                            }
                            else
                            {
                                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                            }
                        }
                    }

                    table.Rows.Add(row);
                }
            }

            return table;
        }
    }
}
