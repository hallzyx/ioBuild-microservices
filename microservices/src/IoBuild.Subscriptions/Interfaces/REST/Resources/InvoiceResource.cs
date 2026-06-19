namespace IoBuild.Subscriptions.Interfaces.REST.Resources;

public record InvoiceResource(
    string Date,
    decimal Amount,
    string Currency,
    string? ReceiptUrl,
    string Description,
    string Status
);
