using MudBlazor;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorCRM.Client.Components
{
    public partial class CrmDataGrid<T>
    {
        [Parameter]
        public required int PageSize { get; set; }

        [Parameter]
        public required List<T> Items { get; set; }

        [Parameter]
        public required string? TableName { get; set; }

        [Parameter]
        public required IQueryable<T> Queryable { get; set; }

        [Parameter]
        public required List<string> DisplayProperties { get; set; }

        private string? _searchString;
        private MudDataGrid<T>? _dataGrid;
        private readonly Dictionary<string, Expression<Func<T, object>>> _propertyExpressions = [];
        private bool _isRenderModeServer;

        protected override void OnInitialized()
        {
            foreach (var display in DisplayProperties)
            {
                _propertyExpressions[display] = CrmDataGrid<T>.GetPropertyExpression(display);
            }
            _isRenderModeServer = !OperatingSystem.IsBrowser();
        }
        private async Task<GridData<T>> ServerReload(GridState<T> state)
        {
            var httpClient = new HttpClient();
            // Simulate API Call
            IEnumerable<T>? data = Items;
            await Task.Delay(1000);
            if (!string.IsNullOrWhiteSpace(_searchString) && data != null)
                data = data.Where(d =>
                {
                    foreach (var propertyName in DisplayProperties)
                    {
                        var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                        if (propertyInfo != null && propertyInfo.PropertyType == typeof(string))
                        {
                            var test = propertyInfo.GetValue(d);
                            if (propertyInfo.GetValue(d) is string strVal && strVal.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                        if (propertyInfo != null && propertyInfo.PropertyType == typeof(DateTime))
                        {
                            var test = propertyInfo.GetValue(d);
                            if (propertyInfo.GetValue(d) is DateTime dateVal && dateVal.Year == Convert.ToInt32(_searchString.Substring(6, 4)))
                                return true;
                        }
                    }
                    return false;
                }).ToArray();
            var totalItems = data?.Count() ?? 0;

            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition != null && !string.IsNullOrWhiteSpace(sortDefinition.SortBy))
            {
                var propertyInfo = typeof(T).GetProperty(sortDefinition.SortBy);
                if (propertyInfo != null && data != null)
                {
                    data = data.OrderByDirection(
                        sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                        x => propertyInfo.GetValue(x, null));
                }
            }

            var pagedData = data?.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new GridData<T>
            {
                TotalItems = totalItems,
                Items = [.. pagedData]
            };
        }
        private async Task<GridData<T>> ServerReloadQueryable(GridState<T> state)
        {
            IQueryable<T>? query = Queryable;
            await Task.Delay(1000);
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                foreach (var propertyName in DisplayProperties)
                {
                    var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfo != null && propertyInfo.PropertyType == typeof(string))
                    {
                        var parameter = Expression.Parameter(typeof(T), "d");
                        var property = Expression.Property(parameter, propertyInfo);
                        var searchValue = Expression.Constant(_searchString, typeof(string));
                        var containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
                        var containsExpression = Expression.Call(property, containsMethod, searchValue);
                        var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

                        query = query.Where(lambda);
                    }
                }
            }

            if (_isRenderModeServer)
            {
                // Server Rendered use service
            }
            else
            {
                // WASM use API call
                var httpClient = new HttpClient();
            }

            var totalItems = query.Count();
            var pagedData = query?.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new GridData<T>
            {
                TotalItems = totalItems,
                Items = [.. pagedData]
            };
        }
        private Task OnSearch(string text)
        {
            _searchString = text;
            return _dataGrid.ReloadServerData();
        }
        private static Expression<Func<T, object>> GetPropertyExpression(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var converted = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(converted, parameter);
        }
        private static string GetDisplayName(string name)
        {
            if (string.Equals(name, "cedealid", StringComparison.OrdinalIgnoreCase))
                return "CE Deal Id";
            StringBuilder sb = new();
            foreach (var item in name)
            {
                if (char.IsUpper(item))
                    sb.Append(' ');
                sb.Append(item);
            }
            return sb.ToString();
        }
    }
}
