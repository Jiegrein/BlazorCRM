using MudBlazor;
using System.Text;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorCRM.Client.Pages
{
    public partial class Deal
    {
        private List<DealReadDto>? Deals;
        private MudDataGrid<DealReadDto>? _dataGrid;
        private string? _searchString;
        private readonly List<string> _displayList =
        [
            nameof(DealReadDto.CEDealId),
            nameof(DealReadDto.BusinessName),
            nameof(DealReadDto.ApplicantName),
            nameof(DealReadDto.RequestedLoanAmount),
            nameof(DealReadDto.SalesRep),
            nameof(DealReadDto.DataAssociate),
            nameof(DealReadDto.Stage),
            nameof(DealReadDto.Status),
            nameof(DealReadDto.CreatedTime),
            nameof(DealReadDto.LeadSource),
        ];
        private bool _isRenderModeServer;

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(500);
            _isRenderModeServer = !OperatingSystem.IsBrowser();
            if (_isRenderModeServer)
                // Server Rendered use service
                Deals = Enumerable.Range(1, 100).Select(index => new DealReadDto()
                {
                    CEDealId = (Random.Shared.Next() + index).ToString(),
                    BusinessName = (Random.Shared.Next() + index).ToString(),
                    ApplicantName = (Random.Shared.Next() + index).ToString(),
                    RequestedLoanAmount = (Random.Shared.Next() + index).ToString(),
                    SalesRep = (Random.Shared.Next() + index).ToString(),
                    DataAssociate = (Random.Shared.Next() + index).ToString(),
                    Stage = (Random.Shared.Next() + index).ToString(),
                    Status = (Random.Shared.Next() + index).ToString(),
                    CreatedTime = RandomDay(),
                    LeadSource = (Random.Shared.Next() + index).ToString(),
                }).ToList();
            else
                // WASM use API call
                Deals = Enumerable.Range(1, 100).Select(index => new DealReadDto()
                {
                    CEDealId = (Random.Shared.Next() + index).ToString(),
                    BusinessName = (Random.Shared.Next() + index).ToString(),
                    ApplicantName = (Random.Shared.Next() + index).ToString(),
                    RequestedLoanAmount = (Random.Shared.Next() + index).ToString(),
                    SalesRep = (Random.Shared.Next() + index).ToString(),
                    DataAssociate = (Random.Shared.Next() + index).ToString(),
                    Stage = (Random.Shared.Next() + index).ToString(),
                    Status = (Random.Shared.Next() + index).ToString(),
                    CreatedTime = RandomDay(),
                    LeadSource = (Random.Shared.Next() + index).ToString(),
                }).ToList();
        }
        private async Task<GridData<DealReadDto>> ServerReload(GridState<DealReadDto> state)
        {
            var httpClient = new HttpClient();
            // Simulate API Call
            IEnumerable<DealReadDto> data = Deals;
            await Task.Delay(1000);
            data = data.Where(DealReadDto =>
            {
                if (string.IsNullOrWhiteSpace(_searchString))
                    return true;
                if (DealReadDto.CEDealId != null && DealReadDto.CEDealId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (DealReadDto.BusinessName != null && DealReadDto.BusinessName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (DealReadDto.ApplicantName != null && DealReadDto.ApplicantName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }).ToArray();
            var totalItems = data.Count();

            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition != null)
            {
                switch (sortDefinition.SortBy)
                {
                    case nameof(DealReadDto.CEDealId):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.CEDealId
                        );
                        break;
                    case nameof(DealReadDto.BusinessName):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.BusinessName
                        );
                        break;
                    case nameof(DealReadDto.ApplicantName):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.ApplicantName
                        );
                        break;
                    case nameof(DealReadDto.RequestedLoanAmount):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.RequestedLoanAmount
                        );
                        break;
                    case nameof(DealReadDto.SalesRep):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.SalesRep
                        );
                        break;
                }
            }

            var pagedData = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new GridData<DealReadDto>
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }
        private static string GetDisplayName(string name)
        {
            if (name == nameof(DealReadDto.CEDealId))
                return "CE Deal Id";
            StringBuilder sb = new();
            foreach (var item in name)
            {
                if(char.IsUpper(item))
                    sb.Append(' ');
                sb.Append(item);
            }
            return sb.ToString();
        }

        public class DealReadDto()
        {
            public required string CEDealId { get; set; }
            public required string BusinessName { get; set; }
            public required string ApplicantName { get; set; }
            public required string RequestedLoanAmount { get; set; }
            public required string SalesRep { get; set; }
            public required string DataAssociate { get; set; }
            public required string Stage { get; set; }
            public required string Status { get; set; }
            public required DateTime CreatedTime { get; set; }
            public required string LeadSource { get; set; }
        }

        private static DateTime RandomDay()
        {
            Random gen = new();
            DateTime start = new(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }
        private Task OnSearch(string text)
        {
            _searchString = text;
            return _dataGrid.ReloadServerData();
        }
    }
}