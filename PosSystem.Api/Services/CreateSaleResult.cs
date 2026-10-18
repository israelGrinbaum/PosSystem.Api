using System.Diagnostics.CodeAnalysis;
using PosSystem.Api.Models;

namespace PosSystem.Api.Services
{
    public sealed class CreateSaleResult
    {
        public CreateSaleStatus Status { get; }
        public Sale? Sale { get; }
        public string? ErrorDetail { get; }
        public string? ErrorField { get; }

        [MemberNotNullWhen(true, nameof(Sale))]
        [MemberNotNullWhen(false, nameof(ErrorDetail))]
        public bool IsSuccess => Status is CreateSaleStatus.Created or CreateSaleStatus.AlreadyExists;

        private CreateSaleResult(CreateSaleStatus status, Sale? sale, string? errorDetail, string? errorField)
        {
            Status = status;
            Sale = sale;
            ErrorDetail = errorDetail;
            ErrorField = errorField;
        }

        public static CreateSaleResult Created(Sale sale) =>
            new(CreateSaleStatus.Created, sale, null, null);

        public static CreateSaleResult AlreadyExists(Sale sale) =>
            new(CreateSaleStatus.AlreadyExists, sale, null, null);

        public static CreateSaleResult NotFound(string detail) =>
            new(CreateSaleStatus.NotFound, null, detail, null);

        public static CreateSaleResult Conflict(string detail) =>
            new(CreateSaleStatus.Conflict, null, detail, null);

        public static CreateSaleResult Validation(string detail, string? field = null) =>
            new(CreateSaleStatus.Validation, null, detail, field);
    }
}
